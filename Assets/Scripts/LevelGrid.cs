using Jelly;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private GameObject gridTileHightlighterPrefab;
    [SerializeField] private TextAsset layoutFile;
    [SerializeField] private float tileSize = 1.0f;
    private Dictionary<Vector2Int, Transform> coordsToGridTileTransforms;


    private Jelly.Grid grid;
    private GameObject hightlighter;
    private bool isGridFull = false;
    private bool isGridResolving = false;

    public bool IsGridFull { get { return isGridFull; } }

    public bool IsGridResolving { get { return isGridResolving; } }

    void Start()
    {
        if (gridTilePrefab == null || layoutFile == null)
        {
            return;
        }

        grid = new Jelly.Grid(layoutFile);
        coordsToGridTileTransforms = new Dictionary<Vector2Int, Transform>();

        // Create grid tiles & blocks
        for (int row = 0; row < grid.Rows; row++)
        {
            for (int col = 0; col < grid.Columns; col++)
            {
                int cell = grid.GetCell(row, col);

                if (cell == (int) Jelly.CellMarker.Obstacle)
                {
                    // Skip obstacles
                    continue;
                }
                // Create a grid tile
                GameObject gridTileGameObj = Instantiate(gridTilePrefab, transform);
                // Set position, make them center
                gridTileGameObj.transform.localScale = Vector3.one * (tileSize - 0.1f);
                gridTileGameObj.transform.localPosition = GetPositionForCell(row, col);
                coordsToGridTileTransforms[new Vector2Int(row, col)] = gridTileGameObj.transform;

                // Create block if the cell is not empty
                if (cell != (int) Jelly.CellMarker.Empty)
                {
                    Jelly.Block block = grid.GetBlock(row, col);
                    CreateLevelBlock(gridTileGameObj.transform, block);
                }
            }
        }


        // Create hightlighter then hide it
        hightlighter = Instantiate(gridTileHightlighterPrefab, transform);
        hightlighter.SetActive(false);
    }


    // .x for row index; .y for column index
    public Vector2Int FindNearbyEmptyCell(Vector3 fromPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(fromPosition);

        float rowFloat = -(localPos.y / tileSize) + grid.Rows / 2f - 0.5f;
        float colFloat = (localPos.x / tileSize) + grid.Columns / 2f - 0.5f;

        int row = Mathf.RoundToInt(rowFloat);
        int col = Mathf.RoundToInt(colFloat);

        if (grid.GetCell(row, col) != (int) Jelly.CellMarker.Empty)
        {
            return new Vector2Int(-1, -1);
        }

        if (row < 0 || row >= grid.Rows || col < 0 || col >= grid.Columns)
        {
            return new Vector2Int(-1, -1);
        }

        return new Vector2Int(row, col);
    }

    public Vector3 GetPositionForCell(int row, int col)
    {
        if (row < 0 || row >= grid.Rows || col < 0 || col >= grid.Columns)
        {
            return Vector3.zero;
        }
        Vector3 normalizedPosition = new Vector3(col - grid.Columns / 2f + 0.5f, -(row - grid.Rows / 2f + 0.5f), 0.0f);
        return normalizedPosition * tileSize;
    }

    public void SetHightlightOnCell(int row, int col)
    {
        if (hightlighter == null)
        {
            return;
        }

        if (grid.GetCell(row, col) == (int) Jelly.CellMarker.Obstacle)
        {
            hightlighter.SetActive(false);
            return;
        }

        hightlighter.SetActive(true);
        Vector3 localPos = GetPositionForCell(row, col);
        hightlighter.transform.localPosition = localPos + Vector3.back * 0.1f;
    }

    public void UnSetHightlight()
    {
        if (hightlighter == null)
        {
            return;
        }

        hightlighter.SetActive(false);
    }

    public void InsertBlock(int row, int column, Jelly.Block block)
    {
        if (isGridFull)
        {
            return;
        }

        if (grid.TryInsertBlock(row, column, block))
        {
            Transform gridTile = coordsToGridTileTransforms[new Vector2Int(row, column)];
            CreateLevelBlock(gridTile, block);
            StartCoroutine(ResolveCoroutine());
        }
    }

    public void InsertBlock(int row, int column, LevelBlock levelBlock)
    {
        if (isGridFull)
        {
            return;
        }

        if (grid.TryInsertBlock(row, column, levelBlock.Block))
        {
            Transform gridTile = coordsToGridTileTransforms[new Vector2Int(row, column)];
            levelBlock.transform.parent = gridTile;
            levelBlock.transform.localPosition = Vector3.zero;
            levelBlock.transform.localScale = Vector3.one;
            StartCoroutine(ResolveCoroutine());
        }

    }

    private IEnumerator ResolveCoroutine()
    {
        int combo = 0;
        bool isFullyResolved = false;
        isGridResolving = true;

        while (!isGridFull && !isFullyResolved)
        {
            Jelly.GridResolveData resolveData = grid.Resolve();
            isFullyResolved = resolveData.RemovedChunks.Length == 0;
            if (isFullyResolved)
                break;

            combo++;
            isGridFull = grid.IsGridFull();
            GameSingleton.instance.AddProgress(resolveData.RemovedChunks);

            yield return new WaitForSeconds(0.55f);
        }

        isGridResolving = false;

        if (combo > 0)
            GameSingleton.instance.NotifyCombo(combo);
    }

    private void CreateLevelBlock(Transform gridTile, Jelly.Block block)
    {
        Jelly.BlockVariant blockVariant = block.Variant;
        LevelBlock prefab = GameSingleton.instance.GetLevelBlockBasePrefab(blockVariant);
        LevelBlock levelBlock = Instantiate(prefab, gridTile.transform);
        levelBlock.Initialize(block);
    }
}

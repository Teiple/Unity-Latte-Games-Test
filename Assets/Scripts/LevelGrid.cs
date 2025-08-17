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
    // The link between the grid occupiable cells and the grid tile transforms
    private Dictionary<Vector2Int, Transform> gridTileTransforms = new Dictionary<Vector2Int, Transform>();


    private Jelly.Grid grid;
    private GameObject hightlighter;

    void Start()
    {
        if (gridTilePrefab == null || layoutFile == null)
        {
            return;
        }

        grid = new Jelly.Grid(layoutFile);

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
                gridTileTransforms[new Vector2Int(row, col)] = gridTileGameObj.transform;

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

    void Update()
    {
        
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
        if (grid.TryInsertBlock(row, column, block))
        {
            Transform gridTile = gridTileTransforms[new Vector2Int(row, column)];
            CreateLevelBlock(gridTile, block);
        }
    }


    private void CreateLevelBlock(Transform gridTile, Jelly.Block block)
    {
        Jelly.BlockVariant blockVariant = block.Variant;
        GameObject prefab = GameSingleton.instance.GetBlockVariantPrefab(blockVariant);
        GameObject levelBlockGameObj = Instantiate(prefab, gridTile.transform);
        levelBlockGameObj.transform.localScale = Vector3.one * (tileSize - 0.1f);
        LevelBlock levelBlock = levelBlockGameObj.GetComponent<LevelBlock>();
        levelBlock.Initialize(block);
    }
}

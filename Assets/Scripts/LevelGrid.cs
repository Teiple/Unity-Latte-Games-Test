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
                GameObject gridTile = Instantiate(gridTilePrefab, transform);
                // Set position, make them center
                gridTile.transform.localScale = Vector3.one * (tileSize - 0.1f);
                gridTile.transform.localPosition = GetPositionForCell(row, col);

                // Create block if the cell is not empty
                if (cell != (int) Jelly.CellMarker.Empty)
                {
                    Jelly.Block block = grid.GetBlock(row, col);
                    Jelly.BlockVariant blockVariant = block.Variant;
                    GameObject prefab = GameSingleton.instance.GetBlockVariantPrefab(blockVariant);
                    GameObject levelBlockGameObj = Instantiate(prefab, gridTile.transform);
                    levelBlockGameObj.transform.localScale = Vector3.one * (tileSize - 0.1f);
                    LevelBlock levelBlock = levelBlockGameObj.GetComponent<LevelBlock>();
                    levelBlock.Initialize(block);
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
    public Vector2Int FindClosestCell(Vector3 fromPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(fromPosition);

        float rowFloat = -(localPos.y / tileSize) + grid.Rows / 2f - 0.5f;
        float colFloat = (localPos.x / tileSize) + grid.Columns / 2f - 0.5f;

        int row = Mathf.RoundToInt(rowFloat);
        int col = Mathf.RoundToInt(colFloat);

        row = Mathf.Clamp(row, 0, grid.Rows - 1);
        col = Mathf.Clamp(col, 0, grid.Columns - 1);

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
}

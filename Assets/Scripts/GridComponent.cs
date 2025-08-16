using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{
    [SerializeField] private GameObject gridTile = null;
    [SerializeField] private TextAsset gridFile = null;
    
    private int rows = 1;
    private int columns = 1;
    private bool[,] grid;

    // Start is called before the first frame update
    void Start()
    {
        if (!gridTile)
        {
            Debug.LogError("Grid tile prefab is not assigned.");
            return;
        }
        if (!gridFile)
        {
            Debug.LogError("Grid file is not assigned.");
            return;
        }

        string[] delimiters = new[] { "\n", "\r\n", "\t" };
        char[] charDelimiters = new[] { '\n', '\r', '\n', '\t' };
        StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries;
        string[] lines = gridFile.text.Split(delimiters, splitOptions);

        rows = lines.Length;
        if (rows == 0)
        {
            Debug.LogError("Grid file is empty.");
            return;
        }
        columns = lines[0].Length;

        grid = new bool[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            string line = lines[i].TrimEnd(charDelimiters);
            if (line.Length != columns)
            {
                Debug.LogError($"Row {i} has length {line.Length}, expected {columns}.");
                return;
            }
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = line[j] == '*';
                if (grid[i, j])
                {
                    // Offset all tiles to center the grid
                    Vector3 tilePosition = new Vector3(i - rows / 2f, j - columns / 2f, 0f);
                    GameObject tile = Instantiate(gridTile, tilePosition, Quaternion.identity, transform);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using Jelly;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private float offsetAmount = 1.0f;
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointCenter;
    [SerializeField] private Transform spawnPointRight;
    [SerializeField] private GameObject dropInBlockPrefab;
    [SerializeField] private TextAsset dropSequenceFile;
    [SerializeField] private int spawnPointsCount = 2;

    private DropInBlock currentSelection;
    private Transform currentSpawnPoint;
    private string[] dropSequence; // Drop sequence must ensure the completion of the level in at least one way
    private int currentDropIndex = 0;

    private void Awake()
    {
        char[] charDelimiters = new[] { '\n', '\r' };
        StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries;
        string[] lines = dropSequenceFile.text.Split(charDelimiters, splitOptions);


        if (lines.Length == 0 || lines.Length % 2 != 0 || lines[0].Length != 2)
        {
            return;
        }

        int rows = lines.Length / 2;
        dropSequence = new string[rows];
        // Get a colorCharCodes each two lines
        for (int i = 0; i < rows; i++)
        {
            string colorCharCodes = lines[i * 2].Substring(0, 2) + lines[i * 2 + 1].Substring(0, 2);
            dropSequence[i] = colorCharCodes;
        }
    }

    void Start()
    {
        if (spawnPointsCount == 2)
        {
            currentSpawnPoint = spawnPointLeft;
            SpawnNewBlock();
            currentSpawnPoint = spawnPointRight;
            SpawnNewBlock();
        } else
        {
            currentSpawnPoint = spawnPointCenter;
            SpawnNewBlock();
        }
    }

    void Update()
    {
        if (GameSingleton.instance.GameOver)
        {
            return;
        }

        if (InputSingleton.instance.IsActionPressed("TouchPress"))
        {
            if (currentSelection == null)
            {
                Vector2 currentTouchPosition = InputSingleton.instance.GetActionPosition("TouchPosition");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(currentTouchPosition), out RaycastHit raycastHit))
                {
                    if (raycastHit.collider.gameObject.TryGetComponent(out DropInBlock dropInBlock))
                    {
                        currentSelection = dropInBlock;
                        currentSpawnPoint = dropInBlock.transform.parent;
                    }
                }
            }
        }
        else
        {
            if (currentSelection != null)
            {
                OnBlockDropped(currentSelection);
                currentSelection = null;
                currentSpawnPoint = null;
            }
        }

        if (currentSelection != null)
        {
            Vector2 currentTouchPosition = InputSingleton.instance.GetActionPosition("TouchPosition");

            float localZ = Camera.main.transform.InverseTransformPoint(transform.position).z;
            Vector3 worldTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, currentTouchPosition.y, localZ));
            worldTouchPosition.z = transform.position.z;

            Vector3 targetPosition = worldTouchPosition + Vector3.up * offsetAmount;
            currentSelection.SetControlledTargetPosition(targetPosition);

            OnBlockDragged(targetPosition);
        }
    }


    private void OnBlockDropped(DropInBlock current)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;
        Vector2Int cell = levelGrid.FindNearbyEmptyCell(current.transform.position);
       
        if (cell.x < 0 || cell.y < 0)
        {
            return;
        }

        levelGrid.UnSetHightlight();

        // Transfer block data & level block
        // (visualized game object of the block)
        // to the grid
        levelGrid.InsertBlock(cell.x, cell.y, current.RepresentLevelBlock);
        Destroy(current.gameObject);

        SpawnNewBlock();
    }

    private void OnBlockDragged(Vector3 dragPosition)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;

        Vector2Int cell = levelGrid.FindNearbyEmptyCell(dragPosition);
        
        if (cell.x < 0 || cell.y < 0)
        {
            levelGrid.UnSetHightlight();
            return;
        }

        levelGrid.SetHightlightOnCell(cell.x, cell.y);
    }

    private void SpawnNewBlock()
    {
        if (currentSpawnPoint == null)
        {
            return;
        }
        GameObject newBlock = Instantiate(dropInBlockPrefab, currentSpawnPoint);
        DropInBlock dropInBlock = newBlock.GetComponent<DropInBlock>();
        if (currentDropIndex < 0 || currentDropIndex >= dropSequence.Length)
        {
            // Temp: Reset index if we run out of blocks
            // I think we should add random block generation with fixed seed later
            currentDropIndex = 0;
        }
        if (currentDropIndex < dropSequence.Length)
        {
            string colorCharCodes = dropSequence[currentDropIndex];
            dropInBlock.Initialize(colorCharCodes);
            currentDropIndex++;
        }
    }
}

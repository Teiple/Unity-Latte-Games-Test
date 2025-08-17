using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float offsetAmount = 1.0f;
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;
    [SerializeField] private GameObject dropInBlockPrefab;

    private DropInBlock currentSelection;
    private Transform currentSpawnPoint;

    void Start()
    {
        currentSpawnPoint = spawnPointLeft;
        SpawnNewBlock();
        currentSpawnPoint = spawnPointRight;
        SpawnNewBlock();
    }

    void Update()
    {
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
        // Test, destroy the block
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
        string[] randomCharCodes = { "rrrr", "ggrr", "pkpy", "ggbr" };
        dropInBlock.Initialize(randomCharCodes[Random.Range(0, randomCharCodes.Length)]);
    }
}

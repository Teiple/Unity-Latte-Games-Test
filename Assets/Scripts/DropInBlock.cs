using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DropInBlock : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20.0f;
    private Jelly.Block block;
    private bool isControlled = false;
    private Vector3 targetPosition;


    public Jelly.Block Block
    {
        get { return block; }
    }


    void Update()
    {
        if (isControlled)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        } else
        {
            transform.position = Vector3.Lerp(transform.position, transform.parent.position, Time.deltaTime * moveSpeed);
        }

        isControlled = false;
    }


    public void OnBlockDropped(Vector3 dropPosition)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;

        levelGrid.UnSetHightlight();
    }

    public void OnBlockDragged(Vector3 dragPosition)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;

        Vector2Int cell = levelGrid.FindNearbyEmptyCell(dragPosition);
        levelGrid.SetHightlightOnCell(cell.x, cell.y);
    }
    

    public void Initialize(string colorCharCodes)
    {
        if (colorCharCodes == null || colorCharCodes.Length != 4)
        {
            return;
        }
        
        block = new Block(colorCharCodes);
        Jelly.BlockVariant blockVariant = block.Variant;
        GameObject prefab = GameSingleton.instance.GetBlockVariantPrefab(blockVariant);
        GameObject levelBlockGameObj = Instantiate(prefab, transform);
        LevelBlock levelBlock = levelBlockGameObj.GetComponent<LevelBlock>();
        // Make isNonFunctional true to prevent the block events from being registered
        levelBlock.Initialize(block, true);
    }

    public void SetControlledTargetPosition(Vector3 position)
    {
        isControlled = true;
        targetPosition = position;
    }
}

using Jelly;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DropInBlock : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20.0f;
    private bool isControlled = false;
    private Vector3 targetPosition;
    private LevelBlock representLevelBlock;

    public LevelBlock RepresentLevelBlock { get { return representLevelBlock; } }

    private void Update()
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
        
        Block block = new Block(colorCharCodes);
        Jelly.BlockVariant blockVariant = block.Variant;
        LevelBlock prefab = GameSingleton.instance.GetLevelBlockBasePrefab(blockVariant);
        LevelBlock levelBlock = Instantiate(prefab, transform);
        levelBlock.Initialize(block);
        representLevelBlock = levelBlock;
    }

    public void SetControlledTargetPosition(Vector3 position)
    {
        isControlled = true;
        targetPosition = position;
    }
}

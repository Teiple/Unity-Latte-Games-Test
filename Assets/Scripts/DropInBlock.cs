using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropInBlock : MonoBehaviour
{
    [SerializeField] private float moveAmount = 1.0f;
    [SerializeField] private float moveSpeed = 20.0f;

    private bool isPicked = false;
    private Vector3 initialPosition;


    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (InputSingleton.instance.IsActionPressed("TouchPress")) {
            if (!isPicked)
            {
                Vector2 currentTouchPosition = InputSingleton.instance.GetActionPosition("TouchPosition");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(currentTouchPosition), out RaycastHit raycastHit))
                {
                    isPicked = raycastHit.collider.gameObject == gameObject;
                }
            }
        } else
        {
            if (isPicked)
            {
                OnBlockDropped(transform.position);
            }
            isPicked = false;
        }

        Vector3 targetPosition = initialPosition;
        if (isPicked)
        {
            Vector2 currentTouchPosition = InputSingleton.instance.GetActionPosition("TouchPosition");

            float localZ = Camera.main.transform.InverseTransformPoint(transform.position).z;
            Vector3 worldTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, currentTouchPosition.y, localZ));
            worldTouchPosition.z = transform.position.z;

            targetPosition = worldTouchPosition + Vector3.up * moveAmount;

            OnBlockDragged(targetPosition);
        }
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }


    private void OnBlockDropped(Vector3 dropPosition)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;

        levelGrid.UnSetHightlight();
    }

    private void OnBlockDragged(Vector3 dragPosition)
    {
        LevelGrid levelGrid = GameSingleton.instance.CurrentLevelGrid;

        Vector2Int cell = levelGrid.FindClosestCell(dragPosition);
        levelGrid.SetHightlightOnCell(cell.x, cell.y);
    }

}

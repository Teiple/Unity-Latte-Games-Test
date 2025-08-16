using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPiece : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public float moveSpeed = 2f;

    private bool isPicked = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (InputManagerSingleton.instance.IsActionPressed("TouchPress")) {
            if (!isPicked)
            {
                Vector2 currentTouchPosition = InputManagerSingleton.instance.GetActionPosition("TouchPosition");
                if (Physics.Raycast(Camera.main.ScreenPointToRay(currentTouchPosition), out RaycastHit raycastHit))
                {
                    isPicked = raycastHit.collider.gameObject == gameObject;
                }
            }
        } else
        {
            isPicked = false;
        }

        Vector3 targetPosition = initialPosition;
        if (isPicked)
        {
            Vector2 currentTouchPosition = InputManagerSingleton.instance.GetActionPosition("TouchPosition");

            float localZ = Camera.main.transform.InverseTransformPoint(transform.position).z;
            Vector3 worldTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, currentTouchPosition.y, localZ));
            worldTouchPosition.z = transform.position.z;

            targetPosition = worldTouchPosition + Vector3.up * moveAmount;
        }
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}

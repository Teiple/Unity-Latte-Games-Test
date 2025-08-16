using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPiece : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public float moveSpeed = 2f;

    private bool isTouching = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        isTouching = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log($"Touch detected: Phase={touch.phase}, Position={touch.position}");

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == transform)
                    {
                        isTouching = true;
                    }
                }
            }
        }

        // Move cube up while touching, return to original position otherwise
        Vector3 targetPos = isTouching
            ? initialPosition + Vector3.up * moveAmount
            : initialPosition;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }
}

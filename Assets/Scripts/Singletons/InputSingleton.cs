using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSingleton : MonoBehaviour
{
    public static InputSingleton instance = null;
    
    private PlayerInput playerInput = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

       playerInput = GetComponent<PlayerInput>();
    }

    public bool IsActionPerformed(string action)
    {
        InputAction inputAction = playerInput.actions.FindAction(action);
        if (inputAction == null)
        {
            return false;
        }
        return inputAction.WasPerformedThisFrame();
    }

    public bool IsActionJustPressed(string action)
    {
        InputAction inputAction = playerInput.actions.FindAction(action);
        if (inputAction == null)
        {
            return false;
        }
        return inputAction.WasPressedThisFrame();
    }

    public bool IsActionJustReleased(string action)
    {
        InputAction inputAction = playerInput.actions.FindAction(action);
        if (inputAction == null)
        {
            return false;
        }
        return inputAction.WasReleasedThisFrame();
    }

    public bool IsActionPressed(string action)
    {
        InputAction inputAction = playerInput.actions.FindAction(action);
        if (inputAction == null)
        {
            return false;
        }
        return inputAction.ReadValue<float>() > 0.0001f;
    }


    public Vector2 GetActionPosition(string action)
    {
        InputAction inputAction = playerInput.actions.FindAction(action);
        if (inputAction == null)
        {
            return Vector2.zero;
        }
        return inputAction.ReadValue<Vector2>();
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputActions inputActions;

    public event EventHandler OnClickAction;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Enable();

        inputActions.Player.Click.performed += Click_performed;
    }

    private void OnDestroy()
    {
        inputActions.Player.Click.performed -= Click_performed;
    }

    private void Click_performed(InputAction.CallbackContext obj)
    {
        OnClickAction?.Invoke(this, EventArgs.Empty);
    }
}

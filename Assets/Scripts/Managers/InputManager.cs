using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputActions inputActions;

    public event EventHandler OnClickAction;
    
    // Camera Controls
    public event EventHandler OnDragAction;
    public event EventHandler OnDragStarted;
    public event EventHandler OnDragCanceled;
    public event EventHandler OnScrollUpAction;
    public event EventHandler OnScrollDownAction;

    private void Awake()
    {
        Instance = this;

        inputActions = new InputActions();
        inputActions.Enable();

        inputActions.Player.Click.performed += Click_performed;

        inputActions.Player.Drag.started += Drag_started;
        inputActions.Player.Drag.performed += Drag_performed;
        inputActions.Player.Drag.canceled += Drag_canceled;
        inputActions.Player.Scroll.performed += Scroll_performed;
    }

    private void OnDestroy()
    {
        inputActions.Player.Click.performed -= Click_performed;
        inputActions.Player.Drag.performed -= Drag_performed;
        inputActions.Player.Drag.started -= Drag_started;
        inputActions.Player.Drag.canceled -= Drag_canceled;
        inputActions.Player.Scroll.performed -= Scroll_performed;
    }

    private void Click_performed(InputAction.CallbackContext ctx)
    {
        OnClickAction?.Invoke(this, EventArgs.Empty);
    }


    private void Drag_started(InputAction.CallbackContext ctx)
    {
        OnDragStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Drag_performed(InputAction.CallbackContext ctx)
    {
        OnDragAction?.Invoke(this, EventArgs.Empty);
    }
    private void Drag_canceled(InputAction.CallbackContext ctx)
    {
        OnDragCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Scroll_performed(InputAction.CallbackContext ctx)
    {
        if (inputActions.Player.Scroll.ReadValue<float>() > 0)
        {
            OnScrollUpAction?.Invoke(this, EventArgs.Empty);
        } else {
            OnScrollDownAction?.Invoke(this, EventArgs.Empty);
        }
    }
}

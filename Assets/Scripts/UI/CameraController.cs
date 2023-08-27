using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Vector3 dragStartPosition;
    private Vector3 dragDelta;
    private Camera mainCamera;
    private CinemachineVirtualCamera cinemachineVirtualCamera;

    private float zoomMin = 0.1f;
    private float zoomMax = 8.0f;
    private float zoomDif = 0.25f;

    private bool isDragging;

    private void Awake()
    {
        mainCamera = Camera.main;
        cinemachineVirtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InputManager.Instance.OnDragAction += InputManager_OnDragAction;
        InputManager.Instance.OnDragStarted += InputManager_OnDragStarted;
        InputManager.Instance.OnDragCanceled += InputManager_OnDragCanceled;
        InputManager.Instance.OnScrollUpAction += InputManager_OnScrollUp;
        InputManager.Instance.OnScrollDownAction += InputManager_OnScrollDown;
    }

    void OnDestroy()
    {
        InputManager.Instance.OnDragAction -= InputManager_OnDragAction;
        InputManager.Instance.OnDragStarted -= InputManager_OnDragStarted;
        InputManager.Instance.OnDragCanceled -= InputManager_OnDragCanceled;
        InputManager.Instance.OnScrollUpAction -= InputManager_OnScrollUp;
        InputManager.Instance.OnScrollDownAction -= InputManager_OnScrollDown;
    }

    private void InputManager_OnDragStarted(object sender, System.EventArgs e)
    {
        dragStartPosition = GetMousePosition();
        isDragging = true;
    }
    private void InputManager_OnDragCanceled(object sender, System.EventArgs e)
    {
        isDragging = false;
    }


    private void InputManager_OnDragAction(object sender, System.EventArgs e)
    {
        if (isDragging)
        {
            dragDelta = GetMousePosition() - transform.position;
            transform.position = dragStartPosition - dragDelta;
        }
    }
    private void InputManager_OnScrollUp(object sender, System.EventArgs e)
    {
        // TODO: Make zoom origin mouse position
        cinemachineVirtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(cinemachineVirtualCamera.m_Lens.OrthographicSize - zoomDif, zoomMin, zoomMax);
    }
    private void InputManager_OnScrollDown(object sender, System.EventArgs e)
    {
        // TODO: Make zoom origin mouse position
        cinemachineVirtualCamera.m_Lens.OrthographicSize = Mathf.Clamp(cinemachineVirtualCamera.m_Lens.OrthographicSize + zoomDif, zoomMin, zoomMax);
    }

    private void LateUpdate()
    {
        if (isDragging)
        {
            dragDelta = GetMousePosition() - transform.position;
            transform.position = dragStartPosition - dragDelta;
        }
    }

    private Vector3 GetMousePosition()
    {
        return mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}

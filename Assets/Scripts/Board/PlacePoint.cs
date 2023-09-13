using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Tilemaps;
using JetBrains.Annotations;

public class PlacePoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private GameObject placePointVisual;
    [SerializeField] private Sprite placePointSprite;
    [SerializeField] private Sprite confirmSprite;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool selected; 

    public event EventHandler OnSelected;
    public event EventHandler OnConfirmed;

    public static bool anySelected;
    
    private void Start()
    {
        spriteRenderer = placePointVisual.GetComponent<SpriteRenderer>();
        animator = placePointVisual.GetComponent<Animator>();
        selected = false;
        anySelected = false;

        InputManager.Instance.OnClickEmptyAction += InputManager_OnClickEmptyAction;
    }

    private void Update()
    {
        if (anySelected && animator.enabled && !selected)
        {
            StopAnimation();
        } else if (!anySelected && !animator.enabled)
        {
            StartAnimation();
        }
    }

    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Confirm()
    {
        OnConfirmed?.Invoke(this, EventArgs.Empty);
        Disable();
        anySelected = false;
    }

    public void Select()
    {
        OnSelected?.Invoke(this, EventArgs.Empty);
        anySelected = true;
        selected = true;
        spriteRenderer.sprite = confirmSprite;
    }

    public void Deselect()
    {
        selected = false;
        anySelected = false;
        spriteRenderer.sprite = placePointSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CatanManager.Instance.IsPlaying() && !selected)
        {
            Show();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CatanManager.Instance.IsPlaying() && !selected)
        {
            Hide();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selected) {
            Confirm();
        } else {
            Select();
        }
    }

    public void StopAnimation()
    {
        animator.enabled = false;
    }

    public void StartAnimation()
    {
        animator.enabled = true;
    }

    public void Reset()
    {
        if (gameObject.activeSelf)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = placePointSprite;
            animator.enabled = true;
            animator.Play("Pulse");
            Deselect();
            Start();
        }
    }

    private void InputManager_OnClickEmptyAction(object sender, EventArgs e)
    {
        if (selected)
        {
            Deselect();
        }
    }
}

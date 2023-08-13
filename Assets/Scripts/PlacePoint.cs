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
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject confirm;

    private bool selected; 
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        animator = visual.GetComponent<Animator>();
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        selected = false;
        Resources.Load<GameObject>("");
        Hide();
    }

    private void Show()
    {
        spriteRenderer.enabled = true;
    }

    private void Hide()
    {
        spriteRenderer.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected)
        {
            Show();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected)
        {
            Hide();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        selected = !selected;
        confirm.SetActive(selected);
        animator.enabled = !selected;
    }
}

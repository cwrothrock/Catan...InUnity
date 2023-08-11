using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacePoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private SpriteRenderer visual;
    private void Start()
    {
        Hide();
    }

    private void Show()
    {
        visual.enabled = true;
    }

    private void Hide()
    {
        visual.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked");
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}

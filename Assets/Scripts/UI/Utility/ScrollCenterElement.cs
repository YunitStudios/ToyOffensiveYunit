using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollCenterElement : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }
    

    private bool ignoreSelect;
    
    public void OnSelect(BaseEventData eventData)
    {
        if (ignoreSelect)
            return;
        
        if(InputManager.Instance)
            CenterScrollRect();
    }
    
    private void CenterScrollRect()
    {
        var scrollRect = GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.SmoothScrollToCenter(transform as RectTransform);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        ignoreSelect = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        ignoreSelect = false;
    }
}

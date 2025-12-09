using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EditorAttributes;
using PrimeTween;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Tab : MonoBehaviour, IPointerClickHandler, ISelectHandler, IPointerDownHandler
{
    [SerializeField] private List<TabContent> tabContent;
    [SerializeField, Tooltip("UI Object to select when opening tab")] private GameObject firstSelected;
    [SerializeField, Tooltip("If ticked this will automatically find the first UI object from the firstSelected root")] private bool findFirstSelectedInChildren;
    [SerializeField] private Image tabImage;
    [SerializeField] private TextMeshProUGUI tabText;
     [SerializeField, ShowField(nameof(IsSelectImage)), Tooltip("Text opacity when tab is unselected")] private float unselectedTextAlpha = 0.5f;
    //[SerializeField] private AudioEventSO selectTabSound;
    [SerializeField] private UnityEvent OnSelectTab;
    public Action<Tab> OnSelectTabAction;

    private bool HasText => tabText != null;

    [Flags]
    public enum TabSelectType
    {
        None = 0,
        Image = 1,
        Color = 2,
        Alpha = 4
    }

    public TabSelectType selectType;
    private bool IsSelectImage => selectType.HasFlag(TabSelectType.Image);
    private bool IsSelectColor => selectType.HasFlag(TabSelectType.Color);
    private bool IsSelectAlpha => selectType.HasFlag(TabSelectType.Alpha);

    [SerializeField, ShowField(nameof(IsSelectImage))] private Sprite unselectedImage;
    [SerializeField, ShowField(nameof(IsSelectImage))] private Sprite selectedImage;

    [SerializeField, ShowField(nameof(IsSelectColor))] private Color unselectedColor = Color.black;
    [SerializeField, ShowField(nameof(IsSelectColor))] private Color selectedColor = Color.white;

    [SerializeField, ShowField(nameof(IsSelectAlpha))] private float unselectedAlpha = 0.0f;
    [SerializeField, ShowField(nameof(IsSelectAlpha))] private float selectedAlpha = 1.0f;


    private void Awake()
    {
        // Get canvas if it exists
        foreach (var content in tabContent)
        {
            content.canvasGroup.TryGetComponent(out content.canvas);
        }

        if (findFirstSelectedInChildren)
            firstSelected = firstSelected.GetComponentInChildren<Selectable>().gameObject;

        SetContent(false);
        ToggleCanvas(false);
    }

    public void SetContent(bool value, bool playSound = false)
    {

        //ToggleContent(value);
        ToggleCanvas(value);
        if (value)
        {
            OnSelectTab?.Invoke();
            //if (playSound)
            //    selectTabSound?.PlayOneShot();
        }

        if (tabText != null)
        {
            Color tabColor = tabText.color;
            tabColor.a = value ? 1 : unselectedTextAlpha;
            tabText.color = tabColor;
        }

        if (IsSelectImage)
        {
            tabImage.sprite = value ? selectedImage : unselectedImage;
        }
        if (IsSelectColor)
        {
            tabImage.color = value ? selectedColor : unselectedColor;
        }
        if (IsSelectAlpha)
        {
            tabImage.color = new Color(tabImage.color.r, tabImage.color.g, tabImage.color.b, value ? selectedAlpha : unselectedAlpha);
        }

        if (value && firstSelected)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    private void ToggleCanvas(bool value)
    {


        foreach (var content in tabContent)
        {
            if (content.alphaTween.isAlive)
                content.alphaTween.Complete();

            if (content.fadeTime == 0)
            {
                content.canvasGroup.alpha = value ? 1.0f : 0.0f;
                if (content.disableCanvas && content.canvas)
                    content.canvas.enabled = value;
            }
            else
            {
                if (content.disableCanvas && content.canvas && value)
                    content.canvas.enabled = true;

                content.alphaTween = Tween.Alpha(content.canvasGroup, value ? 1.0f : 0.0f, content.fadeTime, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true).OnComplete(
                    () =>
                    {
                        if (content.disableCanvas && content.canvas && !value)
                            content.canvas.enabled = false;
                    });
            }

            content.canvasGroup.interactable = value;
            content.canvasGroup.blocksRaycasts = value;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectTabAction?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSelectTabAction?.Invoke(this);
        print("click");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        print("down");
    }
}

[Serializable]
public class TabContent
{
    public CanvasGroup canvasGroup;
    [HideInInspector] public Canvas canvas;
    public float fadeTime;
    public Tween alphaTween;
    public bool disableCanvas = false;
}
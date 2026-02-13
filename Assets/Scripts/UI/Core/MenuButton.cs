using System;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField] private TMP_Text tabText;
    [SerializeField, ShowField(nameof(NeedsImage))] private Image tabImage;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)]
    [SerializeField] private ButtonSelectType selectType;
    [SerializeField, ShowField(nameof(IsSelectImage)), Tooltip("Image sprite when button is unselected")] 
    private Sprite unselectedImage;
    [SerializeField, ShowField(nameof(IsSelectImage)), Tooltip("Image sprite when button is selected")] 
    private Sprite selectedImage;
    [SerializeField, ShowField(nameof(IsSelectColor)), Tooltip("Image color when button is unselected")] 
    private Color unselectedColor = Color.black;
    [SerializeField, ShowField(nameof(IsSelectColor)), Tooltip("Image color when button is selected")] 
    private Color selectedColor = Color.white;
    [SerializeField, ShowField(nameof(IsSelectAlpha)), Tooltip("Image opacity when button is unselected")] 
    private float unselectedAlpha = 0.0f;
    [SerializeField, ShowField(nameof(IsSelectAlpha)), Tooltip("Image opacity when button is selected")] 
    private float selectedAlpha = 1.0f;
    [SerializeField, ShowField(nameof(IsSelectTextAlpha)), Tooltip("Text opacity when button is unselected")] 
    private float unselectedTextAlpha = 0.5f;
    [SerializeField, ShowField(nameof(IsSelectTextAlpha)), Tooltip("Text opacity when button is selected")] 
    private float selectedTextAlpha = 1.0f;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)] 
    [SerializeField] private UnityEvent OnButtonClicked;
    
    private Action OnClickEvent;
    public string GetText => tabText ? tabText.text : string.Empty;
    
    [Flags]
    public enum ButtonSelectType
    {
        None = 0,
        Sprite = 1,
        Color = 2,
        Alpha = 4,
        TextAlpha = 8
    }

    private bool IsSelectImage => selectType.HasFlag(ButtonSelectType.Sprite);
    private bool IsSelectColor => selectType.HasFlag(ButtonSelectType.Color);
    private bool IsSelectAlpha => selectType.HasFlag(ButtonSelectType.Alpha);
    private bool IsSelectTextAlpha => selectType.HasFlag(ButtonSelectType.TextAlpha);
    private bool NeedsImage => IsSelectImage || IsSelectColor || IsSelectAlpha;
    private bool NeedsText => IsSelectTextAlpha;


    public void SetState(bool value)
    {
        if (IsSelectTextAlpha && tabText != null)
        {
            Color tabColor = tabText.color;
            tabColor.a = value ? 1 : unselectedTextAlpha;
            tabText.color = tabColor;
        }

        if (IsSelectImage && tabImage != null)
        {
            tabImage.sprite = value ? selectedImage : unselectedImage;
        }
        if (IsSelectColor && tabImage != null)
        {
            tabImage.color = value ? selectedColor : unselectedColor;
        }
        if (IsSelectAlpha && tabImage != null)
        {
            tabImage.color = new Color(tabImage.color.r, tabImage.color.g, tabImage.color.b, value ? selectedAlpha : unselectedAlpha);
        }
    }

    public void SetClickEvent(Action clickEvent)
    {
        OnClickEvent = clickEvent;
    }


    public void OnSubmit(BaseEventData eventData)
    {
        OnClickEvent?.Invoke();
        OnButtonClicked?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEvent?.Invoke();
        OnButtonClicked?.Invoke();
    }
    
}

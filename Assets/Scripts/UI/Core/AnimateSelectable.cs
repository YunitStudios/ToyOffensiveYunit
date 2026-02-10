using System;
using System.Collections;
using EditorAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AnimateSelectable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
{

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField, ShowField(nameof(NeedsTransform))] private RectTransform selectableTransform;
    [SerializeField, ShowField(nameof(NeedsImage))] private Image selectableImage;
    [SerializeField, ShowField(nameof(NeedsText))] private TextMeshProUGUI selectableText;
    //[SerializeField] private AudioEventSO confirmSound;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private SelectableAnimateType animateType;
    [SerializeField] private bool animateScale;
    [SerializeField, ShowField(nameof(animateScale))] private float scaleMax = 1.1f;
    [SerializeField, ShowField(nameof(animateScale))] private float scaleInLength = 0.1f;
    [SerializeField, ShowField(nameof(animateScale))] private float scaleOutLength = 0.2f;
    [SerializeField] private bool animateColor;
    [SerializeField, ShowField(nameof(animateColor))] private Color hoverButtonColor;
    [SerializeField, ShowField(nameof(animateColor))] private float colorInLength = 0.1f;
    [SerializeField, ShowField(nameof(animateColor))] private float colorOutLength = 0.4f;
    [SerializeField] private bool animatePosition;
    [SerializeField, ShowField(nameof(animatePosition))] private Vector3 hoverOffset;
    [SerializeField, ShowField(nameof(animatePosition))] private float hoverInLength = 0.1f;
    [SerializeField, ShowField(nameof(animatePosition))] private float hoverOutLength = 0.2f;
    [SerializeField] private bool animateTextColor;
    [SerializeField, ShowField(nameof(animateTextColor))] private Color hoverTextColor;
    [SerializeField, ShowField(nameof(animateTextColor))] private float textColorInLength = 0.1f;
    [SerializeField, ShowField(nameof(animateTextColor))] private float textColorOutLength = 0.2f;
    
    private enum SelectableAnimateType
    {
        Click,
        Hover
    }
    
    private bool NeedsTransform => animateScale || animatePosition;
    private bool NeedsImage => animateColor;
    private bool NeedsText => animateTextColor;
    
    private float MaxInDuration => Mathf.Max(
        colorInLength,
        scaleInLength,
        hoverInLength,
        textColorInLength
    );
    
    private Selectable selectable;
    
    private Vector3 buttonScale;
    private Color buttonColor;
    private Vector3 buttonOffset;
    private Tween colorTween;
    private Tween scaleTween;
    private Tween offsetTween;
    private Color defaultTextColor;

    private bool init;

    private void Awake()
    {
        selectable = GetComponentInChildren<Selectable>();
        if (!selectable)
        {
            Debug.LogError("No selectable found, AnimateSelectable script disabled");
            enabled = false;
            return;
        }
    }


    public virtual void Start()
    {
        init = true;
        
        if(selectableImage)
            buttonColor = selectableImage.color;
        buttonScale = selectableTransform.localScale;
        buttonOffset = selectableTransform.localPosition;
        if (selectableText)
            defaultTextColor = selectableText.color;
        
        if(selectable is Button btn)
            btn.onClick.AddListener(PlayConfirmSound);
    }

    void OnDestroy()
    {
        colorTween.Stop();
        scaleTween.Stop();
        offsetTween.Stop();
    }

    private bool CanClick => animateType == SelectableAnimateType.Click && (!selectable || selectable.interactable);
    private bool CanHover => animateType == SelectableAnimateType.Hover && (!selectable || selectable.interactable);

    private void TryClick(bool value)
    {
        if (!CanClick)
            return;
        
        //TryPlaySound(inSoundName);
        
        if(value)
            TriggerDown();
        else
            TriggerUp();
    }
    private void TryHover(bool value)
    {
        if (!CanHover)
            return;
        
        //TryPlaySound(hoverSoundName);
        
        if(value)
            TriggerDown();
        else
            TriggerUp();
    }
    

    public void OnPointerDown(PointerEventData eventData)
    {
        TryClick(true);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        TryClick(false);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        if (!CanClick)
            return;

        StartCoroutine(nameof(TriggerSequence));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TryHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TryHover(false);
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        TryHover(true);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        TryHover(false);
    }
    
    private void PlayConfirmSound()
    {
        //confirmSound?.PlayOneShot();
    }

    private IEnumerator TriggerSequence()
    {
        TriggerDown();
        yield return new WaitForSeconds(MaxInDuration);
        TriggerUp();
    }
    

    private void TriggerDown()
    {
        if (!init)
            return;
        
        
        if(animateColor && selectableImage)
        {
            colorTween.Stop();
            Tween.Color(selectableImage, hoverButtonColor, colorInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }

        if (animateScale)
        {
            scaleTween.Stop();
            Tween.Scale(selectableTransform, buttonScale * scaleMax, scaleInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animatePosition)
        {
            offsetTween.Stop();
            Tween.LocalPosition(selectableTransform, buttonOffset + hoverOffset, hoverInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animateTextColor && selectableText)
        {
            Tween.Color(selectableText, hoverTextColor, textColorInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
    }
    


    private void TriggerUp()
    {
        if (!init)
            return;
        
        if(animateColor && selectableImage)
        {
            colorTween.Stop();
            Tween.Color(selectableImage, buttonColor, colorOutLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }

        if (animateScale)
        {
            scaleTween.Stop();
            Tween.Scale(selectableTransform, buttonScale, scaleOutLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animatePosition)
        {
            offsetTween.Stop();
            Tween.LocalPosition(selectableTransform, buttonOffset, hoverOutLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animateTextColor && selectableText)
        {
            Tween.Color(selectableText, defaultTextColor, textColorOutLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
    }
    
    private void TryPlaySound(string soundName)
    {
        //if(playSound)
        //    AudioManager.PlayOneShot(AudioManager.GetEvent(soundName));
    }
    
}

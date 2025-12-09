using EditorAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AnimateSelectable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    
    [Header("Components")]
    [SerializeField] private RectTransform selectableTransform;
    [SerializeField] private Image selectableImage;
    [SerializeField] private Selectable selectable;
    //[SerializeField] private AudioEventSO confirmSound;

    [Header("Options")] 
    [SerializeField] private bool animateScale;
    [SerializeField, ShowField(nameof(animateScale))] private float hoverScale = 1.1f;
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
    [SerializeField, ShowField(nameof(animateTextColor))] private TextMeshProUGUI buttonText;
    [SerializeField, ShowField(nameof(animateTextColor))] private Color hoverTextColor;
    [SerializeField, ShowField(nameof(animateTextColor))] private float textColorInLength = 0.1f;
    [SerializeField, ShowField(nameof(animateTextColor))] private float textColorOutLength = 0.2f;
    private Color defaultTextColor;

    private Vector3 buttonScale;
    private Color buttonColor;
    private Vector3 buttonOffset;
    private Tween colorTween;
    private Tween scaleTween;
    private Tween offsetTween;

    private bool init;
    

    public virtual void Start()
    {
        init = true;
        
        if(selectableImage)
            buttonColor = selectableImage.color;
        buttonScale = selectableTransform.localScale;
        buttonOffset = selectableTransform.localPosition;
        if (buttonText)
            defaultTextColor = buttonText.color;
        
        if(selectable is Button btn)
            btn.onClick.AddListener(PlayConfirmSound);
    }

    void OnDestroy()
    {
        colorTween.Stop();
        scaleTween.Stop();
        offsetTween.Stop();
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if ( selectable && !selectable.interactable)
            return;
        
        //TryPlaySound(inSoundName);
        
        TriggerDown();
    }
    public void OnSelect(BaseEventData eventData)
    {
        TriggerDown();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if ( selectable && !selectable.interactable)
            return;
        
        //TryPlaySound(outSoundName);
        
        TriggerUp();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        TriggerUp();
    }
    
    private void PlayConfirmSound()
    {
        //confirmSound?.PlayOneShot();
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
            Tween.Scale(selectableTransform, buttonScale * hoverScale, scaleInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animatePosition)
        {
            offsetTween.Stop();
            Tween.LocalPosition(selectableTransform, buttonOffset + hoverOffset, hoverInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
        
        if (animateTextColor && buttonText)
        {
            Tween.Color(buttonText, hoverTextColor, textColorInLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
    }
    
    private void TryPlaySound(string soundName)
    {
        //if(playSound)
        //    AudioManager.PlayOneShot(AudioManager.GetEvent(soundName));
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
        
        if (animateTextColor && buttonText)
        {
            Tween.Color(buttonText, defaultTextColor, textColorOutLength, Ease.Default, 1, CycleMode.Restart, 0f, 0f, true);
        }
    }


    public void OnSubmit(BaseEventData eventData)
    {
        //TryPlaySound(outSoundName);
    }
}

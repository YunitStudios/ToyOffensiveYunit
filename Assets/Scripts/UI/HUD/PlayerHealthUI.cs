using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Image foregroundImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text valueText;

    [Header("Attributes")] 
    // I dont know how to link this value with the players health script
    [SerializeField] private float maxHealth;
    [SerializeField] private float animationLength;
    [SerializeField] private Ease animationEase;
    [SerializeField] private float animationBackgroundDelay;
    [SerializeField] private Color damageFlashColor;
    [SerializeField] private TweenSettings damageFlashInSettings;
    [SerializeField] private TweenSettings damageFlashOutSettings;
    
    [Header("Events")] 
    [SerializeField] private FloatEventChannelSO onHealthChanged;

    private float currentValue;
    private Tween mainTween;
    private Sequence flashSequence;
    private Color defaultColor;

    private void OnEnable()
    {
        onHealthChanged.OnEventRaised += SetValue;
    }
    private void OnDisable()
    {
        onHealthChanged.OnEventRaised -= SetValue;
    }


    private void Awake()
    {
        currentValue = maxHealth;
        defaultColor = foregroundImage.color;
    }

    private void SetValue(float newValue)
    {
        newValue = Mathf.Clamp(newValue, 0, maxHealth);
        
        float difference = newValue - currentValue;
        
        if(difference < 0) 
            PlayDamageAnimation(newValue);
        else
        {
            float percent = newValue / maxHealth;
            foregroundImage.fillAmount = percent;
            backgroundImage.fillAmount = percent;
        }
        
        valueText.text = Mathf.RoundToInt(newValue).ToString();
        currentValue = newValue;
    }

    private void PlayDamageAnimation(float newValue)
    {
        float newPercent = newValue / maxHealth;
        
        if(mainTween.isAlive)
            mainTween.Stop();
        if(flashSequence.isAlive)
            flashSequence.Stop();
        
        mainTween = Tween.UIFillAmount(foregroundImage, newPercent, animationLength, animationEase);
        
        flashSequence = Sequence.Create().
            Group(Tween.Color(foregroundImage, damageFlashColor, damageFlashInSettings))
            .Chain(Tween.Color(foregroundImage, defaultColor, damageFlashOutSettings));

    }
}

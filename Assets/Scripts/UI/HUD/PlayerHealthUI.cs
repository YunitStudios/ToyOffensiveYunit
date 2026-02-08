using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Slider mainSlider;
    [SerializeField] private Slider backgroundSlider;

    [Header("Attributes")] 
    // I dont know how to link this value with the players health script
    [SerializeField] private float maxHealth;
    [SerializeField] private float animationLength;
    [SerializeField] private Ease animationEase;
    [SerializeField] private float animationBackgroundDelay;
    
    [Header("Events")] 
    [SerializeField] private FloatEventChannelSO onHealthChanged;

    private float currentValue;
    private float mainAnimValue;
    private float backgroundAnimValue;
    private Tween mainTween;
    private Tween backgroundTween;

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
    }

    private void SetValue(float newValue)
    {
        float difference = newValue - currentValue;
        
        if(difference < 0) 
            PlayDamageAnimation(newValue);
        else
        {
            float percent = newValue / maxHealth;
            mainSlider.value = percent;
            backgroundSlider.value = percent;
        }

        currentValue = newValue;
    }

    private void PlayDamageAnimation(float newValue)
    {
        float newPercent = newValue / maxHealth;
        
        if(mainTween.isAlive)
            mainTween.Stop();
        if(backgroundTween.isAlive)
            backgroundTween.Stop();
        
        mainTween = Tween.UISliderValue(mainSlider, newPercent, animationLength, animationEase);

        backgroundTween = Tween.Delay(animationBackgroundDelay, () =>
            backgroundTween = Tween.UISliderValue(backgroundSlider, newPercent, animationLength, animationEase)
        );
    }
}

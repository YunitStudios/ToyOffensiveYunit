using System;
using UnityEngine;

public class DamageOverlayController : MonoBehaviour
{
    [Header("Attributes")] 
    // I dont know how to link this value with the players health script
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxOpacity = 0.75f;
    [SerializeField] private float minOpacity = 0.2f;
    
    [Header("Events")] 
    [SerializeField] private FloatEventChannelSO onHealthChanged;
    
    private CanvasFader fader;
    private float lastHealth;
    private void OnEnable()
    {
        onHealthChanged.OnEventRaised += UpdateDamage;
        lastHealth = maxHealth;
    }
    private void OnDisable()
    {
        onHealthChanged.OnEventRaised -= UpdateDamage;
    }
    
    private void Awake()
    {
        fader = gameObject.GetComponent<CanvasFader>();
    }
    void UpdateDamage(float value)
    {
        if (value < lastHealth)
        {
            // map damage between 0 and the max opacity
            float clampedValue = Mathf.Clamp(value, 0f, maxHealth);
            float healthLostPercent = 1f - (clampedValue / maxHealth);
            float interpolatedValue = healthLostPercent * maxOpacity;
            
            lastHealth = clampedValue;
            
            fader.maxAlpha = Mathf.Clamp(interpolatedValue, minOpacity, maxHealth);
            fader.PlayFull();
        }
    }
}

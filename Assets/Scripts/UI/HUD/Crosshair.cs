using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float expandSpeed;
    [SerializeField] private Image hitmarker;
    [SerializeField] private float hitmarkerTime;

    [Header("Input Events")]
    [SerializeField] private FloatEventChannelSO onUpdateSpread;
    [SerializeField] private VoidEventChannelSO onShowHitmarker;

    private RectTransform[] crosshair;
    private Vector2[] originalCrosshairPositions;
    private float currentSpread;
    private float spreadMultiplier;
    private float nonADSspreadMultiplier = 25f;
    private float adsSpreadMultiplier = 10f;
    private bool isADS;
    private float targetSpread;
    private float crosshairScale;

    private void Start()
    {
        // Gets the crosshairs parts and saves the original position
        crosshair = GetComponentsInChildren<RectTransform>();
        originalCrosshairPositions = new Vector2[crosshair.Length];
        for (int i = 0; i < crosshair.Length; i++)
        {
            originalCrosshairPositions[i] = crosshair[i].anchoredPosition;
        }
        // Hit marker is invisible at start
        hitmarker.color = new Color(hitmarker.color.r, hitmarker.color.g, hitmarker.color.b, 0f);
    }

    private void Update()
    {
        if (isADS)
        {
            spreadMultiplier = adsSpreadMultiplier;
            crosshairScale = 0.2f;
        }
        else
        {
            spreadMultiplier = nonADSspreadMultiplier;
            crosshairScale = 1f;
        }
        currentSpread = Mathf.Lerp(currentSpread, targetSpread * spreadMultiplier, Time.deltaTime * expandSpeed);
        
        // Move each crosshair piece outwards due to spread
        for (int i = 0; i < crosshair.Length; i++)
        {
            Vector2 direction = originalCrosshairPositions[i].normalized;
            Vector2 position = originalCrosshairPositions[i] * crosshairScale;
            crosshair[i].anchoredPosition = position + direction * currentSpread;
        }
    }

    private void OnEnable()
    {
        onUpdateSpread.OnEventRaised += UpdateSpread;
        onShowHitmarker.OnEventRaised += ShowHitmarker;
    }
    private void OnDisable()
    {
        onUpdateSpread.OnEventRaised -= UpdateSpread;
        onShowHitmarker.OnEventRaised -= ShowHitmarker;
    }

    private void UpdateSpread(float newSpread)
    {
        targetSpread = newSpread;
        // Smoothly moves current spread to new spread
        if (isADS)
        {
            spreadMultiplier = adsSpreadMultiplier;
        }
        else
        {
            spreadMultiplier = nonADSspreadMultiplier;
        }
        currentSpread = Mathf.Lerp(currentSpread, newSpread * spreadMultiplier, Time.deltaTime * expandSpeed);
    }
    
    // Call when player hits something
    private void ShowHitmarker()
    {
        StartCoroutine(ShowHitMarker());
    }

    // Handles showing the hitmarker and fading out
    IEnumerator ShowHitMarker()
    {
        float timeGone = 0f;
        hitmarker.color = new Color(hitmarker.color.r, hitmarker.color.g, hitmarker.color.b, 1f);
        while (timeGone < hitmarkerTime)
        {
            timeGone += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timeGone / hitmarkerTime);
            hitmarker.color = new Color(hitmarker.color.r, hitmarker.color.g, hitmarker.color.b, alpha);
            yield return null;
        }
    }

    public void SetADS(bool isAiming)
    {
        isADS = isAiming;
    }
}

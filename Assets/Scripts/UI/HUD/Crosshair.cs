using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Crosshair : MonoBehaviour
{
    private RectTransform[] crosshair;
    private Vector2[] originalCrosshairPositions;
    private float currentSpread;
    [SerializeField] private float expandSpeed;
    [SerializeField] private Image hitmarker;
    [SerializeField] private float hitmarkerTime;
    private float spreadMultiplier = 25f;

    void Start()
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

    void Update()
    {
        // Move each crosshair piece outwards due to spread
        for (int i = 0; i < crosshair.Length; i++)
        {
            Vector2 direction = originalCrosshairPositions[i].normalized;
            crosshair[i].anchoredPosition = originalCrosshairPositions[i] + direction * currentSpread;
        }
    }

    public void UpdateSpread(float newSpread)
    {
        // Smoothly moves current spread to new spread
        currentSpread = Mathf.Lerp(currentSpread, newSpread * spreadMultiplier, Time.deltaTime * expandSpeed);
    }
    
    // Call when player hits something
    public void Hitmarker()
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
}

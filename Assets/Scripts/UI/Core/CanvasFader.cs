using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    [Header("General")]
    [SerializeField, Range(0,1)] private float maxAlpha = 1.0f;
    
    [Header("Fade In")] 
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    
    [Header("Fade Out")]
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Ease fadeOutEase = Ease.OutQuad;

    private CanvasGroup canvasGroup;
    private Tween fullTween;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }


    public void PlayFull()
    {
        if (!canvasGroup)
        {
            Debug.LogWarning("No canvas group assigned to " + gameObject.name + ", cannot trigger CanvasFader animation");
            return;
        }
        
        if(fullTween.isAlive)
            fullTween.Stop();

        fullTween = FadeIn();
        fullTween.OnComplete(() => fullTween = FadeOut());

    }
    public void PlayIn() => FadeIn();
    public void PlayOut() => FadeOut();

    private Tween FadeIn()
    {
        if (!canvasGroup)
        {
            Debug.LogWarning("No canvas group assigned to " + gameObject.name + ", cannot trigger CanvasFader animation");
            return default;
        }
        
        return Tween.Alpha(canvasGroup, maxAlpha, fadeInTime, fadeInEase);
    }

    private Tween FadeOut()
    {
        if (!canvasGroup)
        {
            Debug.LogWarning("No canvas group assigned to " + gameObject.name + ", cannot trigger CanvasFader animation");
            return default;
        }

        return Tween.Alpha(canvasGroup, 0.0f, fadeOutTime, fadeOutEase);
    }
}

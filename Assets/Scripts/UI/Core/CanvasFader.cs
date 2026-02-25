using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using EditorAttributes;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    [Header("General")]
    [SerializeField, Range(0, 1)] private float maxAlpha = 1.0f;
    [SerializeField, Tooltip("How long to stay at max alpha during full play")] private float showTime = 2;

    [Header("Fade In")]
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;


    [Header("Fade Out")]
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private Ease fadeOutEase = Ease.OutQuad;


    [Header("Scaling")]
    [SerializeField, ShowField(nameof(usingScaling))] private Transform scaleTransform;
    [SerializeField] private bool scaleIn = false;
    [SerializeField, ShowField(nameof(scaleIn))] private float scaleInTime = 0.5f;
    [SerializeField, ShowField(nameof(scaleIn))] private float startingScale = 0.5f;
    [SerializeField, ShowField(nameof(scaleIn))] private float targetScale = 1f;
    [SerializeField, ShowField(nameof(scaleIn))] private Ease scaleInEase = Ease.OutQuad;
    [SerializeField] private bool scaleOut = false;
    [SerializeField, ShowField(nameof(scaleOut))] private float scaleOutTime = 0.5f;
    [SerializeField, ShowField(nameof(scaleOut))] private float endingScale = 1;
    [SerializeField, ShowField(nameof(scaleOut))] private Ease scaleOutEase = Ease.OutQuad;
    private bool usingScaling => scaleIn || scaleOut;
    private Transform ScalingTransform => scaleTransform ? scaleTransform : canvasGroup.transform;

    public Action OnFadeOutStart;
    public Action OnFadeOutEnd;

    private CanvasGroup canvasGroup;
    private Sequence fadeSequence;
    
    public bool IsFading => fadeSequence.isAlive;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    private void OnDisable()
    {
        if(fadeSequence.isAlive)
            fadeSequence.Stop();
    }
    
    public enum FadeType
    {
        Full,
        In,
        Out
    }
    
    public void Play(FadeType fadeType)
    {
        if (fadeSequence.isAlive)
            fadeSequence.Stop();

        fadeSequence = Sequence.Create();

        if (fadeType is FadeType.Full or FadeType.In)
        {
            fadeSequence.Chain(FadeIn());
            if (scaleIn)
                fadeSequence.Group(ScaleIn());
        }

        if (fadeType == FadeType.Full)
        {
            fadeSequence.ChainDelay(showTime);
        }

        if (fadeType is FadeType.Full or FadeType.Out)
        {
            fadeSequence.Chain(FadeOut());
            if (scaleOut)
                fadeSequence.Group(ScaleOut());
        }
    }

    private Tween FadeIn()
    {
        return Tween.Alpha(canvasGroup, maxAlpha, fadeInTime, fadeInEase);
    }

    private Tween FadeOut()
    {
        OnFadeOutStart?.Invoke();
        return Tween.Alpha(canvasGroup, 0.0f, fadeOutTime, fadeOutEase).OnComplete(() => OnFadeOutEnd?.Invoke());
    }

    private Tween ScaleIn()
    {
        return Tween.Scale(ScalingTransform, targetScale, scaleInTime);
    }
    private Tween ScaleOut()
    {
        return Tween.Scale(ScalingTransform, endingScale, scaleInTime);
    }
    
    public void OverwriteFadeTimes(float fadeIn, float fadeOut, float show)
    {
        fadeInTime = fadeIn;
        fadeOutTime = fadeOut;
        showTime = show;
    }
}

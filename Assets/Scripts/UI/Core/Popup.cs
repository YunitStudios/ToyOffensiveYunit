using System;
using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup),typeof(Canvas))]
public class Popup : MonoBehaviour
{
    private bool HasBackgroundPanel => backgroundPanel;
    public bool IsDisplaying => canvasGroup && canvasGroup.alpha > 0.0f;
    public float OutTime => Mathf.Max(fadeOutTime, moveOutTime);

    public static Action OnPopupOpened;
    public static Action OnPopupClosed;

    [Title("\n<b><color=#ff8080>References", 15, 5, false)]
    [SerializeField, Tooltip("Transform for where the UI starts, excluding any background UI")] 
    private RectTransform contentPanel;
    [SerializeField, Tooltip("Canvas group at the root for any background UI (e.g. black tint)")] 
    private CanvasGroup backgroundPanel;
    [SerializeField, Tooltip("First object to be selected by the input system")] 
    private GameObject firstSelectedRoot;
    [SerializeField, ShowField(nameof(HasFirstSelected)), Tooltip("If ticked this will automatically find the first UI object from the firstSelected root")]
    private bool findFirstSelectedInChildren;

    private bool HasFirstSelected => firstSelectedRoot;

     [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)]
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private Ease fadeInEase = Ease.Default;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private Ease fadeOutEase = Ease.Default;
    [SerializeField] private bool movePanel;
    [SerializeField, ShowField(nameof(movePanel))] private float moveInDistance = 300;
    [SerializeField, ShowField(nameof(movePanel))] private float moveInTime = 0.5f;
    [SerializeField, ShowField(nameof(movePanel))] private Ease moveInEase = Ease.Default;
    [SerializeField, ShowField(nameof(movePanel))] private float moveOutDistance = 100;
    [SerializeField, ShowField(nameof(movePanel))] private float moveOutTime = 0.25f;
    [SerializeField, ShowField(nameof(movePanel))] private Ease moveOutEase = Ease.Default;
    [SerializeField, ShowField(nameof(movePanel)), Tooltip("Toggle to swap move animation to sideways instead")] private bool moveSide = false;
    [SerializeField, ShowField(nameof(HasBackgroundPanel))] private float backgroundFadeInTime = 0.2f;
    [SerializeField, ShowField(nameof(HasBackgroundPanel))] private Ease backgroundFadeInEase = Ease.Default;
    [SerializeField, ShowField(nameof(HasBackgroundPanel))] private float backgroundFadeOutTime = 0.2f;
    [SerializeField, ShowField(nameof(HasBackgroundPanel))] private Ease backgroundFadeOutEase = Ease.Default;
    [SerializeField] private bool disableGameObjectWhenClosed = true;
    [SerializeField, Tooltip("Pause game while popup is displaying")] private bool freezeTimeScale = false;
    [SerializeField, Tooltip("If display is called again, then it closes rather than reopen")] private bool closeOnRetoggle;
    [SerializeField] private bool disableInputsWhenOpen;
    [SerializeField] private bool toggleCursor;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    public UnityEvent OnOpenPopup;
    public UnityEvent OnOpenedPopup;
    public UnityEvent OnClosePopup;
    public UnityEvent OnClosedPopup;

    private GameObject lastSelected;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector3 startingPos;

    private Tween dimTween;
    private Tween moveTween;
    private Tween fadeTween;



    private bool init;

    protected virtual void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
        if (backgroundPanel)
        {
            backgroundPanel.alpha = 0.0f;
            backgroundPanel.blocksRaycasts = false;
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if (findFirstSelectedInChildren)
            firstSelectedRoot = firstSelectedRoot.GetComponentInChildren<Selectable>().gameObject;
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
    }

    private void Start()
    {
        if (disableGameObjectWhenClosed)
            gameObject.SetActive(false);
    }

    private void Setup()
    {
        startingPos = contentPanel.localPosition;

        init = true;
    }

    [Button]
    public void Display()
    {
        if (disableGameObjectWhenClosed && !gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!init)
            Setup();

        if (closeOnRetoggle && IsDisplaying)
        {
            Hide();
            return;
        }

        if (dimTween.isAlive)
            dimTween.Stop();
        if (fadeTween.isAlive)
            fadeTween.Stop();
        if (moveTween.isAlive)
            moveTween.Stop();

        if (freezeTimeScale)
            Time.timeScale = 0.0f;

        //if (disableInputsWhenOpen)
            //InputManager.Instance.ToggleInputs(false);

        if (EventSystem.current.currentSelectedGameObject)
            lastSelected = EventSystem.current.currentSelectedGameObject;
        if (firstSelectedRoot)
            EventSystem.current.SetSelectedGameObject(firstSelectedRoot);

        //AudioManager.PlayOneShot(openSound);

        canvas.enabled = true;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0.001f;
        
        if(toggleCursor)
            InputManager.Instance.ToggleCursor(true);

        if (backgroundPanel)
        {
            backgroundPanel.blocksRaycasts = true;
            dimTween = Tween.Alpha(backgroundPanel, 0.0f, 1.0f, backgroundFadeInTime, backgroundFadeInEase, 1, CycleMode.Restart, 0f, 0f, true);
        }
        fadeTween = Tween.Alpha(canvasGroup, 0.0f, 1.0f, fadeInTime, fadeInEase, 1, CycleMode.Restart, 0f, 0f, true).OnComplete(
            () =>
            {
                canvasGroup.interactable = true;
                OnOpenedPopup?.Invoke();
            });

        if(movePanel)
        {
            if (!moveSide)
                moveTween = Tween.LocalPositionY(contentPanel, startingPos.y - moveInDistance, startingPos.y,
                    moveInTime, moveInEase, 1, CycleMode.Restart, 0f, 0f, true);
            else
                moveTween = Tween.LocalPositionX(contentPanel, startingPos.x + moveInDistance, startingPos.x,
                    moveInTime, moveInEase, 1, CycleMode.Restart, 0f, 0f, true);
        }

        OnOpenPopup?.Invoke();
        OnPopupOpened?.Invoke();
    }

    [Button]
    public void Hide()
    {
        if (!init)
            Setup();

        if (!IsDisplaying)
            return;

        if (dimTween.isAlive)
            dimTween.Stop();
        if (fadeTween.isAlive)
            fadeTween.Stop();
        if (moveTween.isAlive)
            moveTween.Stop();

        if (backgroundPanel)
            dimTween = Tween.Alpha(backgroundPanel, 0.0f, backgroundFadeOutTime, backgroundFadeOutEase, 1, CycleMode.Restart, 0f, 0f, true).OnComplete(() => backgroundPanel.blocksRaycasts = false);
        fadeTween = Tween.Alpha(canvasGroup, 0.0f, fadeOutTime, fadeOutEase, 1, CycleMode.Restart, 0f, 0f, true).OnComplete(() =>
        {
            canvas.enabled = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (freezeTimeScale)
                Time.timeScale = 1.0f;
            //if (disableInputsWhenOpen)
                //InputManager.Instance.ToggleInputs(true);

            if (lastSelected)
                EventSystem.current.SetSelectedGameObject(lastSelected);
            
            if (toggleCursor)
                InputManager.Instance.ToggleCursor(false);

            if (disableGameObjectWhenClosed)
                gameObject.SetActive(false);
            
            OnClosedPopup?.Invoke();
        });

        if(movePanel)
        {
            if (!moveSide)
                moveTween = Tween.LocalPositionY(contentPanel, startingPos.y - moveOutDistance, moveOutTime,
                    moveOutEase, 1, CycleMode.Restart, 0f, 0f, true);
            else
                moveTween = Tween.LocalPositionX(contentPanel, startingPos.x + moveOutDistance, moveOutTime,
                    moveOutEase, 1, CycleMode.Restart, 0f, 0f, true);
        }

        OnClosePopup?.Invoke();
        OnPopupClosed?.Invoke();
    }
}
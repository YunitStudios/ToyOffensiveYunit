using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using PrimeTween;
using UnityEngine.SceneManagement;
using EditorAttributes;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [SerializeField] private SerializedDictionary<SceneTypes, SceneReference> scenes;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 1;
    [SerializeField] private Ease fadeInEase;
    [SerializeField] private float fadeOutTime = 1;
    [SerializeField] private Ease fadeOutEase;
    [SerializeField] private float waitTime;

    private Tween fadeInTween;
    private Tween fadeOutTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        Init();
    }

    private void Init()
    {
        canvasGroup.alpha = 0.0f;
    }


    public static void TransitionScene(SceneTypes sceneType, Action callback = null)
    {
        SceneReference scene = GetSceneReference(sceneType);

        Instance.StartTransition(scene, callback);

    }
    public static SceneReference GetSceneReference(SceneTypes type)
    {
        return Instance.scenes.GetValueOrDefault(type);
    }

    private void StartTransition(SceneReference scene, Action callback)
    {
        StartCoroutine(TransitioningCoroutine(scene, callback));
    }
    
    private void EndTransition(Action callback)
    {
        callback?.Invoke();
    }

    private IEnumerator TransitioningCoroutine(SceneReference scene, Action callback)
    {
        fadeInTween = Tween.Alpha(canvasGroup, 1.0f, fadeInTime);

        yield return new WaitForSeconds(fadeInTime);

        var sceneLoading = SceneManager.LoadSceneAsync(scene.ScenePath);
        
        if(sceneLoading == null)
        {
            Debug.LogError($"Scene {scene} could not be loaded. Check that it is added to the build settings.");
            yield break;
        }
        
        while (!sceneLoading.isDone)
            yield return null;

        yield return new WaitForSeconds(waitTime);

        fadeOutTween = Tween.Alpha(canvasGroup, 0.0f, fadeOutTime);
        
        yield return new WaitForSeconds(fadeOutTime);

        EndTransition(callback);
    }
    



    public enum SceneTypes
    {
        MainMenu,
        Ingame
    }

}

using System;
using EditorAttributes;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool Ingame { get; private set; }
    
    #region Singleton
    public static GameManager Instance { get; private set; }
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
    #endregion
    
    [Header("References")]
    [SerializeField] private IngameStats ingameStats;

    [Header("Event Binding")] 
    [SerializeField] private VoidEventChannelSO onLoadGame;
    
    [Header("Event Triggering")]
    [SerializeField] private FloatEventChannelSO onTimePassed;

    private void OnEnable()
    {
        onLoadGame.OnEventRaised += LoadGame;
    }
    private void OnDisable()
    {
        onLoadGame.OnEventRaised -= LoadGame;
    }

    private void Init()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
    }


    private void Update()
    {
        if(Ingame)
            IngameTick();
    }
    
    private void IngameTick()
    {
        onTimePassed?.Invoke(Time.deltaTime);
    }

    [Button]
    public void LoadGame()
    {
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.Ingame, StartGame);
    }

    public void StartGame()
    {
        Ingame = true;
        
        ingameStats.Start();
        
        print("Game Started");
    }
    [Button]
    public void EndGame()
    { 
        Ingame = false;
        
        ingameStats.Stop();
        
        print("Game Ended");
        
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.MainMenu);

    }
    
}

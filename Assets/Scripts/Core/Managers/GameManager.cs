using System;
using EditorAttributes;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    
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
    [SerializeField] private ScoreTrackerSO scoreTracker;
    [SerializeField] private PlayerDataSO playerDataSO;

    [Header("Input Events")] 
    [SerializeField] private VoidEventChannelSO onStartLevel;
    [SerializeField] private VoidEventChannelSO onQuitLevel;
    [SerializeField] private VoidEventChannelSO onRestartLevel;
    [SerializeField] private VoidEventChannelSO onStopLevel;
    [SerializeField] private VoidEventChannelSO onQuitToDesktop;

    [Header("Output Events")]
    [SerializeField] private FloatEventChannelSO onTimePassed;

    public bool Ingame { get; private set; }
    public static PlayerDataSO PlayerData => Instance ? Instance.playerDataSO : null;
    public static ScoreTrackerSO ScoreTracker => Instance ? Instance.scoreTracker : null;
    
    
    private void OnEnable()
    {
        onStartLevel.OnEventRaised += LoadLevel;
        onQuitLevel.OnEventRaised += EndLevel;
        onStartLevel.OnEventRaised += StopLevel;
        onRestartLevel.OnEventRaised += RestartLevel;
        onQuitToDesktop.OnEventRaised += QuitToDesktop;
    }
    private void OnDisable()
    {
        onStartLevel.OnEventRaised -= LoadLevel;
        onQuitLevel.OnEventRaised -= EndLevel;
        onStartLevel.OnEventRaised -= StopLevel;
        onRestartLevel.OnEventRaised -= RestartLevel;
        onQuitToDesktop.OnEventRaised -= QuitToDesktop;
    }

    private void Init()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
        
        
        ingameStats.Init();
        PlayerData.Init();
    }

    private void OnApplicationQuit()
    {
        ingameStats.Reset();
        PlayerData.Reset();
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
    public void LoadLevel()
    {
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.Ingame);
    }

    public void StartLevel()
    {
        Ingame = true;
        
        ingameStats.Start();
        PlayerData.Start();
        MissionManager.Instance.StartMission();

        print("Game Started");
    }
    private void EndLevel()
    { 
        
        print("Game Ended");

        StopLevel();
        
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.MainMenu);

    }

    // Logic from stopping all game logic
    private void StopLevel()
    {
        if (!Ingame)
            return;
        
        Ingame = false;
        
        ingameStats.Stop();
        
        if(MissionManager.Instance)
            MissionManager.Instance.EndMission();
    }

    private void RestartLevel()
    {
        print("Game Restarted");
        
        StopLevel();
        
        LoadLevel();
    }
    
    

    public void QuitToDesktop()
    {
        Application.Quit();
    }
    
}

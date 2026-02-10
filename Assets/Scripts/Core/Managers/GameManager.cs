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
    [SerializeField] private VoidEventChannelSO onStartMission;
    [SerializeField] private VoidEventChannelSO onQuitMission;
    [SerializeField] private VoidEventChannelSO onRestartMission;
    [SerializeField] private VoidEventChannelSO onQuitToDesktop;

    [Header("Output Events")]
    [SerializeField] private FloatEventChannelSO onTimePassed;

    public bool Ingame { get; private set; }
    public static PlayerDataSO PlayerData => Instance ? Instance.playerDataSO : null;
    public static ScoreTrackerSO ScoreTracker => Instance ? Instance.scoreTracker : null;
    
    
    private void OnEnable()
    {
        onStartMission.OnEventRaised += LoadGame;
        onQuitMission.OnEventRaised += EndGame;
        onRestartMission.OnEventRaised += RestartGame;
        onQuitToDesktop.OnEventRaised += QuitToDesktop;
    }
    private void OnDisable()
    {
        onStartMission.OnEventRaised -= LoadGame;
        onQuitMission.OnEventRaised -= EndGame;
        onRestartMission.OnEventRaised -= RestartGame;
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
    public void LoadGame()
    {
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.Ingame);
    }

    public void StartGame()
    {
        Ingame = true;
        
        ingameStats.Start();
        PlayerData.Init();
        MissionManager.Instance.StartMission();

        print("Game Started");
    }
    [Button]
    public void EndGame()
    { 
        
        print("Game Ended");

        StopGame();
        
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.MainMenu);

    }

    // Logic from stopping all game logic
    private void StopGame()
    {
        Ingame = false;
        
        ingameStats.Stop();
        
        if(MissionManager.Instance)
            MissionManager.Instance.StopMission();
    }

    public void RestartGame()
    {
        print("Game Restarted");
        
        StopGame();
        
        LoadGame();
    }
    
    

    public void QuitToDesktop()
    {
        Application.Quit();
    }
    
}

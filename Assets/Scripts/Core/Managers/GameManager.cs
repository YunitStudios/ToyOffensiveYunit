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

    private void OnDestroy()
    {
        Stop();
    }

    #endregion
    
    [Header("References")]
    [SerializeField] private IngameStats ingameStats;
    [SerializeField] private ScoreTrackerSO scoreTracker;
    [SerializeField] private PlayerDataSO playerDataSO;

    [Header("Input Events")] 
    [SerializeField] private VoidEventChannelSO onStartMission;
    [SerializeField] private VoidEventChannelSO onQuitMission;
    [SerializeField] private VoidEventChannelSO onQuitToDesktop;

    [Header("Output Events")]
    [SerializeField] private FloatEventChannelSO onTimePassed;

    public bool Ingame { get; private set; }
    public static PlayerDataSO PlayerData => Instance.playerDataSO;
    public static ScoreTrackerSO ScoreTracker => Instance.scoreTracker;
    
    
    private void OnEnable()
    {
        onStartMission.OnEventRaised += LoadGame;
        onQuitMission.OnEventRaised += EndGame;
        onQuitToDesktop.OnEventRaised += QuitToDesktop;
    }
    private void OnDisable()
    {
        onStartMission.OnEventRaised -= LoadGame;
        onQuitMission.OnEventRaised -= EndGame;
        onQuitToDesktop.OnEventRaised -= QuitToDesktop;
    }

    private void Init()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
        ingameStats.Start();
        PlayerData.Init();
    }

    private void Start()
    {
        if(MissionManager.Instance)
            MissionManager.Instance.StartMission();
    }

    private void Stop()
    {
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
        PlayerData.Init();
        MissionManager.Instance.StartMission();

        print("Game Started");
    }
    [Button]
    public void EndGame()
    { 
        Ingame = false;
        
        ingameStats.Stop();
        
        print("Game Ended");
        
        if(MissionManager.Instance)
            MissionManager.Instance.StopMission();
        
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.MainMenu);

    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
    
}

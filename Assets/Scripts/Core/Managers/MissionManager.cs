using System;
using EditorAttributes;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    #region Singleton
    public static MissionManager Instance { get; private set; }
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
        
        ManagerAwake();
    }



    private void OnDestroy()
    {
        ManagerDestroy();
    }

    private void OnApplicationQuit()
    {
        currentMission.Clear();
    }

    #endregion

    [SerializeField] private MissionSO currentMission;
    public static MissionSO CurrentMission => Instance.currentMission;

    [Title("\n<b><color=#8880ff>Callbacks", 15, 5, false)]
    [SerializeField] private VoidEventChannelSO onMissionStart;
    [SerializeField] private VoidEventChannelSO onMissionComplete;
    [SerializeField] private VoidEventChannelSO onMissionEnd;
    
    private void ManagerAwake()
    {
    }
    
    private void ManagerDestroy()
    {
    }

    public void StartMission()
    {
        currentMission.Init();

        currentMission.MainObjective.OnObjectiveCompleted += CompleteMission;
        
        onMissionStart?.Invoke();
    }

    public void StopMission()
    {
        currentMission.Clear();
        
        // Run fail trigger on any uncompleted Trigger objectives without fail trigger
        foreach (var bonusObjective in currentMission.BonusObjectives)
        {
            if(bonusObjective is TriggerObjectivesSO { HasFailTrigger: false, Completed: false } triggerObjective)
                triggerObjective.TriggeredFail();
        }
        
        onMissionEnd?.Invoke();
    }

    private void CompleteMission()
    {
        onMissionComplete?.Invoke();
        
        // Run complete trigger on any non-failed Trigger based objectives without complete trigger
        foreach (var bonusObjective in currentMission.BonusObjectives)
        {
            if(bonusObjective is TriggerObjectivesSO { HasCompleteTrigger: false, Failed: false } triggerObjective)
                triggerObjective.TriggeredComplete();
        }
    }
}

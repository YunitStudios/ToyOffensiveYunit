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

    [Title("\n<b><color=#8880ff>Output Callbacks", 15, 5, false)] 
    [SerializeField] private Vector3EventChannelSO onTeleportPlayer;
    [SerializeField] private VoidEventChannelSO onMissionStart;
    [SerializeField] private VoidEventChannelSO onMissionComplete;
    [SerializeField] private VoidEventChannelSO onMissionEnd;

    [Title("\n<b><color=#8880ff>Input Callbacks", 15, 5, false)] 
    [SerializeField] private VoidEventChannelSO onLevelFail;
    
    
    // Notes:
    // Mission Start = Scene Load
    // Mission Complete = Mission Conditions Met
    // Mission End = Scene Unload
    // Mission Fail = Complete conditions no longer possible (player died, hostage died etc)
    
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
        
        onTeleportPlayer?.Invoke(currentMission.GetStartPosition);
    }

    public void EndMission()
    {
        currentMission.Clear();
        
        // Run fail trigger on any uncompleted Trigger objectives without fail trigger
        // E.G. Complete a mission without triggering an alarm
        foreach (var bonusObjective in currentMission.BonusObjectives)
        {
            if(bonusObjective is TriggerObjectivesSO { HasFailTrigger: false, Completed: false } triggerObjective)
                triggerObjective.TriggeredFail();
        }
        // Run complete trigger on any non-failed Trigger based objectives without complete trigger
        foreach (var bonusObjective in currentMission.BonusObjectives)
        {
            if(bonusObjective is TriggerObjectivesSO { HasCompleteTrigger: false, Failed: false } triggerObjective)
                triggerObjective.TriggeredComplete();
        }
        
        onMissionEnd?.Invoke();
    }

    private void CompleteMission()
    {
        onMissionComplete?.Invoke();

    }

    private void FailMission()
    {
        // Fail all objectives
        currentMission.MainObjective.FailObjective();
        
        foreach(var obj in currentMission.BonusObjectives)
            obj.FailObjective();
        
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/TriggerObjectives", fileName = "TriggerObjectives")]
public class TriggerObjectivesSO : CoreObjectiveSO
{
    [Header("Triggers")] 
    [SerializeField, Tooltip("If triggered, objective is completed. If left empty, automatically triggers when main objective is completed")] private VoidEventChannelSO completeTrigger;
    [SerializeField, Tooltip("If triggered, objective is failed. If left empty, automatically triggers on game end if not completed")] private VoidEventChannelSO failTrigger;
    public override string ObjectiveText => objectiveDesc;
    public override string ProgressText => "";

    public bool HasCompleteTrigger => completeTrigger != null;
    public bool HasFailTrigger => failTrigger != null;
    
    public override void SetupObjective()
    {
        base.SetupObjective();
        if(HasCompleteTrigger)
            completeTrigger.OnEventRaised += TriggeredComplete;
        if(HasFailTrigger)
            failTrigger.OnEventRaised += TriggeredFail;
    }

    public override void ResetObjective()
    {
        base.ResetObjective();
        if(HasCompleteTrigger)
            completeTrigger.OnEventRaised -= TriggeredComplete;
        if(HasFailTrigger)
            failTrigger.OnEventRaised -= TriggeredFail;
    }

    public void TriggeredComplete()
    {
        CompleteObjective();
    }
    
    public void TriggeredFail()
    {
        FailObjective();
    }


}

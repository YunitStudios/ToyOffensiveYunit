using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/MultiTriggerObjectives", fileName = "MultiTriggerObjectives")]
public class MultiTriggerObjectivesSO : CoreObjectiveSO
{
    public override string ObjectiveText => objectiveDesc;
    public override string ProgressText => "";

    [Header("Triggers Objectives")] 
    [SerializeField] private TriggerObjectivesSO[] triggers;
    
    public override void SetupObjective()
    {
        base.SetupObjective();
        foreach (var trigger in triggers)
        {
            trigger.SetupObjective();
            trigger.OnObjectiveCompleted += CheckCompletion;
            trigger.OnObjectiveFailed += TriggeredFail;
        }
    }

    public override void ResetObjective()
    {
        base.ResetObjective();
        foreach (var trigger in triggers)
        {
            trigger.ResetObjective();
            trigger.OnObjectiveCompleted -= CheckCompletion;
            trigger.OnObjectiveFailed -= TriggeredFail;
        }
    }

    private void CheckCompletion()
    {
        foreach (var trigger in triggers)
        {
            if (!trigger.Completed)
                return;
        }
        TriggeredComplete();
    }

    private void TriggeredComplete()
    {
        CompleteObjective();
    }
    
    private void TriggeredFail()
    {
        FailObjective();
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/NoNonTargetEnemyKillsObjectives", fileName = "NoNonTargetEnemyKillsObjectives")]
public class NoNonTargetEnemyKillsObjectiveSO : TargetObjectivesSO
{
    public override void SetupObjective()
    {
        base.SetupObjective();
        AIController.OnEnemyKilled += OnEnemyKilled;
    }



    public override void ResetObjective()
    {
        base.ResetObjective();
        AIController.OnEnemyKilled -= OnEnemyKilled;
    }
    
    private void OnEnemyKilled(IObjectiveTarget enemy)
    {
        // Check if the enemy was a target
        bool notTarget = true;
        foreach (var target in targetStates.Keys)
        {
            if (target.Instance == enemy)
                notTarget = false;
        }

        if (notTarget)
        {
            FailObjective();
            OnObjectiveUpdated?.Invoke();
        }
    }
}

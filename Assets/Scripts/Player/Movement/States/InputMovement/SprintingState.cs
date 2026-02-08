using System;
using UnityEngine;

[Serializable]
public class SprintingSettings : StateSettings
{
    [Header("Sprinting")]
    [Tooltip("Speed multiplier when sprinting")]
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    public float SprintSpeedMultiplier => sprintSpeedMultiplier;
}

public class SprintingState : InputMoveState
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SprintingState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    private new SprintingSettings Settings => stateMachine.SprintingSettings;
    
    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        AddCanEnterCondition(CanSprint);
    }

    public override void OnEnter()
    {
        base.OnEnter();

    }

    public override void OnExit()
    {
    }
    public override void Tick()
    {
        base.Tick();
    }

    public override void CheckTransitions()
    {
        base.CheckTransitions();
        
        if(!stateMachine.InputController.IsSprinting)
            SwitchState(stateMachine.WalkingState);
        
        // Only slide if not running into a wall
        if(stateMachine.InputController.CrouchHeld && !stateMachine.IsFacingWall())
            SwitchState(stateMachine.SlidingState);
        
        // Stop sprinting if input is below threshold
        if(!CanSprint())
            SwitchState(stateMachine.WalkingState);
    }
    
    private bool CanSprint()
    {
        return stateMachine.InputController.FrameMove.y > base.Settings.ForwardInputThreshold;
    }

    public override float GetSpeedMultiplier => Settings.SprintSpeedMultiplier;
}

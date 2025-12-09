using System;
using UnityEngine;

[Serializable]
public class WalkingSettings : StateSettings
{
    [Header("Walking")]
    [Tooltip("Speed multiplier when walking")]
    [SerializeField] private float walkSpeedMultiplier = 1f;
    public float WalkSpeedMultiplier => walkSpeedMultiplier;
}

public class WalkingState : InputMoveState
{

    private new WalkingSettings Settings => stateMachine.WalkingSettings;
    
    public override float GetSpeedMultiplier => Settings.WalkSpeedMultiplier;
    

    public WalkingState(StateMachine stateMachine) : base(stateMachine)
    {
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
        
        if(stateMachine.InputController.IsSprinting)
            SwitchState(stateMachine.SprintingState);
        
        if(stateMachine.InputController.CrouchHeld)
            SwitchState(stateMachine.CrouchingState);
    }
    

}

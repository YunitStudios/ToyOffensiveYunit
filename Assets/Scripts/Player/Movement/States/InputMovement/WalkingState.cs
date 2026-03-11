using System;
using PrimeTween;
using UnityEngine;

[Serializable]
public class WalkingSettings : StateSettings
{
    [Header("Walking")]
    [Tooltip("Speed multiplier when walking")]
    [SerializeField] private float walkSpeedMultiplier = 1f;
    
    [Tooltip("Bandaid solution because entering this state while jumping/sprinting and holding aim would trigger ADS. a delay ensures it switches state before this can happen")]
    public float WalkSpeedMultiplier => walkSpeedMultiplier;
    [SerializeField] private float aimDelay = 0.1f;
    public float AimDelay => aimDelay;
}

public class WalkingState : InputMoveState
{

    private new WalkingSettings Settings => stateMachine.WalkingSettings;
    
    public override float GetSpeedMultiplier => Settings.WalkSpeedMultiplier;
    public WalkingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    private Tween aimTween;

    public override bool CanAim => !aimTween.isAlive;

    public override void OnEnter()
    {
        base.OnEnter();

        aimTween = Tween.Delay(Settings.AimDelay);

    }

    public override void OnExit()
    {
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void LateTick()
    {
        
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

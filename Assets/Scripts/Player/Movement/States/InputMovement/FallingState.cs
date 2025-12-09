using System;
using PrimeTween;
using UnityEngine;

[Serializable]
public class FallingSettings : StateSettings
{
    [Header("Falling")]
    [Tooltip("Speed multiplier when falling")]
    [SerializeField] private float fallingHorizontalSpeedMultiplier = 0.33f;
    public float FallingHorizontalSpeedMultiplier => fallingHorizontalSpeedMultiplier;
    [Tooltip("Minimum vertical velocity needed before considering the player to be falling")]
    [SerializeField] private float fallingVelocityThreshold = 0.5f;
    public float FallingVelocityThreshold => fallingVelocityThreshold;
    [Tooltip("Time taken to transition speed from previous to new speed")]
    [SerializeField] private float speedTransitionTime = 0.5f;
    public float SpeedTransitionTime => speedTransitionTime;
    [SerializeField] private Ease speedTransitionEase = Ease.OutQuad;
    public Ease SpeedTransitionEase => speedTransitionEase;
}

public class FallingState : InputMoveState
{
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    
    public new FallingSettings Settings => stateMachine.FallingSettings;
    public override float GetSpeedMultiplier => currentSpeedMultiplier;
    private float currentSpeedMultiplier = 1f;
    private float taretSpeedMultiplier = 1f;
    private float previousSpeedMultiplier = 1f;
    public override bool CanJump => false;

    public FallingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        taretSpeedMultiplier = Settings.FallingHorizontalSpeedMultiplier * previousSpeedMultiplier;
        
        // Tween current multiple from previous to target
        Tween.Custom(previousSpeedMultiplier, taretSpeedMultiplier, Settings.SpeedTransitionTime, value => currentSpeedMultiplier = value, Settings.SpeedTransitionEase);

        stateMachine.PlayerAnimator.CrossFadeInFixedTime("Falling", 0.2f);
        stateMachine.PlayerAnimator.SetBool(IsFalling, true);
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsFalling, false);
    }

    public override void Tick()
    {
        base.Tick();
    }
    public override void FixedTick()
    {
    }

    public override void CheckTransitions()
    {
        base.CheckTransitions();
        // Wait until character controller detects ground
        if(stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);

        if (stateMachine.InputController.JumpDown)
            SwitchState(stateMachine.ParachuteState);
    }
    
    public void SetSpeedMultiplier(float currentSpeedMultiplier)
    {
        previousSpeedMultiplier = currentSpeedMultiplier;
    }

}

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
    [Tooltip("Percentage of damage to deal based on velocity scale")]
    [SerializeField] private Vector2 fallDamageScale = new(0.1f, 1f);
    public Vector2 FallDamageScale => fallDamageScale;

    [Tooltip("Minimum and maximum velocity to calculate damage amount. X triggers the minimum damage, Y deals max damage")]
    [SerializeField] private Vector2 fallVelocityScale = new(3f, 10f);
    public Vector2 FallVelocityScale => fallVelocityScale;
    [SerializeField] private float animationBlendInTime = 0.5f;
    public float AnimationBlendInTime => animationBlendInTime;
    [SerializeField] private float animationBlendOutTime = 0.1f;
    public float AnimationBlendOutTime => animationBlendOutTime;
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

        stateMachine.PlayerAnimator.CrossFadeInFixedTime("Falling", Settings.AnimationBlendInTime);
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.CrossFadeInFixedTime("Moving", Settings.AnimationBlendOutTime);
    }

    public override void Tick()
    {
        base.Tick();
    }
    public override void FixedTick()
    {
    }

    public override void LateTick()
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

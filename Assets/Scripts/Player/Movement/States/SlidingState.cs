using System;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SlidingSettings : StateSettings
{
    [Header("Sliding")]
    [Tooltip("Speed multiplier when sliding")]
    [SerializeField] private float slideForwardSpeed = 1.5f;
    public float SlideForwardSpeed => slideForwardSpeed;
    [SerializeField] private float slideDuration = 1f;
    public float SlideDuration => slideDuration;
    [SerializeField] private AnimationCurve slideSpeedCurve;
    public AnimationCurve SlideSpeedCurve => slideSpeedCurve;
    [SerializeField] private float slideCooldown = 1f;
    public float SlideCooldown => slideCooldown;
    [Tooltip("Downwards velocity to apply while sliding. Prevents slopes from breaking")]
    [SerializeField] private float slideGravity = 5;
    public float SlideGravity => slideGravity;
}

public class SlidingState : MovementState
{
    private static readonly int IsSliding = Animator.StringToHash("IsSliding");
    
    private SlidingSettings Settings => stateMachine.SlidingSettings;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SlidingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    private float slideTime;
    private Tween durationTween;

    public override bool CanShoot => false;
    public override bool CanAim => false;
    public override bool UseGravity => false;
    private bool launched = false;


    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.ChangeHeight(stateMachine.CrouchingState.GetCrouchHeight);
        stateMachine.PlayerAnimator.SetBool(IsSliding, true);
        
        durationTween = Tween.Delay(Settings.SlideDuration);
        
        slideTime = 0f;
        
        SetDelay(Settings.SlideCooldown);
    }

    public override void OnExit()
    {
        stateMachine.ChangeHeightDefault();
        stateMachine.PlayerAnimator.SetBool(IsSliding, false);

        if(durationTween.isAlive)
            durationTween.Stop();
        
        // Reset Y velocity on exit
        if (!launched)
        {
            stateMachine.SetVelocity(new Vector3(stateMachine.CurrentVelocity.x, 0, stateMachine.CurrentVelocity.z));
        }
    }

    public override void Tick()
    {
        if (launched)
        {
            return;
        }
        Vector3 direction = stateMachine.Forward;
        float progress = slideTime / Settings.SlideDuration;
        float speedMultiplier = Settings.SlideSpeedCurve.Evaluate(1-progress);
        
        Vector3 slideVelocity = direction * (Settings.SlideForwardSpeed * speedMultiplier);
        // Add downwards motion for slopes
        slideVelocity += Vector3.down * Settings.SlideGravity;
        stateMachine.SetVelocity(slideVelocity);
        
        slideTime += Time.deltaTime;
    }

    public override void LateTick()
    {
        
    }

    public override void CheckTransitions()
    {
        // End of slide
        if(slideTime >= Settings.SlideDuration)
        {
            SwitchState(stateMachine.CrouchingState);
            return;
        }
        
        // Cancel slide
        if(!stateMachine.InputController.CrouchHeld)
        {
            // If they cant stand up, go to crouch
            if(!stateMachine.CrouchingState.CanStandUp())
                SwitchState(stateMachine.CrouchingState);
            else if(stateMachine.InputController.IsSprinting)
                SwitchState(stateMachine.SprintingState);
            else
                SwitchState(stateMachine.WalkingState);
        }
        
        // If they slide into a wall
        if (stateMachine.IsFacingWall() && stateMachine.ClimbingState.CanClimb())
        {
            SwitchState(stateMachine.ClimbingState);
        }
        
        // If they slide off a ledge
        if (!stateMachine.IsGrounded)
        {
            SwitchState(stateMachine.FallingState);
        }
    }

    public override void FixedTick()
    {
    }

    public void hasLaunched(bool hasLaunched)
    {
        launched = hasLaunched;
    }
}

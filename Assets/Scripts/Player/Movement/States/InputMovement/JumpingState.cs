using System;
using PrimeTween;
using UnityEngine;

[Serializable]
public class JumpingSettings : StateSettings
{
    [Header("Jumping")] 
    [SerializeField] private float jumpHeight;
    public float JumpHeight => jumpHeight;
    [Tooltip("Delay after jumping before it can switch state, prevents instant landing before leaving ground")]
    [SerializeField] private float jumpLandDelay = 0.2f;
    public float JumpLandDelay => jumpLandDelay;
    [SerializeField] private float rejumpDelay = 0.2f;
    public float RejumpDelay => rejumpDelay;

}

public class JumpingState : InputMoveState
{
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    
    private new JumpingSettings Settings => stateMachine.JumpingSettings;

    public override bool CanJump => false;
    public override bool CanShoot => true;
    public override bool CanAim => false;

    public JumpingState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    private Tween landDelayTween;
    private Tween rejumpDelayTween;

    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        AddCanEnterCondition(CanPlayerJump);
    }

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.PlayerAnimator.SetBool(IsJumping, true);
        
        // Reset vertical velocity and add jump velocity
        stateMachine.SetVelocity(new Vector3(stateMachine.CurrentVelocity.x, 0, stateMachine.CurrentVelocity.z));
        Vector3 jumpVelocity = Vector3.zero;
        jumpVelocity.y = Settings.JumpHeight;
        stateMachine.ImpulseVelocity(jumpVelocity);
        
        landDelayTween = Tween.Delay(Settings.JumpLandDelay);
        
        if(sprinting)
            stateMachine.PlayerCamera.CurrentFovMultiplier = stateMachine.SprintingSettings.FovMultiplier;
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsJumping, false);
        
        rejumpDelayTween = Tween.Delay(Settings.RejumpDelay);
        
        stateMachine.PlayerCamera.CurrentFovMultiplier = 1;
       
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

        if (stateMachine.IsGrounded && !landDelayTween.isAlive)
            SwitchState(stateMachine.WalkingState);
        
        // Parachute if far enough from ground
        if (stateMachine.InputController.JumpDown)
            SwitchState(stateMachine.ParachuteState);
    }

    public override float GetSpeedMultiplier => speedMultiplier;
    private float speedMultiplier;
    private bool sprinting;
    
    public void SetSpeedMultiplier(float currentSpeedMultiplier, bool sprinting = false)
    {
        speedMultiplier = currentSpeedMultiplier;
        this.sprinting = sprinting;
    }

    public override void FixedTick()
    {
    }
    
    private bool CanPlayerJump()
    {
        return !rejumpDelayTween.isAlive && !stateMachine.IsSlopeSliding;
    }
}

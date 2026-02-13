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

    public JumpingState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    private Tween landDelayTween;
    private Tween rejumpDelayTween;

    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        AddCanEnterCondition(() => !rejumpDelayTween.isAlive);
    }

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.PlayerAnimator.SetBool(IsJumping, true);
        
        Vector3 jumpVelocity = Vector3.zero;
        jumpVelocity.y = Settings.JumpHeight;
        stateMachine.AddVelocity(jumpVelocity);
        
        landDelayTween = Tween.Delay(Settings.JumpLandDelay);
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsJumping, false);
        
        rejumpDelayTween = Tween.Delay(Settings.RejumpDelay);
       
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
    
    public void SetSpeedMultiplier(float currentSpeedMultiplier)
    {
        speedMultiplier = currentSpeedMultiplier;
    }

    public override void FixedTick()
    {
    }
}

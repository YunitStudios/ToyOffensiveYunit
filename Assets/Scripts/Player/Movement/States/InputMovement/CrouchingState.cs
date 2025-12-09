using System;
using UnityEngine;

[Serializable]
public class CrouchingSettings : StateSettings
{
    [Header("Crouching")]
    [Tooltip("Speed multiplier when crouching")]
    [SerializeField] private float crouchingSpeedMultiplier = 0.5f;
    public float CrouchingSpeedMultiplier => crouchingSpeedMultiplier;
    [Tooltip("Multiplier for collider height when crouching")]
    [SerializeField] private float crouchingHeightMultiplier = 0.5f;
    public float CrouchingHeightMultiplier => crouchingHeightMultiplier;
}

public class CrouchingState : InputMoveState
{
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    private new CrouchingSettings Settings => stateMachine.CrouchingSettings;
    
    public CrouchingState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public float GetCrouchHeight => stateMachine.PlayerHeight * Settings.CrouchingHeightMultiplier;
    

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.ChangeHeight(GetCrouchHeight);
        stateMachine.PlayerAnimator.SetBool(IsCrouching, true);
    }

    public override void OnExit()
    {
        stateMachine.ChangeHeightDefault();
        stateMachine.PlayerAnimator.SetBool(IsCrouching, false);
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
        
        if(!stateMachine.InputController.CrouchHeld && CanStandUp())
            SwitchState(stateMachine.WalkingState);
    }
    
    // Check for collisions above the player to see if they can stand up
    public bool CanStandUp()
    {
        float defaultHeight = stateMachine.PlayerHeight;
        Vector3 start = stateMachine.Position + Vector3.up * GetCrouchHeight;
        Vector3 end = stateMachine.Position + Vector3.up * defaultHeight;
        float radius = stateMachine.PlayerRadius * 0.9f;

        // Check for collisions using a capsule cast
        return !Physics.CheckCapsule(start, end, radius, stateMachine.EnvironmentLayer);
    }

    public override float GetSpeedMultiplier => Settings.CrouchingSpeedMultiplier;
}

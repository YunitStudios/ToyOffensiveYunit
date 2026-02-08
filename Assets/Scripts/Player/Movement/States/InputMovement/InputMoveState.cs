using System;
using UnityEngine;

[Serializable]
public class MovementSettings : StateSettings
{
    [Header("Movement")]
    [Tooltip("Base movement speed of the player")]
    [SerializeField] private float maxSpeed = 2f;
    public float MaxSpeed => maxSpeed;
    [Tooltip("Speed added to player each frame when moving")]
    [SerializeField] private float acceleration = 20f;
    public float Acceleration => acceleration;
    [Tooltip("Maximum speed change allowed per frame")]
    [SerializeField] private float maxAcceleration = 20f;
    public float MaxAcceleration => maxAcceleration;
    [Tooltip("Speed at which the player turns to face movement direction")]
    [SerializeField] private float turnSpeed = 1f;
    public float TurnSpeed => turnSpeed;
    [Tooltip("Minimum forward input required to be considered moving forward (used in sprinting check for example)")]
    [SerializeField] private float forwardInputThreshold = 0.6f;
    public float ForwardInputThreshold => forwardInputThreshold;
}

public abstract class InputMoveState : MovementState
{
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
    
    public MovementSettings Settings =>stateMachine.MovementSettings; 
    
    protected InputMoveState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public virtual bool CanJump => true;
    
    public abstract float GetSpeedMultiplier { get; }

    public void ApplyInputMovement()
    {
        Vector3 input = stateMachine.InputController.FrameMove;
        
        // Calculate movement direction relative to camera
        Vector3 cameraForward = stateMachine.PlayerCamera.CameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Vector3 cameraRight = stateMachine.PlayerCamera.CameraTransform.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();
        
        Vector3 moveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;
        
        //// Face movement direction
        //if(moveDirection.magnitude > 0.1f)
        //{
        //    //// Only if direction is forwards
        //    //if (Vector3.Dot(moveDirection, transform.forward) > 0f)
        //    //{
        //        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        //    //}
        //}

        float speedFactor = 1f;
        
        speedFactor *= GetSpeedMultiplier;
        
        
        
        Vector3 targetVelocity = moveDirection * (Settings.MaxSpeed * speedFactor);
        
        Vector3 currentVelocity = stateMachine.CurrentVelocity;
        
        // Preserving vertical velocity
        float verticalVelocity = currentVelocity.y;

        // Smooth acceleration 
        Vector3 horizontalVelocity = Vector3.MoveTowards(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            new Vector3(targetVelocity.x, 0, targetVelocity.z),
            Settings.Acceleration * Time.deltaTime);
        
        
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, Settings.MaxAcceleration);
        
        // Walking animation based on current speed
        float horizontalSpeed = horizontalVelocity.magnitude;
        // Divide by max speed to get 0-1 range
        horizontalSpeed /= Settings.MaxSpeed;
        // Half to fit walking blend tree
        horizontalSpeed /= 2f;
        // If sprinting, remove multiplier and instead double to get full speed
        if (stateMachine.InputController.IsSprinting)
        {
            horizontalSpeed /= stateMachine.SprintingSettings.SprintSpeedMultiplier;
            horizontalSpeed *= 2f;
        }
        
        stateMachine.PlayerAnimator.SetFloat(AnimMoveSpeed, horizontalSpeed, 0.1f, Time.deltaTime);
        stateMachine.PlayerAnimator.SetFloat(AnimMoveX, input.x, 0.1f, Time.deltaTime);
        stateMachine.PlayerAnimator.SetFloat(AnimMoveY, input.y, 0.1f, Time.deltaTime);
        
        stateMachine.SetVelocity(new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z));
    }

    public override void Tick()
    {
        ApplyInputMovement();
    }

    public override void FixedTick()
    {
    }

    public override void CheckTransitions()
    {
        if (stateMachine.ClimbingState.CanInitiateClimb() && stateMachine.ClimbingState.CanEnter() && stateMachine.InputController.FrameMove.y > 0f)
        {
            SwitchState(stateMachine.ClimbingState);
        }

        if (CanJump && stateMachine.IsGrounded && stateMachine.InputController.JumpHeld)
        {
            // Set jumping state multiplier to current speed multiplier
            stateMachine.JumpingState.SetSpeedMultiplier(GetSpeedMultiplier);
            SwitchState(stateMachine.JumpingState);
        }

        // Start falling if midair with sufficient downward velocity
        // Ignore if state is fallingstate
        if(!stateMachine.IsGrounded && stateMachine.CurrentVelocity.y < stateMachine.FallingState.Settings.FallingVelocityThreshold && this is not FallingState)
        {
            stateMachine.FallingState.SetSpeedMultiplier(GetSpeedMultiplier);
            SwitchState(stateMachine.FallingState);
        }
    }
}

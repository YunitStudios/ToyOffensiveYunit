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
    [SerializeField] private DirectionalSpeedMultiplier directionalSpeed;
    public DirectionalSpeedMultiplier DirectionalSpeed => directionalSpeed;
    [Tooltip("% of movement speed while aiming down sight")]
    [SerializeField] private float aimingMovementPercentage = 0.5f;
    public float AimingMovementPercentage => aimingMovementPercentage;
    
    
    [Header("Jumping")]
    [Tooltip("Time after leaving ground that player can still jump")]
    [SerializeField] private float coyoteTime = 0.2f;
    public float CoyoteTime => coyoteTime;
    
    [Serializable]
    public struct DirectionalSpeedMultiplier
    {
        public float forward;
        public float backward;
        public float strafe;
    }

    [Header("Animation")] 
    [Tooltip("Min and Max speed the moving animation can be, the speed is based on their current speed compared to the max speed")]
    [SerializeField] private Vector2 moveAnimSpeedRange = new(0f, 1f);
    public Vector2 MoveAnimSpeedRange => moveAnimSpeedRange;
    
}

public abstract class InputMoveState : MovementState
{
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int AnimMoveSpeedModified = Animator.StringToHash("MoveSpeedModified");
    private static readonly int AnimMoveX = Animator.StringToHash("MoveX");
    private static readonly int AnimMoveY = Animator.StringToHash("MoveY");
    
    public MovementSettings Settings =>stateMachine.MovementSettings; 
    
    protected InputMoveState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public virtual bool CanJump => true;
    public override bool RotatePlayerVertically => true;

    public abstract float GetSpeedMultiplier { get; }

    private float currentAirTime;

    public void ApplyInputMovement()
    {
        // Skip if player is dead
        if (GameManager.PlayerData && !GameManager.PlayerData.IsAlive)
        {
            // Remove all input velocity
            stateMachine.SetVelocity(new Vector3(0, stateMachine.CurrentVelocity.y, 0));
            return;
        }
        
        Vector3 input = stateMachine.InputController.FrameMove;
        
        float forwardMultiplier = input.y > 0 ? Settings.DirectionalSpeed.forward : Settings.DirectionalSpeed.backward;
        float strafeMultiplier = Settings.DirectionalSpeed.strafe;

        // Apply directional multipliers
        Vector2 scaledInput = new Vector2(
            input.x * strafeMultiplier,
            input.y * forwardMultiplier
        );
        
        // Calculate movement direction relative to camera
        Vector3 cameraForward = stateMachine.PlayerCamera.CameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Vector3 cameraRight = stateMachine.PlayerCamera.CameraTransform.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Convert input to world space
        Vector3 moveDirection =
            cameraRight * scaledInput.x +
            cameraForward * scaledInput.y;
        
        float speedFactor = 1f;

        speedFactor *= GetSpeedMultiplier;
        speedFactor *= stateMachine.MovementMultiplier;
        speedFactor *= stateMachine.PlayerData.IsAiming ? Settings.AimingMovementPercentage : 1;
        
        
        
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
        
        stateMachine.SetVelocity(new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z));
        
        
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
        
        stateMachine.PlayerAnimator.SetFloat(AnimMoveSpeed, horizontalSpeed);
        float modifiedSpeed = horizontalSpeed * 2f;
        // Keep in range
        modifiedSpeed = Mathf.Lerp(Settings.MoveAnimSpeedRange.x, Settings.MoveAnimSpeedRange.y, modifiedSpeed);
        stateMachine.PlayerAnimator.SetFloat(AnimMoveSpeedModified, modifiedSpeed, 0.1f, Time.deltaTime);
        stateMachine.PlayerAnimator.SetFloat(AnimMoveX, input.x, 0.1f, Time.deltaTime);
        stateMachine.PlayerAnimator.SetFloat(AnimMoveY, input.y, 0.1f, Time.deltaTime);
    }

    public override void Tick()
    {
        ApplyInputMovement();
        
        // Coyote Time
        if (!stateMachine.IsGrounded)
        {
            currentAirTime += Time.deltaTime;
        }
        else
        {
            currentAirTime = 0f;
        }
        
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
        if (CanJump && stateMachine.InputController.JumpHeld && currentAirTime <= Settings.CoyoteTime)
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

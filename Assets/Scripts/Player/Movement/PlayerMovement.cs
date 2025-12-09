using System;
using PrimeTween;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Used https://github.com/nskoczylas/FSM-Movement for reference and inspiration

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : StateMachine
{
    private CharacterController cc;
    private CapsuleCollider col;
    
    [Header("Components")]
    [SerializeField] private Animator playerAnimator;
    public Animator PlayerAnimator => playerAnimator;
    [SerializeField] private Transform thirdPersonTracker;
    public Transform ThirdPersonTracker => thirdPersonTracker;
    [FormerlySerializedAs("playerCamera")] [FormerlySerializedAs("playerCameras")] [SerializeField] private PlayerCamera playerCamera;
    public PlayerCamera PlayerCamera => playerCamera;
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private Transform visualRoot;
    public Transform VisualRoot => visualRoot;
    public Vector3 Position => transform.position;
    public Quaternion Rotation => rotationRoot.rotation;
    public Vector3 RotationEuler => rotationRoot.eulerAngles;
    public Vector3 LocalRotationEuler => rotationRoot.localEulerAngles;
    public Vector3 Forward => rotationRoot.forward;
    public Vector3 Right => rotationRoot.right;
    public Vector3 Up => rotationRoot.up;
    public InputManager InputController => InputManager.Instance;


    [Header("Attributes")] 
    [Tooltip("Height of the player character, used in things like climbing checks")]
    [SerializeField] private float playerHeight = 2f;
    public float PlayerHeight => playerHeight;
    [SerializeField] private float playerRadius = 0.5f;
    public float PlayerRadius => playerRadius;
    [Tooltip("Normalized height of the player's head relative to total height (0 = feet, 1 = top of head)")]
    [Range(0,1)] [SerializeField] private float playerHeadNormalizedHeight = 0.9f;
    public float PlayerHeadNormalizedHeight => playerHeadNormalizedHeight;
    public float PlayerHeadHeight => playerHeight * PlayerHeadNormalizedHeight;
    [Tooltip("Distance from ground to be considered grounded")]
    [SerializeField] private float minGroundDistance = 0.15f;
    public float MinGroundDistance => minGroundDistance;
    [Tooltip("Speed that gravity will push you down a slope")]
    [SerializeField] private float slopeSlideSpeed = 2;
    public float SlopeSlideSpeed => slopeSlideSpeed;

    [SerializeField] private LayerMask environmentLayer;
    public LayerMask EnvironmentLayer => environmentLayer;
    [SerializeField] private MovementSettings movementSettings = new MovementSettings();
    public MovementSettings MovementSettings => movementSettings;
    
    [SerializeField] private WalkingSettings walkingSettings = new WalkingSettings();
    public WalkingSettings WalkingSettings => walkingSettings;
    [SerializeField] private SprintingSettings sprintingSettings = new SprintingSettings();
    public SprintingSettings SprintingSettings => sprintingSettings;
    [SerializeField] private CrouchingSettings crouchingSettings = new CrouchingSettings();
    public CrouchingSettings CrouchingSettings => crouchingSettings;
    [SerializeField] private SlidingSettings slidingSettings = new SlidingSettings();
    public SlidingSettings SlidingSettings => slidingSettings;

    [SerializeField] private ClimbingSettings climbingSettings = new ClimbingSettings();
    public ClimbingSettings ClimbingSettings => climbingSettings;
    [SerializeField] private JumpingSettings jumpingSettings = new JumpingSettings();
    public JumpingSettings JumpingSettings => jumpingSettings;
    [SerializeField] private FallingSettings fallingSettings = new FallingSettings();
    public FallingSettings FallingSettings => fallingSettings;
    [SerializeField] private ParachutingSettings parachutingSettings = new ParachutingSettings();
    public ParachutingSettings ParachutingSettings => parachutingSettings;
    
    private WalkingState walkingState; 
    public WalkingState WalkingState => walkingState;
    private SprintingState sprintingState;
    public SprintingState SprintingState => sprintingState;
    private CrouchingState crouchingState;
    public CrouchingState CrouchingState => crouchingState;
    private ClimbingState climbingState;
    public ClimbingState ClimbingState => climbingState;
    private SlidingState slidingState;
    public SlidingState SlidingState => slidingState;
    private JumpingState jumpingState;
    public JumpingState JumpingState => jumpingState;
    private FallingState fallingState;
    public FallingState FallingState => fallingState;
    private ParachuteState parachuteState;
    public ParachuteState ParachuteState => parachuteState;
    

    
    public Vector3 CurrentVelocity => currentVelocity;
    private Vector3 currentVelocity;
    
    
    [Header("Looking")]
    [Tooltip("Multiplier to adjust look sensitivity")]
    [SerializeField] private float lookSensitivity = 0.1f;
    
    public bool MouseRotatePlayer => currentState is IMovementState { UseMouseRotatePlayer: true };




    
    private float defaultColliderHeight;
    private InputAxis.RecenteringSettings originalCameraRecentering;
    
    // Public Properties
    public bool IsGrounded => CheckOnGround(); 

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        
        cc = GetComponent<CharacterController>();
        col = GetComponent<CapsuleCollider>();
        // Copy character controller collider data to capsule collider
        cc.radius = playerRadius;
        cc.height = playerHeight;
        col.radius = playerRadius;
        col.height = playerHeight;
        col.center = cc.center;
        
        Cursor.lockState = CursorLockMode.Locked;
        
        SetupStates();
        
        defaultColliderHeight = cc.height;
    }
    
    private void SetupStates()
    {
        walkingState = new WalkingState(this);
        walkingState.Initialize();
        sprintingState = new SprintingState(this);
        sprintingState.Initialize();
        crouchingState = new CrouchingState(this);
        crouchingState.Initialize();
        climbingState = new ClimbingState(this);
        climbingState.Initialize();
        slidingState = new SlidingState(this);
        slidingState.Initialize();
        jumpingState = new JumpingState(this);
        jumpingState.Initialize();
        fallingState = new FallingState(this);
        fallingState.Initialize();
        parachuteState = new ParachuteState(this);
        parachuteState.Initialize();
        
        
        SwitchState(walkingState, null);
    }

    protected override void Update()
    {
        base.Update();
        
        // Rotation when no rigidbody
        FrameLook();
        
        ApplyVelocity();
    }
    

    private void ApplyVelocity()
    {
        // Apply gravity
        if (currentState is IMovementState { UseGravity: true })
        {
            currentVelocity.y += Physics.gravity.y * Time.deltaTime;
            
            // Set velocity to small negative value when grounded to prevent floating
            if (IsGrounded && currentVelocity.y < 0f)
            {
                currentVelocity.y = -1f; // Small negative value to keep grounded
            }
        }
        
        // Apply velocity for frame
        Vector3 finalVelocity = currentVelocity * Time.deltaTime;
        cc.Move(finalVelocity); 
    }

    
    private void OnAnimatorMove()
    {
        if (playerAnimator.applyRootMotion)
        {
            Vector3 animDeltaPosition = playerAnimator.deltaPosition;
            cc.Move(animDeltaPosition);
        }
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        currentVelocity = newVelocity;
    }
    public void AddVelocity(Vector3 addVelocity)
    {
        currentVelocity += addVelocity;
    }
    public void SetPosition(Vector3 newPosition)
    {
        cc.enabled = false;
        transform.position = newPosition;
        cc.enabled = true;
    }
    public void SetRotation(Quaternion newRotation)
    {
        rotationRoot.rotation = newRotation;
    }
    public void SetRotation(Vector3 eulerAngles)
    {
        rotationRoot.rotation = Quaternion.Euler(eulerAngles);
    }
    public Quaternion GetVisualRotation()
    {
        return visualRoot.localRotation;
    }
    public void SetVisualRotation(Quaternion newRotation)
    {
        visualRoot.localRotation = newRotation;
    }
    public void SetVisualRotation(Vector3 eulerAngles)
    {
        visualRoot.localRotation = Quaternion.Euler(eulerAngles);
    }
    
    public void ChangeHeight(float newHeight)
    {
        // Clamp height to double the radius minimum
        newHeight = Mathf.Max(newHeight, cc.radius * 2f);
        
        cc.height = newHeight;
        Vector3 center = cc.center;
        center.y = newHeight / 2f;
        cc.center = center;
    }
    public void ChangeHeightDefault()
    {
        ChangeHeight(defaultColliderHeight);
    }

    private void FrameLook()
    {
        Vector2 input = InputManager.Instance.FrameLook;
        
        Vector2 finalInput = input * (lookSensitivity * Time.deltaTime);
        
        
        // Rotate player Y axis
        if(MouseRotatePlayer)
            rotationRoot.Rotate(Vector3.up, finalInput.x);
        
        // If X or Z axis is not zero, slerp back to 0
        if (currentState is IMovementState { ControlRotation: false } 
            && (LocalRotationEuler.x != 0f || LocalRotationEuler.z != 0f))
        {
            Quaternion startRotation = rotationRoot.localRotation;
            Quaternion targetRotation = Quaternion.Euler(0f, LocalRotationEuler.y, 0f);
            rotationRoot.localRotation = Quaternion.Slerp(startRotation, targetRotation, 0.2f);
        }
        

        // Rotate third person tracker vertically
        thirdPersonTracker.transform.rotation *= Quaternion.AngleAxis(-finalInput.y, Vector3.right);
        
        // Clamp third person X axis
        Vector3 trackerEuler = thirdPersonTracker.localEulerAngles;
        trackerEuler.z = 0;
        float angle = trackerEuler.x;
        if (angle is > 180f and < 340f) angle = 340f;
        else if (angle is < 180f and > 40f) angle = 40f;
        trackerEuler.x = angle;
        thirdPersonTracker.localEulerAngles = trackerEuler;
        
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 hitNormal = hit.normal;
        #region Slope Sliding
        // Source: https://discussions.unity.com/t/character-controller-slide-down-slope/188130/2
        // Check if we are sliding
        var angle = Vector3.Angle(Vector3.up, hitNormal);
        bool isSliding = (angle > hit.controller.slopeLimit && angle < 85f);
        if (isSliding && !IsGrounded && currentVelocity.y <= 0f){
            {
                print("slide");
                var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitNormal);
                // Calculate speed based on rotation from up
                float slopeSpeed = (angle-45) / 45f; // Normalize between 0 and 1 from 45 to 90 degrees
                slopeSpeed++;
                
                var slopeVelocity = slopeRotation * new Vector3(hitNormal.x, 0f, hitNormal.z) * slopeSlideSpeed * slopeSpeed;
                currentVelocity = slopeVelocity;
            }

        }
        #endregion

    }
    
    public bool IsFacingWall(float distance = 1f)
    {
        Vector3 origin = Position + Vector3.up * PlayerHeight/2;
        Vector3 direction = Forward;
        return Physics.Raycast(origin, Forward, distance, EnvironmentLayer);
    }
    
    public float GetGroundDistance()
    {
        float sphereRadius = cc.radius * 0.9f;
        Vector3 sphereOrigin = transform.position + Vector3.up * (sphereRadius);
        float maxDistance = 100f;
        
        if(Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out var hitInfo, maxDistance, environmentLayer))
        {
            // // Make sure surface is flat
            if(Vector3.SignedAngle(hitInfo.normal, Vector3.up, Vector3.right) <= cc.slopeLimit)
                return hitInfo.distance;
        }
        return maxDistance;
        
    }

    // Spherecast to check if on ground
    private bool CheckOnGround()
    {
        if(GetGroundDistance() < minGroundDistance)
            return true;

        return false;
    }
    
}

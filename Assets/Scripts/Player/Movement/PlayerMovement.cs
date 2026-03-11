using System;
using PrimeTween;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Used https://github.com/nskoczylas/FSM-Movement for reference and inspiration

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : StateMachine
{
    public static readonly Vector3 NULL_POSITION = new Vector3(0,-1000,0);

    private CharacterController cc;
    private CapsuleCollider col;
    private PlayerHealth playerHealth;
    public bool IsAlive => playerHealth.IsAlive;
    
    [Header("Components")]
    [SerializeField] private Animator playerAnimator;
    public Animator PlayerAnimator => playerAnimator;
    [SerializeField] private Transform thirdPersonTracker;
    public Transform ThirdPersonTracker => thirdPersonTracker;
    [SerializeField] private Transform yawTracker;
    public Transform YawTracker => yawTracker;
    [FormerlySerializedAs("playerCamera")] [FormerlySerializedAs("playerCameras")] [SerializeField] private PlayerCamera playerCamera;
    public PlayerCamera PlayerCamera => playerCamera;
    [SerializeField] private PlayerDataSO playerData;
    public PlayerDataSO PlayerData => playerData;
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform aimingTarget;
    [SerializeField] private Rig aimingRig;
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
    [SerializeField] private float gravity = 15;
    public float Gravity => gravity;
    [Tooltip("Height of the player character, used in things like climbing checks")]
    [SerializeField] private float playerHeight = 2f;
    public float PlayerHeight => playerHeight;
    [SerializeField] private float playerRadius = 0.25f;
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
    [Tooltip("How much to offset the cameras look target from the players pivot")]
    [SerializeField] private float cameraHeightOffset = 0.5f;

    [Tooltip("How much to offset the cameras look target from the players pivot when crouched")]
    [SerializeField] private float crouchedCameraHeightOffset = 0f;

    [Tooltip("How much to move the aiming target so that the player looks slightly to the right and up to match what the camera sees")]
    [SerializeField] private Vector2 aimingShoulderOffset = new Vector2(3,1);

    [Tooltip("Speed to move the aiming target when you start aiming")] [SerializeField] private float aimingShoulderSpeed = 0.1f;

    public float GetCurrentCameraHeightOffset => currentState switch
    {
        global::CrouchingState => crouchedCameraHeightOffset,
        global::SlidingState => crouchedCameraHeightOffset,
        _ => cameraHeightOffset
    };

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
    [SerializeField] private ParachuteLandingSettings parachuteLandingSettings = new ParachuteLandingSettings();
    public ParachuteLandingSettings ParachuteLandingSettings => parachuteLandingSettings;
    
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
    private ParachuteLanding parachuteLandingState;
    public ParachuteLanding ParachuteLandingState => parachuteLandingState;
    

    
    public Vector3 CurrentVelocity => currentVelocity;
    private Vector3 currentVelocity;
    public Vector3 FrameVelocity => frameVelocity;
    private Vector3 frameVelocity;
    public float CurrentRadius => cc.radius;
    
    
    [Header("Looking")]
    [Tooltip("Multiplier to adjust look sensitivity")]
    [SerializeField] private float lookSensitivity = 0.1f;

    [Header("Input Events")] 
    [SerializeField] private FloatEventChannelSO onDealPlayerDamage;
    [SerializeField] private Vector3EventChannelSO onTeleportPlayer;
    [SerializeField] private VoidEventChannelSO onTryUnstuck;
    
    public void OnDealPlayerDamage(float damage) => onDealPlayerDamage?.Invoke(damage);
    
    public bool ShouldMouseRotatePlayer => currentState is IMovementState { UseMouseRotatePlayer: true };
    public bool ShouldMouseRotateVisuals => currentState is IMovementState { UseMouseRotateVisuals: true };


    public float MovementMultiplier { get; private set; }
    public float ClampedCameraYaw { get; private set; }

    
    private float defaultColliderHeight;
    private InputAxis.RecenteringSettings originalCameraRecentering;

    private bool movementFrozen;
    private bool hasCachedGroundCheck;
    private bool cachedGroundCheck;
    private Vector2 currentShoulderOffset;
    
    // Public Properties
    public bool IsGrounded
    {
        get
        {
            cachedGroundCheck = CheckOnGround();
            hasCachedGroundCheck = true;
            return cachedGroundCheck;
        }
    }

    public bool CanShoot => CheckCanShoot();
    public bool CanAim => CheckCanAim();
    public bool DisableSprinting { get; private set; }
    public bool IsSlopeSliding { get; private set; }

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        
        cc = GetComponent<CharacterController>();
        col = GetComponent<CapsuleCollider>();
        playerHealth = GetComponent<PlayerHealth>();
        // Copy character controller collider data to capsule collider
        cc.radius = playerRadius;
        cc.height = playerHeight;
        col.radius = playerRadius;
        col.height = playerHeight;
        col.center = cc.center;
        
        Cursor.lockState = CursorLockMode.Locked;
        
        SetupStates();
        
        defaultColliderHeight = cc.height;

        MovementMultiplier = 1;
    }

    private void OnEnable()
    {
        onTeleportPlayer.OnEventRaised += SetPosition;
        onTryUnstuck.OnEventRaised += OnTryUnstuck;
    }

    private void OnDisable()
    {
        onTeleportPlayer.OnEventRaised -= SetPosition;
        onTryUnstuck.OnEventRaised -= OnTryUnstuck;
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
        parachuteLandingState = new ParachuteLanding(this);
        parachuteLandingState.Initialize();
        
        
        SwitchState(walkingState, null);
    }

    protected override void Update()
    {
        if (movementFrozen || !playerHealth.IsAlive)
        {
            return;
        }
        
        base.Update();
        
        FrameLook();
        
        ApplyVelocity();

        if (GameManager.PlayerData)
        {
            GameManager.PlayerData.StorePosition(transform.position);
            GameManager.PlayerData.StoreRotationRootTransform(rotationRoot);
        }
           
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        hasCachedGroundCheck = false;
    }


    private void ApplyVelocity()
    {
        if (currentState is IMovementState { UseGravity: true })
        {
            currentVelocity.y += gravity * Time.deltaTime;

            // https://discussions.unity.com/t/character-controller-slide-down-slope/188130
            // Thanks claude for once you were actually helpful
            bool onSteepSlope = Vector3.Angle(Vector3.up, hitNormal) > cc.slopeLimit;
            bool nearGround = GetGroundDistance() < minGroundDistance*2;
            if (nearGround && onSteepSlope)
            {
                float slideFriction = 1f - slopeSlideSpeed / 10f;
                currentVelocity.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction);
                currentVelocity.z += (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction);
                IsSlopeSliding = true;
            }
            else if (IsGrounded && currentVelocity.y < 0f)
            {
                currentVelocity.y = -1f;
                IsSlopeSliding = false;
            }
        }

        Vector3 finalVelocity = (currentVelocity + frameVelocity) * Time.deltaTime;
        
        cc.Move(finalVelocity);
        frameVelocity = Vector3.zero;
    }
    
    public void SetVelocity(Vector3 newVelocity)
    {
        currentVelocity = newVelocity;
    }
    public void ImpulseVelocity(Vector3 addVelocity)
    {
        currentVelocity += addVelocity;
    }
    public void AddFrameVelocity(Vector3 addVelocity)
    {
        frameVelocity += addVelocity;
    }
    public void SetPosition(Vector3 newPosition)
    {
        if (newPosition == NULL_POSITION)
        {
            Debug.LogError("Attempted to teleport player to NULL_POSITION, ignoring.");
            return;
        }
        
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

    public Quaternion GetCameraRotation()
    {
        return thirdPersonTracker.rotation;
    }
    public void SetCameraRotation(Quaternion newRotation)
    {
        if (ClampedCameraYaw != 0)
        {
            float currentYaw = Rotation.eulerAngles.y;
            float targetYaw = newRotation.eulerAngles.y;

            float delta = Mathf.DeltaAngle(currentYaw, targetYaw);
            delta = Mathf.Clamp(delta, -ClampedCameraYaw, ClampedCameraYaw);

            float finalYaw = currentYaw + delta;

            Vector3 newEuler = newRotation.eulerAngles;
            newRotation = Quaternion.Euler(newEuler.x, finalYaw, newEuler.z);
        }

        thirdPersonTracker.rotation = newRotation;
    }
    public void ClampCameraYaw(float yaw)
    {
        ClampedCameraYaw = yaw;
    }
    
    public void ToggleCollision(bool enabled)
    {
        cc.excludeLayers = enabled ? 0 : ~0; // If enabled, collide with everything. If disabled, collide with nothing.
        col.enabled = enabled;
    }
    public void SetCollisionScale(Vector2 newScale, bool dontMoveCenter)
    {
        cc.radius = playerRadius * newScale.x;
        cc.height = playerHeight * newScale.y;
        
        col.radius = playerRadius * newScale.x;
        col.height = playerHeight * newScale.y;

        if (dontMoveCenter)
            return;
        Vector3 center = cc.center;
        center.y = cc.height / 2f;
        cc.center = center;
        col.center = center;
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
    public void ChangeRadius(float newRadius)
    {
        cc.radius = newRadius;
    }
    public void ChangeRadiusDefault()
    {
        ChangeRadius(playerRadius);
    }

    public void SetMovementFrozen(bool isFrozen)
    {
        movementFrozen = isFrozen;
    }

    private void FrameLook()
    {
        Vector2 input = InputManager.Instance.FrameLook;
        
        Vector2 finalInput = input * (lookSensitivity * Time.deltaTime);
        
        // If the player is alive rotate as normal
        if(GameManager.PlayerData && GameManager.PlayerData.IsAlive)
        {
            // Rotate player Y axis
            if (ShouldMouseRotatePlayer)
                rotationRoot.Rotate(Vector3.up, finalInput.x);

            // Rotate third person tracker vertically
            thirdPersonTracker.transform.rotation *= Quaternion.AngleAxis(-finalInput.y, Vector3.right);
            // Rotate tracker horizontally if visuals are affected
            if(ShouldMouseRotateVisuals)
                thirdPersonTracker.transform.rotation *= Quaternion.AngleAxis(finalInput.x, Vector3.up);
            
            SetCameraRotation(thirdPersonTracker.transform.rotation);
                
            
        }
        // If theyre dead, only rotate the third person tracker
        else
        {
            thirdPersonTracker.transform.rotation *= Quaternion.AngleAxis(-finalInput.y, Vector3.right);
            thirdPersonTracker.transform.rotation *= Quaternion.AngleAxis(finalInput.x, Vector3.up);
        }
        
        // If X or Z axis is not zero, slerp back to 0
        if (currentState is IMovementState { ControlRotation: false } 
            && (LocalRotationEuler.x != 0f || LocalRotationEuler.z != 0f))
        {
            Quaternion startRotation = rotationRoot.localRotation;
            Quaternion targetRotation = Quaternion.Euler(0f, LocalRotationEuler.y, 0f);
            rotationRoot.localRotation = Quaternion.Slerp(startRotation, targetRotation, 0.2f);
        }
        


        
        // Clamp third person X axis
        Vector3 trackerEuler = thirdPersonTracker.localEulerAngles;
        trackerEuler.z = 0;
        float angle = trackerEuler.x;
        if (angle is > 180f and < 340f) angle = 340f;
        else if (angle is < 180f and > 40f) angle = 40f;
        trackerEuler.x = angle;
        thirdPersonTracker.localEulerAngles = trackerEuler;
        
        // Copy yaw
        yawTracker.localEulerAngles = new Vector3(0, trackerEuler.y, 0);
        
        
        // Set trackers Y position
        Vector3 trackerPos = thirdPersonTracker.localPosition;
        trackerPos.y = GetCurrentCameraHeightOffset;
        thirdPersonTracker.localPosition = trackerPos;
        
        // Face player fowards
        Vector3 targetPosition = transform.position + Vector3.up * PlayerHeadHeight;
        
        // Only offset the shoulder while aiming
        if(playerData.IsAiming && currentShoulderOffset != aimingShoulderOffset)
            currentShoulderOffset = Vector2.Lerp(currentShoulderOffset, aimingShoulderOffset, aimingShoulderSpeed * Time.deltaTime);
        else if (!playerData.IsAiming && currentShoulderOffset != Vector2.zero)
            currentShoulderOffset = Vector2.Lerp(currentShoulderOffset, Vector2.zero, aimingShoulderSpeed * Time.deltaTime);
        
        // Offset the aim target to be around the shoulder
        targetPosition += rotationRoot.right * currentShoulderOffset.x + rotationRoot.up * currentShoulderOffset.y;
        
        
        // Move forwards to smoothen the rotation
        targetPosition += thirdPersonTracker.transform.forward * 10;;
        aimingTarget.transform.position = targetPosition;
        
    }
    
    public bool IsFacingWall(float distance = 1f)
    {
        Vector3 origin = Position + Vector3.up * PlayerHeight/2;
        Vector3 direction = Forward;
        return Physics.Raycast(origin, direction, distance, EnvironmentLayer);
    }
    
    public float GetGroundDistance()
    {
        float sphereRadius = cc.radius;
        Vector3 sphereOrigin = transform.position + Vector3.up * (sphereRadius);
        float maxDistance = 100f;
        
        if(Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out var hitInfo, maxDistance, environmentLayer))
        {
            return hitInfo.distance;
        }
        return maxDistance;
        
    }

    // Spherecast to check if on ground
    private bool CheckOnGround()
    {
        if(hasCachedGroundCheck)
            return cachedGroundCheck;
        
        // Landed
        if (GetGroundDistance() < minGroundDistance)
        {
            ApplyFallDamage();
            climbingState.ResetStamina();
            
            return true;
        }

        return false;
    }
    
    public bool IsFallingLethal()
    {
        float fallDamage = CheckFallDamage();
        
        return fallDamage >= playerHealth.CurrentHealth;
    }
    
    private float CheckFallDamage()
    {
        if (currentState is not global::FallingState) 
            return 0;


        float fallDistance =  Mathf.Abs(transform.position.y - fallingState.FallingStartHeight);
        
        if (fallDistance < FallingSettings.FallDistanceScale.x)
            return 0;
        
        float t = Mathf.InverseLerp(FallingSettings.FallDistanceScale.x, FallingSettings.FallDistanceScale.y, fallDistance);
        print(t);
        float damageScale = Mathf.Lerp(FallingSettings.FallDamageScale.x, FallingSettings.FallDamageScale.y, t);
        print(damageScale);
        return 100 * damageScale;
    }

    private void ApplyFallDamage()
    {
        float fallDamage = CheckFallDamage();
        OnDealPlayerDamage(fallDamage);
    }

    private bool CheckCanShoot()
    {
        //return currentState is IMovementState { CanADS: false };
        if (currentState is IMovementState state)
            return state.CanShoot;
        return true;
    }
    private bool CheckCanAim()
    {
        //return currentState is IMovementState { CanADS: false };
        if (currentState is IMovementState state)
            return state.CanAim;
        return true;
    }


    
    private void OnTryUnstuck()
    {
        // Check for nearest nav mesh position
        if (NavMesh.SamplePosition(transform.position, out var hit, float.MaxValue, NavMesh.AllAreas))
        {
            // Dont move if the player is already on the nav mesh
            if (hit.position == transform.position)
                return;
            
            SetPosition(hit.position);
        }
    }

    public void TemporaryMovementModifier(float duration, float multiplier, bool disableSprinting = false, bool disableJumping = false, float transitionTime = 0.0f)
    {
        DisableSprinting = disableSprinting;
        MovementMultiplier += multiplier;
        Tween.Delay(duration - transitionTime, () =>
        {
            if (transitionTime == 0)
            {
                MovementMultiplier -= multiplier;
                if(disableSprinting)
                    DisableSprinting = false;
            }
            else
            {
                // TODO
                // This actually breaks if anything else touches movement multiplier
                // Uh lets just ignore that for now
                
                float startValue = MovementMultiplier;
                float endValue = MovementMultiplier - multiplier;

                Tween.Custom(startValue, endValue, transitionTime, v =>
                {
                    MovementMultiplier = v;
                }).OnComplete(() =>
                {
                    if(disableSprinting)
                        DisableSprinting = false;
                });
            }
        });
    }

    public void ManualMovementModifier(float multiplier)
    {
        MovementMultiplier += multiplier;
    }
    public void TemporaryVelocityBoost(float duration, Vector3 localVelocity, AnimationCurve curve)
    {
        // Set frame velocity over time based on curve
        Tween.Delay(duration).OnUpdate(target: this, (target, tween) =>
        {
            Vector3 worldVelocity = target.rotationRoot.TransformDirection(localVelocity);
            AddFrameVelocity(worldVelocity * curve.Evaluate(tween.progress));
        });
    }
    
    
    
    
    
    private Vector3 hitNormal = Vector3.up;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }
    
}

using System;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using Plane = UnityEngine.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class ClimbingSettings : StateSettings
{
    [Header("Climbing")]
    [SerializeField] private LayerMask climbableLayer;
    public LayerMask ClimbableLayer => climbableLayer;
    [SerializeField, Tooltip("Distance to wall before starting climb. Multiplier by players radius")] private float climbRange = 1f;
    public float ClimbRange => climbRange;
    [FormerlySerializedAs("climbingAngleLimits")] [SerializeField] private Vector2 climbingVerticalAngleLimits = new(-40f, 40f);
    public Vector2 ClimbingVerticalAngleLimits => climbingVerticalAngleLimits;
    [SerializeField] private Vector2 climbingHorizontalAngleLimits = new(-40f, 40f);
    public Vector2 ClimbingHorizontalAngleLimits => climbingHorizontalAngleLimits;
    [SerializeField] private float climbVerticalSpeed = 1f;
    public float ClimbVerticalSpeed => climbVerticalSpeed;
    [SerializeField] private float climbHorizontalSpeed = 1f;
    public float ClimbHorizontalSpeed => climbHorizontalSpeed;
    [Tooltip("Initially to stop player from reclimbing immediately after jumping")]
    [SerializeField] private float climbingDelayAfterJump = 0.5f;
    public float ClimbingDelayAfterJump => climbingDelayAfterJump;
    [Tooltip("Delay before being able to enter climb state again")]
    [SerializeField] private float climbingRetryDelay = 0.5f;
    public float ClimbingRetryDelay => climbingRetryDelay;
    [Tooltip("How far the player is from the wall while climbing. Choose a value from X to Y based on the climbDistanceFromWallRange value and the current angle.")]
    [SerializeField] private Vector2 climbDistanceFromWall = new(0.33f,0.75f);
    public Vector2 ClimbDistanceFromWall => climbDistanceFromWall;
    [Tooltip("Min and max angle to use when checking what distance from the wall to use")]
    [SerializeField] private Vector2 climbDistanceFromWallRange = new(0f,40f);
    public Vector2 ClimbDistanceFromWallRange => climbDistanceFromWallRange;
    [Tooltip("Width of player from their head when checking for side climbing space")]
    [SerializeField] private float climbWidth = 0.5f;
    public float ClimbWidth => climbWidth;
    [Tooltip("Maximum gap distance that the player can ignore and climb over. A gap is a part of the wall which either moves inwards or just isn't there")]
    [SerializeField] private float climbingMaxGapDistance = 0.5f;
    public float ClimbingMaxGapDistance => climbingMaxGapDistance;
    [Tooltip("Speed that it takes for the player to lock onto the current wall normal")] 
    [SerializeField] private float climbingWallLockSpeed = 10f;
    public float ClimbingWallLockSpeed => climbingWallLockSpeed;
    [Tooltip("How long it takes to lock the player to the wall at the start of a climb")] 
    [SerializeField] private float climbingStartWallLockTime = 0.25f;
    public float ClimbingStartLockIntoPlace => climbingStartWallLockTime;
    
    
    
    
    [Header("Vaulting")]
    [Tooltip("How far forward from the ledge to vault forward")]
    [SerializeField] private float climbVaultDistance = 0.5f;
    public float ClimbVaultDistance => climbVaultDistance;
    [Tooltip("The maximum height the player can vault upwards over a ledge. Usually pointless but useful in scenarios where the edge is unusual")]
    [SerializeField] private float climbMaxVaultHeight = 1f;
    public float ClimbMaxVaultHeight => climbMaxVaultHeight;
    [Tooltip("How long the vault process takes")]
    [SerializeField] private float climbVaultDuration = 1f;
    public float ClimbVaultDuration => climbVaultDuration;
    [Tooltip("Control vertical speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingVertical;
    public AnimationCurve ClimbVaultEasingVertical => climbVaultEasingVertical;
    [Tooltip("Control horizontal speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingHorizontal;
    public AnimationCurve ClimbVaultEasingHorizontal => climbVaultEasingHorizontal;


    
    
    [Header("Hanging")]
    [Tooltip("Brief delay before being able to stop hanging")]
    [SerializeField] private float unhangDelay = 0.4f;
    public float UnhangDelay => unhangDelay;
    [Tooltip("Delay before player can rehang after falling")]
    [SerializeField] private float rehangDelay = 0.2f;
    public float RehangDelay => rehangDelay;
    [Tooltip("Distance from the top of the wall at which entering climb will automatically hang")]
    [SerializeField] private float autoTriggerHangDistance = 1f;
    public float AutoTriggerHangDistance => autoTriggerHangDistance;
    
    [Header("Up Leap")]
    [Tooltip("Cooldown before being able to sprint leap again")]
    [SerializeField] private float climbSprintLeapCooldown = 2f;
    public float ClimbSprintLeapCooldown => climbSprintLeapCooldown;
    [Tooltip("Percentage of stamina to use when performing a sprint leap")]
    [SerializeField] private float climbSprintLeapStaminaPercentage = 0.2f;
    public float ClimbSprintLeapStaminaPercentage => climbSprintLeapStaminaPercentage;
    [Tooltip("Padding of extra stamina needed to be able to sprint leap, prevents sprint leaping and then immediately falling because you have no stamina")]
    [SerializeField] private float climbSprintLeapStaminaPadding = 0.1f;
    public float ClimbSprintLeapStaminaPadding => climbSprintLeapStaminaPadding;
    
    
    [Header("Climbing Stamina")]
    [SerializeField] private CanvasGroup climbStaminaUI;
    public CanvasGroup ClimbStaminaUI => climbStaminaUI;
    [SerializeField] private Image climbStaminaBar;
    public Image ClimbStaminaBar => climbStaminaBar;
    [Tooltip("Maximum stamina before falling")]
    [SerializeField] private float maxClimbStamina = 5f;
    public float MaxClimbStamina => maxClimbStamina;
    [Tooltip("Stamina drain rate per second while climbing")]
    [SerializeField] private float climbStaminaDrainRate = 1f;
    public float ClimbStaminaDrainRate => climbStaminaDrainRate;
    [Tooltip("Stamina regen rate per second while hanging")]
    [SerializeField] private float climbStaminaRegenRate = 2f;
    public float ClimbStaminaRegenRate => climbStaminaRegenRate;
    [SerializeField] private float climbStaminaFadeDuration = 0.5f;
    public float ClimbStaminaFadeDuration => climbStaminaFadeDuration;
    [SerializeField] private Gradient climbStaminaGradient;
    public Gradient ClimbStaminaGradient => climbStaminaGradient;
    [SerializeField] private float climbStartMinStamina = 0.2f;
    public float ClimbStartMinStamina => climbStartMinStamina;

    [Header("Debug")] 
    [SerializeField] private bool showClimbingStateDebugLog;
    public bool ShowClimbingStateDebugLog => showClimbingStateDebugLog;
}

public class ClimbingState : MovementState
{
    private static readonly int AnimIsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int AnimClimbSpeed = Animator.StringToHash("ClimbSpeed");
    private static readonly int AnimIsHanging = Animator.StringToHash("IsHanging");

    private ClimbingSettings Settings => stateMachine.ClimbingSettings;
    
    public ClimbingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool UseGravity => false;
    public override bool UseRootMotion => true;
    public override bool UseMouseRotatePlayer => false;
    public override bool ControlRotation => true;
    public override bool UseCollision => false;

    private Tween unhangDelayTween;
    private Tween rehangDelayTween;
    private Tween staminaFadeTween;
    private Tween sprintLeapCooldownTween;
    
    
    private float climbTimer;
    private bool isVaulting;
    private bool isHanging;
    private float currentStamina;
    
    // If already in the climbing state, the direction is the opposite wall normal. If not, then its the players direction. This fixes any problems with gradual rotation changes
    //private Vector3 CurrentForwardDirection => stateMachine.CurrentState == this ? -currentWallNormal : stateMachine.Forward;
    //private Vector3 CurrentUpDirection => Vector3.Cross(CurrentForwardDirection, stateMachine.Right);
    private Vector3 CurrentForwardDirection => stateMachine.Forward;
    private Vector3 CurrentUpDirection => stateMachine.Up;
        
    private Vector3 currentWallNormal;
    private Vector3 currentWallPos;

    private Vector3 startWallNormal;
    private Vector3 startPlayerPosition;
    
    private Vector3 currentClimbDirection;
    private Vector3 lastClimbDirection;
    
    private float CurrentClimbRange => Settings.ClimbRange;
    private bool IsStartingClimb => climbTimer < Settings.ClimbingStartLockIntoPlace;
    
    private float GetClimbingDistance(float angle)
    {
        float normalizedAngle = angle - 90;
        float t = Mathf.InverseLerp(Settings.ClimbDistanceFromWallRange.x, Settings.ClimbDistanceFromWallRange.y, normalizedAngle);

        Debug.Log(Mathf.Lerp(Settings.ClimbDistanceFromWall.x, Settings.ClimbDistanceFromWall.y, t));
        return Mathf.Lerp(Settings.ClimbDistanceFromWall.x, Settings.ClimbDistanceFromWall.y, t);
    }


    private void TryDebugLog(string value)
    {
        if (Settings.ShowClimbingStateDebugLog)
        {
            //if (stateMachine.CurrentState != this)
            //    return;
            
            Debug.Log(value);
        }
    }


    public override void Initialize()
    {
        base.Initialize();
        
        Settings.ClimbStaminaUI.alpha = 0.0f;
        
        currentStamina = Settings.MaxClimbStamina;

    }

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Climbing);
        
        stateMachine.PlayerAnimator.SetBool(AnimIsClimbing, true);
        
        currentStamina = Settings.MaxClimbStamina;
        
        ToggleStaminaBar(true);
        
        // Check if they should start hanging
        Vector3 topPosition = Vector3.zero;
        if(GetDistanceToTop(ref topPosition) < Settings.AutoTriggerHangDistance)
        {
            StartHanging();
            
            // Shift start position to same Y level as top
            // Move down slightly to account for head height
            Vector3 startPos = stateMachine.Position;
            startPos.y = topPosition.y - (stateMachine.PlayerHeight - (stateMachine.PlayerHeadHeight+ 0.01f));
            stateMachine.SetPosition(startPos);
        }

        climbTimer = 0.0f;
        isVaulting = false;
    }

    public override void OnExit()
    {
        if (!isVaulting)
            stateMachine.SetVelocity(Vector3.zero);
        
        // Climb speed needs to be set to 1 to finish climb animation properly
        stateMachine.PlayerAnimator.SetFloat(AnimClimbSpeed, 1);
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
        
        ToggleStaminaBar(false);
        
        StopClimbing();
    }

    private void StopClimbing()
    {
        stateMachine.PlayerAnimator.SetBool(AnimIsClimbing, false);
        stateMachine.PlayerAnimator.applyRootMotion = false;
    }
    

    public override void Tick()
    {
        if(isVaulting)
            return;

        ClimbDirections climbState = GetClimbState();
        
        // If they can climb or hang
        if (CanClimb(climbState))
        {
            // Get input
            Vector3 input = stateMachine.InputController.FrameMove;
            
            float upInput = input.y;
            // Only move up if they can
            if (upInput > 0f && !climbState.HasFlag(ClimbDirections.Up))
                upInput = 0f;
            
            float sideInput = input.x;
            // Only move sideways if theres space
            if (sideInput < 0f && !climbState.HasFlag(ClimbDirections.Left))
                sideInput = 0f;
            if (sideInput > 0f && !climbState.HasFlag(ClimbDirections.Right))
                sideInput = 0f;
            
            // If moving down while hanging, stop hanging
            if (isHanging && upInput < 0f && !unhangDelayTween.isAlive)
            {
                StopHanging();
            }
            
            // If moving sideways but not up, disable root motion
            if (Mathf.Abs(sideInput) > 0f && Mathf.Abs(upInput) <= 0f)
            {
                stateMachine.PlayerAnimator.applyRootMotion = false;
            }
            if(!isHanging && Mathf.Abs(upInput) > 0f)
            {
                stateMachine.PlayerAnimator.applyRootMotion = true;
            }
            
            float currentClimbSpeed = Mathf.Abs(upInput) >= Mathf.Abs(sideInput) ? upInput : sideInput;
            
            // Drain stamina if moving in either direction
            if (Mathf.Abs(upInput) > 0f || Mathf.Abs(sideInput) > 0f)
            {
                currentStamina -= Settings.ClimbStaminaDrainRate * Mathf.Abs(currentClimbSpeed) * Time.deltaTime;
                currentStamina = Mathf.Max(0f, currentStamina);
            }
            
            stateMachine.PlayerAnimator.SetFloat(AnimClimbSpeed, currentClimbSpeed);
            
            // If moving upwards, sprinting, not on cooldown, you have enough stamina
            if (upInput > 0 && 
                stateMachine.InputController.IsSprinting && 
                !sprintLeapCooldownTween.isAlive &&
                GetStaminaPercentage() >= Settings.ClimbSprintLeapStaminaPercentage + Settings.ClimbSprintLeapStaminaPadding &&
                !IsStartingClimb)
                SprintLeap();
            
            Vector3 upVelocity = CurrentUpDirection * (Settings.ClimbVerticalSpeed * upInput);
            Vector3 rightVelocity = stateMachine.Right * (Settings.ClimbHorizontalSpeed * sideInput);
            Vector3 finalVelocity = upVelocity + rightVelocity;
            
            stateMachine.SetVelocity(finalVelocity);

            currentClimbDirection = new Vector3(sideInput, upInput, 0);
            
            // Lock to wall
            Vector3 lockPos = currentWallPos + currentWallNormal * GetClimbingDistance(Vector3.Angle(currentWallNormal, Vector3.up));
            Debug.Log(Vector3.Angle(currentWallNormal, Vector3.up));
            // Since the start position is based on the players center, shift target position down to feet
            lockPos -= stateMachine.Up * (stateMachine.PlayerHeight/2);
            stateMachine.SetPosition(Vector3.Lerp(stateMachine.Position, lockPos, Settings.ClimbingWallLockSpeed * Time.deltaTime));
        
            // Face wall
            Quaternion targetRotation = Quaternion.LookRotation(-currentWallNormal);
            stateMachine.SetRotation(Quaternion.Slerp(stateMachine.Rotation, targetRotation, Settings.ClimbingWallLockSpeed * Time.deltaTime));
            
        }
        
        // If they cant climb up
        if (CanHang(climbState) && !rehangDelayTween.isAlive)
        {
            // If not currently hanging, start hanging
            if(!isHanging)
                StartHanging();
            
            // Regain stamina while hanging
            currentStamina = Mathf.Clamp(currentStamina + Settings.ClimbStaminaRegenRate * Time.deltaTime, 0, Settings.MaxClimbStamina);
            
            // Can jump to vault over ledge
            if (stateMachine.InputController.JumpDown && !unhangDelayTween.isAlive)
            {
                VaultOverLedge();
            }

            if (stateMachine.InputController.IsSprinting && !unhangDelayTween.isAlive)
            {
                VaultOverLedge();
            }
        }
        else if(isHanging && !unhangDelayTween.isAlive)
        {
            StopHanging();
        }
        

        
        UpdateStaminaUI();
        
        climbTimer += Time.deltaTime;

    }

    public override void FixedTick()
    {
    }

    public override void LateTick()
    {
        lastClimbDirection = currentClimbDirection;
        currentClimbDirection = Vector3.zero;
    }

    private void UpdateStaminaUI()
    {
        float t = currentStamina / Settings.MaxClimbStamina;
        Settings.ClimbStaminaBar.fillAmount = t;
        
        Color staminaColor = Settings.ClimbStaminaGradient.Evaluate(1-t);
        Settings.ClimbStaminaBar.color = staminaColor;
    }
    
    private float GetStaminaPercentage()
    {
        return currentStamina / Settings.MaxClimbStamina;
    }

    private void ToggleStaminaBar(bool value)
    {
        float alpha = value ? 1f : 0f;
        
        if(staminaFadeTween.isAlive)
            staminaFadeTween.Stop();
        
        staminaFadeTween = Tween.Alpha(Settings.ClimbStaminaUI, alpha, Settings.ClimbStaminaFadeDuration);
    }
    
    private void StartHanging()
    {
        isHanging = true;
        
        stateMachine.PlayerAnimator.SetBool(AnimIsHanging, true);
        stateMachine.PlayerAnimator.applyRootMotion = false;
        
        // If in the sprint leap animation, crossfade to hanging
        if (stateMachine.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClimbingSprint"))
        {
            stateMachine.PlayerAnimator.CrossFadeInFixedTime("ClimbingHang", 0.2f);
        }
        
        unhangDelayTween = Tween.Delay(Settings.UnhangDelay);
    }

    private void StopHanging()
    {
        isHanging = false;
        
        stateMachine.PlayerAnimator.SetBool(AnimIsHanging, false);
        stateMachine.PlayerAnimator.applyRootMotion = true;
 
        rehangDelayTween = Tween.Delay(Settings.RehangDelay);
    }

    private void SprintLeap()
    {
        sprintLeapCooldownTween = Tween.Delay(Settings.ClimbSprintLeapCooldown);
        
        stateMachine.PlayerAnimator.CrossFadeInFixedTime("ClimbingSprint", 0.1f);
        
        currentStamina -= Settings.MaxClimbStamina * Settings.ClimbSprintLeapStaminaPercentage;
    }

    private void VaultOverLedge()
    {
        SetDelay(Settings.ClimbingDelayAfterJump);
        
        ToggleStaminaBar(false);
                
        Vector3 vaultPosition = GetVaultPosition();
        // Raycast down to find ground
        Ray downRay = new Ray(vaultPosition, Vector3.down);
        if (Physics.SphereCast(downRay, Settings.ClimbVaultDistance, out var hitInfo, stateMachine.PlayerHeight + Settings.ClimbMaxVaultHeight, Settings.ClimbableLayer))
        {
            vaultPosition = hitInfo.point + stateMachine.Up * 0.1f; // Slightly above ground
        }
        
        stateMachine.PlayerAnimator.CrossFadeInFixedTime("ClimbingFinish", 0.2f);
        //StopClimbing();

        stateMachine.StartCoroutine(VaultingOverLedge(vaultPosition));
                
        isVaulting = true;

        // Reset camera
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
    }

    private Vector3 GetVaultPosition()
    {
        // Calculate ending position
        Vector3 verticalOffset = CurrentUpDirection * stateMachine.PlayerHeight + (Vector3.up * Settings.ClimbMaxVaultHeight);
        Vector3 forwardOffset = CurrentForwardDirection * Settings.ClimbVaultDistance;
        Vector3 targetPosition = stateMachine.Position + verticalOffset + forwardOffset;
        // Raycast down to find ground
        return targetPosition;
    }

    private IEnumerator VaultingOverLedge(Vector3 targetPosition)
    {

        float vaultTimer;
        float vaultDuration = Settings.ClimbVaultDuration;
        Vector3 startPosition = stateMachine.Position;
        Vector3 startRotation = stateMachine.RotationEuler;
        for (vaultTimer = 0f; vaultTimer < vaultDuration; vaultTimer += Time.deltaTime)
        {
            float t = vaultTimer / vaultDuration;
            
            // Use vertical and horizontal easing curves
            float verticalT = Settings.ClimbVaultEasingVertical.Evaluate(t);
            float horizontalT = Settings.ClimbVaultEasingHorizontal.Evaluate(t);
            // Lerp separately
            Vector3 verticalPosition = Vector3.Lerp(startPosition, new Vector3(startPosition.x, targetPosition.y, startPosition.z), verticalT);
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, startPosition.y, targetPosition.z), horizontalT);
            Vector3 newPosition = new Vector3(horizontalPosition.x, verticalPosition.y, horizontalPosition.z);
            stateMachine.SetPosition(newPosition);
            
            // Lerp X rotation back to 0
            Quaternion targetRotation = Quaternion.Euler(0f, startRotation.y, startRotation.z);
            stateMachine.SetRotation(Quaternion.Slerp(stateMachine.Rotation, targetRotation, t));
            
            yield return null;
        }
        
        // Switch to walking state
        SwitchState(stateMachine.WalkingState);
    }

    public override void CheckTransitions()
    {
        
        // Ignore while vaulting
        if(isVaulting)
            return;
        
        var climbState = GetClimbState();
        // If they crouch or can no longer climb
        if (
            (stateMachine.InputController.CrouchDown) || 
             (CantClimb(climbState) && !isVaulting && !IsStartingClimb)
            )
        {
            // Trigger retry delay
            SetDelay(Settings.ClimbingRetryDelay);
            
            SwitchState(stateMachine.FallingState);
            return;
        }

        // Fall when out of stamina
        if (currentStamina <= 0f)
        {
            SetDelay(Settings.ClimbingRetryDelay);
            
            SwitchState(stateMachine.FallingState);
            return;
        }
        
        // Return to walk are pressing down and are now grounded
        if (stateMachine.InputController.IsMovingDown && stateMachine.IsGrounded)
        {
            SwitchState(stateMachine.WalkingState);
            return;
        }
    }

    private float GetDistanceToTop(ref Vector3 topPosition)
    {
        float maxDistance = 100;
        
        Vector3 headOrigin = stateMachine.Position + CurrentUpDirection * stateMachine.PlayerHeadHeight;
        Vector3 wallPosition = headOrigin + (CurrentForwardDirection * (CurrentClimbRange + 0.1f));

        Ray distanceRay = new Ray(wallPosition + Vector3.up * maxDistance, -CurrentUpDirection);
        if (Physics.Raycast(distanceRay, out var hitInfo, maxDistance, Settings.ClimbableLayer))
        {
            topPosition = hitInfo.point;
            return Vector3.Distance(headOrigin, hitInfo.point);
        }

        return maxDistance;
    }
    private float DistanceToFloor(float offset = 0)
    {
        float maxDistance = 100;
        Vector3 startPosition = stateMachine.Position + Vector3.up * offset;
        Ray distanceRay = new Ray(startPosition, -Vector3.up);
        if (Physics.Raycast(distanceRay, out var hitInfo, maxDistance, stateMachine.EnvironmentLayer))
        {
            return Vector3.Distance(startPosition, hitInfo.point);
        }

        return maxDistance;
    }

    // For some unknown reason this is unnecessarily complicated to check
    // In the case for this climbing, I'll do a linecast from the point to the player
    // If it passes through an odd number of surfaces, then that means it started inside an object
    private static readonly RaycastHit[] raycastBuffer = new RaycastHit[16];
    private bool IsPointInsideMesh(Vector3 point)
    {
        Vector3 origin = stateMachine.Position;
        Vector3 direction = point - origin;
        float distance = direction.magnitude;
        direction.Normalize();

        // If theyre on the same position
        if (distance <= Mathf.Epsilon)
            return false;

        int hitCount = Physics.RaycastNonAlloc(
            origin,
            direction,
            raycastBuffer,
            distance,
            stateMachine.EnvironmentLayer,
            QueryTriggerInteraction.Ignore
        );

        return (hitCount % 2) == 1;
    }
    
    
    
    public ClimbDirections GetClimbState()
    {
            
            bool CheckNormalAngles(Vector3 normal)
            {
                // Calculate vertical angle between normal and world up
                float verticalAngle = Vector3.Angle(-normal, Vector3.up) - 90f;
                // Calculate horizontal angle between normal and player forward
                float horizontalAngle = Vector3.SignedAngle(-normal, CurrentForwardDirection, CurrentUpDirection);
                // Check if angles are within limits
                if (verticalAngle < Settings.ClimbingVerticalAngleLimits.x || verticalAngle > Settings.ClimbingVerticalAngleLimits.y)
                    return false;
                if (horizontalAngle < Settings.ClimbingHorizontalAngleLimits.x || horizontalAngle > Settings.ClimbingHorizontalAngleLimits.y)
                    return false;
                
                return true;
            }
        
        Vector3 playerPosition = stateMachine.Position;
        Vector3 direction = CurrentForwardDirection;
        Vector3 playerUp = CurrentUpDirection;
        Vector3 bottomOrigin = playerPosition;
        Vector3 topOrigin = playerPosition + playerUp * stateMachine.PlayerHeight;
        Vector3 middleOrigin = playerPosition + playerUp * (stateMachine.PlayerHeight/2);
        Vector3 sideDirection = stateMachine.Right;
        Vector3 widthOffset = sideDirection * Settings.ClimbWidth;
        
        // Create rays for each direction
        Ray middleRay = new Ray(middleOrigin, direction);
        Ray leftRay = new Ray(middleOrigin - widthOffset, direction);
        Ray rightRay = new Ray(middleOrigin + widthOffset, direction);
        Ray downRay = new Ray(bottomOrigin, direction);
        Ray upRay = new Ray(topOrigin, direction);
        Ray ceilingRay = new Ray(topOrigin, playerUp);
        
        // Set wallnormal to be player backwards by default
        Vector3 wallNormal = -direction;
        // Set wallpos to be middle pos by default
        Vector3 wallPos = middleOrigin;
        
        
        ClimbDirections climbDirections = ClimbDirections.None;
        
        RaycastHit headHitInfo;
        // Check if middle can find wall
        bool foundWall = Physics.Raycast(middleRay, out headHitInfo, CurrentClimbRange, Settings.ClimbableLayer);
        

        if (foundWall)
        { 
            // If hit position is too far, ignore
            if (Vector3.Distance(headHitInfo.point, middleOrigin) > CurrentClimbRange)
            {
                // Dont end check if the down ray hit an obj instead
                if(!Physics.Raycast(downRay, out var bottomHit, CurrentClimbRange, Settings.ClimbableLayer))
                {
                    TryDebugLog("Head and down ray did not hit close surface");
                    return climbDirections;
                }
                
                // Make sure head and down ray hit the same object
                if (bottomHit.collider != headHitInfo.collider)
                {
                    TryDebugLog("Feet hit surface but it was the same collider as head");
                    return climbDirections;
                }
                
            }

            
            Vector3 headNormal = headHitInfo.normal;

            

            
            // Assume can climb in all directions initially
            climbDirections = ClimbDirections.Up | ClimbDirections.Down | ClimbDirections.Left | ClimbDirections.Right;
            
            // If some ray don't hit, remove from result
            
            RaycastHit upHit, downHit, leftHit, rightHit;

            bool canUp = Physics.Raycast(upRay, out upHit, CurrentClimbRange, Settings.ClimbableLayer) && CheckNormalAngles(upHit.normal);
            bool canDown = Physics.Raycast(downRay, out downHit, CurrentClimbRange, Settings.ClimbableLayer) && CheckNormalAngles(downHit.normal);
            bool canLeft = Physics.Raycast(leftRay, out leftHit, CurrentClimbRange, Settings.ClimbableLayer) && CheckNormalAngles(leftHit.normal);
            bool canRight = Physics.Raycast(rightRay, out rightHit, CurrentClimbRange, Settings.ClimbableLayer) && CheckNormalAngles(rightHit.normal);
            
            Debug.DrawRay(upRay.origin, upRay.direction * CurrentClimbRange, canUp ? Color.green : Color.red);
            Debug.DrawRay(downRay.origin, downRay.direction * CurrentClimbRange, canDown ? Color.green : Color.red);
            Debug.DrawRay(leftRay.origin, leftRay.direction * CurrentClimbRange, canLeft ? Color.green : Color.red);
            Debug.DrawRay(rightRay.origin, rightRay.direction * CurrentClimbRange, canRight ? Color.green : Color.red);

            if (!canUp)
                climbDirections &= ~ClimbDirections.Up;
            else if (Physics.Raycast(ceilingRay, 0.1f, Settings.ClimbableLayer))
                climbDirections &= ~ClimbDirections.Up;

            if (!canDown)
                climbDirections &= ~ClimbDirections.Down;

            if (!canLeft)
                climbDirections &= ~ClimbDirections.Left;

            if (!canRight)
                climbDirections &= ~ClimbDirections.Right;

            Vector3 surfaceNormal = Vector3.zero;

            // Find average surface normal
            int normalCount = 0;
            if (canUp)
            {
                surfaceNormal += upHit.normal;
                normalCount++;
            }
            if (canDown)
            {
                surfaceNormal += downHit.normal;
                normalCount++;
            }
            if (canLeft)
            {
                surfaceNormal += leftHit.normal;
                normalCount++;
            }
            if (canRight)
            {
                surfaceNormal += rightHit.normal;
                normalCount++;
            }

            if (normalCount > 0)
                surfaceNormal /= normalCount;
            else
                surfaceNormal = headNormal;

            wallNormal = surfaceNormal;
            wallPos = headHitInfo.point;
            
            // Set the start data if they aren't currently climbing
            if (stateMachine.CurrentState != this)
            {
                Vector3 startPosition = headHitInfo.point;
                Vector3 startNormal = headHitInfo.normal;
                
                void HorizontalOffsetStartPosition(bool isLeft)
                {
                    float directionSign = isLeft ? -1f : 1f;
                    Vector3 horizontalDirection = Vector3.Cross(startNormal, playerUp).normalized * directionSign;
                    Vector3 horizontalOffset = horizontalDirection * Settings.ClimbWidth;
                
                    // Draw line from side ray into wall
                    Vector3 rayOrigin = middleOrigin + horizontalOffset;
                    // Offset forward by ray distance
                    rayOrigin += -headHitInfo.normal * (headHitInfo.distance+0.1f);
                    Ray horizontalRay = new Ray(rayOrigin, -horizontalDirection);
                    if (Physics.Raycast(horizontalRay, out var horizontalHit, Settings.ClimbWidth, Settings.ClimbableLayer))
                    {
                        // Shift start position by the distance hit
                        startPosition -= horizontalDirection * horizontalHit.distance;
                    }
                }
                
                // If cant climb left or right, do sphere cast to find the edge of wall
                if (!climbDirections.HasFlag(ClimbDirections.Left))
                {
                    HorizontalOffsetStartPosition(true);
                }
                else if (!climbDirections.HasFlag(ClimbDirections.Right))
                {
                    HorizontalOffsetStartPosition(false);
                }
            }

            currentWallNormal = wallNormal;
            currentWallPos = wallPos;

        }
        return climbDirections;
    }
    
    private bool CantClimb(ClimbDirections climbDirections) => climbDirections == ClimbDirections.None;
    public bool CantClimb() => CantClimb(GetClimbState());
    private bool CanClimb(ClimbDirections climbDirections) => climbDirections.HasFlag(ClimbDirections.Up) || 
                                                             climbDirections.HasFlag(ClimbDirections.Down) || 
                                                             climbDirections.HasFlag(ClimbDirections.Left) || 
                                                             climbDirections.HasFlag(ClimbDirections.Right);
    
    public bool CanClimb() => CanClimb(GetClimbState());
    private bool CanInitiateClimb(ClimbDirections climbDirections) => climbDirections.HasFlag(ClimbDirections.Up) && 
                                                                      (climbDirections.HasFlag(ClimbDirections.Left ) || climbDirections.HasFlag(ClimbDirections.Right));
    public bool CanInitiateClimb() => CanInitiateClimb(GetClimbState());

    private bool CanHang(ClimbDirections climbDirections) => CanClimb(climbDirections) && !climbDirections.HasFlag(ClimbDirections.Up) && CanVault();
    public bool CanHang() => CanHang(GetClimbState());

    private bool CanVault()
    {
        Vector3 vaultPosition = GetVaultPosition();
        // Make sure point isnt inside mesh and player collider can fit
        if (IsPointInsideMesh(vaultPosition) || Physics.CheckSphere(vaultPosition, stateMachine.PlayerRadius, stateMachine.EnvironmentLayer, QueryTriggerInteraction.Ignore))
            return false;

        return true;

    }
    [Flags]
    public enum ClimbDirections
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
}

using System;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

[Serializable]
public class ClimbingSettings : StateSettings
{
    [Header("Climbing")]
    [SerializeField] private LayerMask climbableLayer;
    public LayerMask ClimbableLayer => climbableLayer;
    [SerializeField] private float climbRange = 1f;
    public float ClimbRange => climbRange;
    [FormerlySerializedAs("climbingAngleLimits")] [SerializeField] private Vector2 climbingVerticalAngleLimits = new Vector2(-40f, 40f);
    public Vector2 ClimbingVerticalAngleLimits => climbingVerticalAngleLimits;
    [SerializeField] private Vector2 climbingHorizontalAngleLimits = new Vector2(-40f, 40f);
    public Vector2 ClimbingHorizontalAngleLimits => climbingHorizontalAngleLimits;
    [SerializeField] private float climbVerticalSpeed = 1f;
    public float ClimbVerticalSpeed => climbVerticalSpeed;
    [SerializeField] private float climbHorizontalSpeed = 1f;
    public float ClimbHorizontalSpeed => climbHorizontalSpeed;
    [SerializeField] private InputAxis.RecenteringSettings climbCameraRecentering;
    public InputAxis.RecenteringSettings ClimbCameraRecentering => climbCameraRecentering;
    [Tooltip("Initially to stop player from reclimbing immediately after jumping")]
    [SerializeField] private float climbingDelayAfterJump = 0.5f;
    public float ClimbingDelayAfterJump => climbingDelayAfterJump;
    [SerializeField] private float climbingRetryDelay = 0.5f;
    public float ClimbingRetryDelay => climbingRetryDelay;
    [Tooltip("Time taken for player to lock onto the wall when they start climbing")]
    [SerializeField] private float climbingStartLockIntoPlace = 0.25f;
    public float ClimbingStartLockIntoPlace => climbingStartLockIntoPlace;
    [SerializeField] private float climbDistanceFromWall = 0.33f;
    public float ClimbDistanceFromWall => climbDistanceFromWall;
    [SerializeField] private float climbVaultDistance = 0.5f;
    public float ClimbVaultDistance => climbVaultDistance;
    [SerializeField] private float climbVaultDuration = 1f;
    public float ClimbVaultDuration => climbVaultDuration;
    [Tooltip("Control vertical speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingVertical;
    public AnimationCurve ClimbVaultEasingVertical => climbVaultEasingVertical;
    [Tooltip("Control horizontal speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingHorizontal;
    public AnimationCurve ClimbVaultEasingHorizontal => climbVaultEasingHorizontal;
    [Tooltip("Brief delay before being able to stop hanging")]
    [SerializeField] private float unhangDelay = 0.4f;
    public float UnhangDelay => unhangDelay;
    [Tooltip("Delay before player can rehang after falling")]
    [SerializeField] private float rehangDelay = 0.2f;
    public float RehangDelay => rehangDelay;
    [Tooltip("Distance from the top of the wall at which entering climb will automatically hang")]
    [SerializeField] private float autoTriggerHangDistance = 1f;
    public float AutoTriggerHangDistance => autoTriggerHangDistance;
    [Tooltip("Additional padding when checking for side climbing space")]
    [SerializeField] private float sideWidthOffset = 0.5f;
    public float SideWidthOffset => sideWidthOffset;
    
    [Header("Sprint Leap")]
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
}

public class ClimbingState : MovementState
{
    private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");
    private static readonly int IsHanging = Animator.StringToHash("IsHanging");

    private ClimbingSettings Settings => stateMachine.ClimbingSettings;
    
    public ClimbingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool UseGravity => false;
    public override bool UseRootMotion => true;
    public override bool UseMouseRotatePlayer => false;
    public override bool ControlRotation => true;

    private Tween unhangDelayTween;
    private Tween rehangDelayTween;
    private Tween staminaFadeTween;
    private Tween sprintLeapCooldownTween;
    private float climbTimer;
    private bool isVaulting;
    private bool isHanging;
    private float currentStamina;
    private bool isStartingClimb => climbTimer < Settings.ClimbingStartLockIntoPlace;
    private float ClimbingWidth => stateMachine.PlayerRadius + Settings.SideWidthOffset;

    public override void Initialize()
    {
        base.Initialize();
        
        Settings.ClimbStaminaUI.alpha = 0.0f;
        
        currentStamina = Settings.MaxClimbStamina;

    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        // If no start data, climb condition hasnt properly been checked
        if (!climbStartData.IsValid)
        {
            Debug.LogWarning("ClimbState entered without valid climb start data");
            SwitchState(stateMachine.FallingState);
            return;
        }

        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Climbing);
        
        stateMachine.PlayerAnimator.SetBool(IsClimbing, true);
        
        currentStamina = Settings.MaxClimbStamina;
        
        ToggleStaminaBar(true);
        
        // Check if they should start hanging
        Vector3 topPosition = Vector3.zero;
        if(GetDistanceToTop(ref topPosition) < Settings.AutoTriggerHangDistance)
        {
            StartHanging();
            
            // Shift start position to same Y level as top
            // Move down slightly to account for head height
            Vector3 startPos = climbStartData.StartPosition;
            startPos.y = topPosition.y - (stateMachine.PlayerHeight - (stateMachine.PlayerHeadHeight+ 0.11f));
            climbStartData.StartPosition = startPos;
        }

        climbTimer = 0.0f;
        isVaulting = false;
    }

    public override void OnExit()
    {
        if (!isVaulting)
            stateMachine.SetVelocity(Vector3.zero);
        
        // Climb speed needs to be set to 1 to finish climb animation properly
        stateMachine.PlayerAnimator.SetFloat(ClimbSpeed, 1);
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
        
        ToggleStaminaBar(false);
        
        StopClimbing();
    }

    private void StopClimbing()
    {
        stateMachine.PlayerAnimator.SetBool(IsClimbing, false);
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
            
            float currentClimbSpeed = Mathf.Abs(upInput) > Mathf.Abs(sideInput) ? upInput : sideInput;
            
            // Drain stamina if moving in either direction
            if (Mathf.Abs(upInput) > 0f || Mathf.Abs(sideInput) > 0f)
            {
                currentStamina -= Settings.ClimbStaminaDrainRate * Mathf.Abs(currentClimbSpeed) * Time.deltaTime;
                currentStamina = Mathf.Max(0f, currentStamina);
            }
            
            stateMachine.PlayerAnimator.SetFloat(ClimbSpeed, currentClimbSpeed);
            
            // If moving upwards, sprinting, not on cooldown, not starting the climb, you have enough stamina
            if (upInput > 0 && 
                stateMachine.InputController.IsSprinting && 
                !sprintLeapCooldownTween.isAlive &&
                GetStaminaPercentage() >= Settings.ClimbSprintLeapStaminaPercentage + Settings.ClimbSprintLeapStaminaPadding &&
                !isStartingClimb)
                SprintLeap();
            
            Vector3 upVelocity = climbStartData.UpDirection * (Settings.ClimbVerticalSpeed * upInput);
            Vector3 rightVelocity = stateMachine.Right * (Settings.ClimbHorizontalSpeed * sideInput);
            Vector3 finalVelocity = upVelocity + rightVelocity;
            
            stateMachine.SetVelocity(finalVelocity);
        }
        
        // If they cant climb up
        if (CanHang(climbState) && !rehangDelayTween.isAlive)
        {
            // If not currently hanging, start hanging
            if(!isHanging)
                StartHanging();
            
            // Regain stamina while hanging
            currentStamina += Settings.ClimbStaminaRegenRate * Time.deltaTime;
            
            // Can jump to vault over ledge
            if (stateMachine.InputController.JumpDown && !unhangDelayTween.isAlive)
            {
                VaultOverLedge();
            }
        }
        else if(isHanging && !unhangDelayTween.isAlive)
        {
            StopHanging();
        }
        
        // Lock player to wall at start
        if (climbTimer < Settings.ClimbingStartLockIntoPlace)
        {
            float lockT = climbTimer / Settings.ClimbingStartLockIntoPlace;
            Vector3 direction = -climbStartData.StartNormal;

            // Lerp in direction
            Vector3 targetPosition = climbStartData.StartPosition - direction * Settings.ClimbDistanceFromWall;
            // Since the start position is based on the players head, shift target position down to feet
            targetPosition -= climbStartData.UpDirection * stateMachine.PlayerHeadHeight;
            
            stateMachine.SetPosition(Vector3.Lerp(climbStartData.PlayerPosition, targetPosition, lockT));
            // Face wall
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateMachine.SetRotation(Quaternion.Slerp(stateMachine.Rotation, targetRotation, lockT));
        }
        
        UpdateStaminaUI();
        
        climbTimer += Time.deltaTime;

    }

    public override void FixedTick()
    {
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
        
        stateMachine.PlayerAnimator.SetBool(IsHanging, true);
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
        
        stateMachine.PlayerAnimator.SetBool(IsHanging, false);
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
                
        //stateMachine.AddVelocity(Vector3.up * stateMachine.JumpForce);
        
        // Calculate ending position
        Vector3 verticalOffset = climbStartData.UpDirection * (stateMachine.PlayerHeight + 0.5f);
        Vector3 forwardOffset = stateMachine.Forward * Settings.ClimbVaultDistance;
        Vector3 targetPosition = stateMachine.Position + verticalOffset + forwardOffset;
        // Raycast down to find ground
        Vector3 vaultPosition = targetPosition;
        Ray downRay = new Ray(targetPosition, Vector3.down);
        if (Physics.SphereCast(downRay, Settings.ClimbVaultDistance, out var hitInfo, stateMachine.PlayerHeight + 1.5f, Settings.ClimbableLayer))
        {
            vaultPosition = hitInfo.point + climbStartData.UpDirection * 0.1f; // Slightly above ground
        }
        
        stateMachine.PlayerAnimator.CrossFadeInFixedTime("ClimbingFinish", 0.2f);
        //StopClimbing();

        stateMachine.StartCoroutine(VaultingOverLedge(vaultPosition));
                
        isVaulting = true;

        // Reset camera
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
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
             (CantClimb(climbState) && !isVaulting && climbTimer > Settings.ClimbingStartLockIntoPlace)
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
        
        Vector3 headOrigin = stateMachine.Position + stateMachine.Up * stateMachine.PlayerHeadHeight;
        Vector3 wallPosition = headOrigin + (stateMachine.Forward * (Settings.ClimbRange+0.1f));

        Ray distanceRay = new Ray(wallPosition + Vector3.up * maxDistance, -climbStartData.UpDirection);
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
    
    
    private ClimbStartData climbStartData;
    
    public ClimbDirections GetClimbState()
    {
        Vector3 playerPosition = stateMachine.Position;
        Vector3 playerUp = stateMachine.Up;
        Vector3 bottomOrigin = playerPosition + playerUp * 0.1f;
        Vector3 topOrigin = playerPosition + playerUp * (stateMachine.PlayerHeight - 0.1f);
        Vector3 headOrigin = playerPosition + playerUp * stateMachine.PlayerHeadHeight;
        Vector3 direction = stateMachine.Forward;
        Vector3 sideDirection = stateMachine.Right;
        Vector3 sideOffset = sideDirection * (ClimbingWidth/2);
        // Create rays for each direction
        Ray downRay = new Ray(bottomOrigin, direction);
        Ray upRay = new Ray(topOrigin, direction);
        Ray headRay = new Ray(headOrigin, direction);
        Ray leftRay = new Ray(headOrigin - sideOffset, direction);
        Ray rightRay = new Ray(headOrigin + sideOffset, direction);
        
        
        ClimbDirections climbDirections = ClimbDirections.None;
        
        if (Physics.Raycast(headRay, out var hitInfo, 100, Settings.ClimbableLayer))
        {
            // If hit position is too far, ignore
            // Do additional check at feet in case of walls slanted forward
            if (Vector3.Distance(hitInfo.point, headOrigin) > Settings.ClimbRange)
            {
                if(!Physics.Raycast(downRay, out var downHit, Settings.ClimbRange, Settings.ClimbableLayer))
                    return climbDirections;
                
                // Make sure head and down ray hit the same object
                if (downHit.collider != hitInfo.collider)
                    return climbDirections;
                
            }

            bool CheckNormalAngles(Vector3 normal)
            {
                // Calculate angle between normal and up direction
                float verticalAngle = Vector3.SignedAngle(-normal, Vector3.up, Vector3.up);
                verticalAngle -= 90;
                if (verticalAngle < Settings.ClimbingVerticalAngleLimits.x || verticalAngle > Settings.ClimbingVerticalAngleLimits.y)
                    return false;
                // Calculate angle between normal and forward direction
                float horizontalAngle = Vector3.SignedAngle(-normal, stateMachine.Forward, sideDirection);
                if (horizontalAngle < Settings.ClimbingHorizontalAngleLimits.x || horizontalAngle > Settings.ClimbingHorizontalAngleLimits.y)
                    return false;
                return true;
            }
            
            // If up direction is too steep, ignore
            Vector3 normal = hitInfo.normal;
            if (!CheckNormalAngles(normal))
                return climbDirections;
            

            
            // Assume can climb in all directions initially
            climbDirections = ClimbDirections.Up | ClimbDirections.Down | ClimbDirections.Left | ClimbDirections.Right;
            
            // If some ray don't hit, remove from result
            if (!Physics.Raycast(upRay, out var upInfo, Settings.ClimbRange, Settings.ClimbableLayer)
                || !CheckNormalAngles(upInfo.normal))
            {
                climbDirections &= ~ClimbDirections.Up;
            }
            if (!Physics.Raycast(leftRay, out var leftInfo, Settings.ClimbRange, Settings.ClimbableLayer)
                || !CheckNormalAngles(leftInfo.normal))
            {
                climbDirections &= ~ClimbDirections.Left;
            }
            if (!Physics.Raycast(rightRay, out var rightInfo, Settings.ClimbRange, Settings.ClimbableLayer)
                || !CheckNormalAngles(rightInfo.normal))
            {
                climbDirections &= ~ClimbDirections.Right;
            }
            if (!Physics.Raycast(downRay, out var downInfo, Settings.ClimbRange, Settings.ClimbableLayer)
                || !CheckNormalAngles(downInfo.normal))
            {
                climbDirections &= ~ClimbDirections.Down;
            }


            
            // Set the start data if they aren't currently climbing
            if (stateMachine.CurrentState != this)
            {
                Vector3 startPosition = hitInfo.point;
                Vector3 startNormal = hitInfo.normal;
                
                void HorizontalOffsetStartPosition(bool isLeft)
                {
                    float directionSign = isLeft ? -1f : 1f;
                    Vector3 horizontalDirection = Vector3.Cross(startNormal, playerUp).normalized * directionSign;
                    Vector3 horizontalOffset = horizontalDirection * (ClimbingWidth/2);
                
                    // Draw line from side ray into wall
                    Vector3 rayOrigin = headOrigin + horizontalOffset;
                    // Offset forward by ray distance
                    rayOrigin += -hitInfo.normal * (hitInfo.distance+0.1f);
                    Ray horizontalRay = new Ray(rayOrigin, -horizontalDirection);
                    Debug.DrawRay(horizontalRay.origin, horizontalRay.direction * ClimbingWidth, Color.blue, 1f);
                    if (Physics.Raycast(horizontalRay, out var horizontalHit, ClimbingWidth, Settings.ClimbableLayer))
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
                
                climbStartData = new ClimbStartData(startPosition, startNormal, stateMachine.Position);
            }

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

    private bool CanHang(ClimbDirections climbDirections) => CanClimb(climbDirections) && !climbDirections.HasFlag(ClimbDirections.Up);
    public bool CanHang() => CanHang(GetClimbState());
    [Flags]
    public enum ClimbDirections
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
    
    private struct ClimbStartData
    {
        public bool IsValid;
        public Vector3 StartPosition;
        public Vector3 StartNormal;
        public Vector3 PlayerPosition;
        public Vector3 UpDirection;
        
        
        public ClimbStartData(Vector3 startPosition, Vector3 startNormal, Vector3 playerPosition)
        {
            this.IsValid = true;
            StartPosition = startPosition;
            StartNormal = startNormal;
            PlayerPosition = playerPosition;
            Vector3 rightDirection = Vector3.Cross(Vector3.up, startNormal).normalized;
            UpDirection = Vector3.Cross(startNormal, rightDirection).normalized;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class ParachutingSettings : StateSettings
{
    [Header("Parachuting")]
    [SerializeField] private float parachutingMaxSpeed = 15f;
    public float ParachuteMaxSpeed => parachutingMaxSpeed;
    [SerializeField] private float parachutingAcceleration = 30f;
    public float ParachuteAcceleration => parachutingAcceleration;
    [SerializeField] private float parachutingTurnMaxSpeed = 5f;
    public float ParachuteTurnMaxSpeed => parachutingTurnMaxSpeed;
    [SerializeField] private float parachutingTurnAcceleration = 3f;
    public float ParachuteTurnAcceleration => parachutingTurnAcceleration;
    [SerializeField] private float parachutingTurnTiltAngle = 15f;
    public float ParachuteTurnTiltAngle => parachutingTurnTiltAngle;
    [SerializeField] private Easing.EaseType parachutingTiltEasing = Easing.EaseType.InOutSine;
    public Easing.EaseType ParachuteTiltEasing => parachutingTiltEasing;
    [SerializeField] private float parachutingTileDampening = 5f;
    public float ParachuteTileDampening => parachutingTileDampening;
    [SerializeField] private float parachutingDiveMaxAngle = 45f;
    public float ParachuteDiveMaxAngle => parachutingDiveMaxAngle;
    [SerializeField] private float parachutingDiveSpeed = 3;
    public float ParachuteDiveSpeed => parachutingDiveSpeed;
    [SerializeField] private float parachutingDiveSpeedBoost = 3;
    public float ParachuteDiveSpeedBoost => parachutingDiveSpeedBoost;
    [SerializeField] private Easing.EaseType parachutingDiveEasing = Easing.EaseType.InOutSine;
    public Easing.EaseType ParachuteDiveEasing => parachutingDiveEasing;
    [SerializeField] private float parachutingGravity = -3f;
    public float ParachuteGravity => parachutingGravity;
    [SerializeField] private float parachutingStartBoost;
    public float ParachuteStartBoost => parachutingStartBoost;
    [Tooltip("Minimum distance from ground to initiate parachute")]
    [SerializeField] private float minimumHeightToDeploy = 3f;
    public float MinimumHeightToDeploy => minimumHeightToDeploy;
    [Tooltip("Maximum downward velocity to check if theyre near the ground, if they're falling faster then this no need to check if they're near the ground")]
    [SerializeField] private float groundCheckVelocityThreshold = -2f;
    public float GroundCheckVelocityThreshold => groundCheckVelocityThreshold;
    [Tooltip("Minimum distance from wall to initiate parachute")]
    [SerializeField] private float minimumDeployDistanceFromWall = 3;
    public float MinimumDeployDistanceFromWall => minimumDeployDistanceFromWall;
    [Tooltip("Multiplier to increase the camera FOV when travelling at max speed")]
    [SerializeField] private float fovMultiplierAtMaxSpeed = 1.2f;
    public float FovMultiplierAtMaxSpeed => fovMultiplierAtMaxSpeed;
}

public class ParachuteState : MovementState
{
    private static readonly int IsParachuting = Animator.StringToHash("IsParachuting");
    
    public ParachuteState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public ParachutingSettings Settings => stateMachine.ParachutingSettings;
    public override bool UseMouseRotatePlayer => false;
    public override bool ControlRotation => true;

    private float currentTurnValue = 0.0f;
    private float smoothedTiltAngle = 0.0f;
    private float currentDiveValue = 0.0f;


    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        
        AddCanEnterCondition(CanParachute);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Parachute);
        
        stateMachine.PlayerAnimator.SetBool(IsParachuting, true);
        stateMachine.PlayerAnimator.CrossFade("Parachuting", 0.1f);
        
        currentDiveValue = 0.0f;

    }

    public override void CheckTransitions()
    { 
        // Cancel state
        if (stateMachine.InputController.CrouchDown)
            SwitchState(stateMachine.FallingState);

        // Landed
        if (stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);
        
        // If they hit a wall
        if (stateMachine.IsFacingWall() && stateMachine.ClimbingState.CanClimb())
        {
            SwitchState(stateMachine.ClimbingState);
        }
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsParachuting, false);
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
        
        // Reset visual rotation
        stateMachine.SetVisualRotation(Quaternion.identity);

    }

    public override void Tick()
    {
        Vector2 input = stateMachine.InputController.FrameMove;
        
        // Turn
        float targetTurnValue = input.x * Settings.ParachuteTurnMaxSpeed;
        currentTurnValue = Mathf.MoveTowards(currentTurnValue, targetTurnValue, Settings.ParachuteTurnAcceleration * Time.deltaTime);
        float finalTurnAngle = stateMachine.RotationEuler.y + currentTurnValue;

        // Dive
        if (Mathf.Abs(input.y) > 0.1f)
            currentDiveValue += input.y * Time.deltaTime;
        else
            // Reset dive
            currentDiveValue = Mathf.MoveTowards(currentDiveValue, 0, Time.deltaTime);
        currentDiveValue = Mathf.Clamp(currentDiveValue, 0, Settings.ParachuteDiveSpeed);
        
        float diveProgress = Mathf.Clamp01(currentDiveValue / Settings.ParachuteDiveSpeed);
        float easedDive = Easing.FindEaseType(Settings.ParachuteDiveEasing)(diveProgress);
        float finalDiveAngle = easedDive * Settings.ParachuteDiveMaxAngle;
        
        // Set rotation
        Quaternion finalRotation = Quaternion.Euler(finalDiveAngle, finalTurnAngle, 0);
        stateMachine.SetRotation(finalRotation);
        
        
        
        // Velocity
        // Speed boost while diving
        float diveBoost = 1f + (diveProgress * Settings.ParachuteDiveSpeedBoost);
        
        Vector3 startVelocity = stateMachine.CurrentVelocity;
        Vector3 targetVelocity = stateMachine.Forward * (Settings.ParachuteMaxSpeed * diveBoost);
        Vector3 travelVelocity = Vector3.MoveTowards(startVelocity, targetVelocity, Settings.ParachuteAcceleration);
        
        // Apply gentle gravity
        travelVelocity += Vector3.up * (Settings.ParachuteGravity);
        stateMachine.SetVelocity(travelVelocity);
        
        // Set fov based on velocity
        float speedProgress = Mathf.Clamp01(travelVelocity.magnitude / (Settings.ParachuteMaxSpeed * (1f + Settings.ParachuteDiveSpeedBoost)));
        stateMachine.PlayerCamera.CurrentFovMultiplier = Mathf.Lerp(1, Settings.FovMultiplierAtMaxSpeed, speedProgress);
        

        
        // Tilt based on turn
        float turnProgress = Mathf.Clamp01(Mathf.Abs(currentTurnValue) / Settings.ParachuteTurnMaxSpeed);
        float easedTurn = Easing.FindEaseType(Settings.ParachuteTiltEasing)(turnProgress);
        
        smoothedTiltAngle = Mathf.Lerp(
            smoothedTiltAngle,
            easedTurn * Settings.ParachuteTurnTiltAngle * Mathf.Sign(currentTurnValue),
            Settings.ParachuteTileDampening * Time.deltaTime
        );
        
        Quaternion tiltRotation = Quaternion.Euler(0, 0, -smoothedTiltAngle);
        stateMachine.SetVisualRotation(tiltRotation);

    }

    public override void FixedTick()
    {
        

        




    }

    private bool CanParachute()
    {
        // Check distance to ground if velocity is too low
        if(stateMachine.GetGroundDistance() < Settings.MinimumHeightToDeploy && stateMachine.CurrentVelocity.y > Settings.GroundCheckVelocityThreshold)
            return false;
        
        
        // Check if wall is in the wall
        if (stateMachine.IsFacingWall(Settings.MinimumDeployDistanceFromWall))
            return false;

        return true;
    }

}

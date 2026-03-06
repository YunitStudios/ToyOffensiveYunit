using System;
using PrimeTween;
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
    [SerializeField] private float parachutingDiveInSpeed = 0.1f;
    public float ParachuteDiveInSpeed => parachutingDiveInSpeed;
    [SerializeField] private float parachutingDiveOutSpeed = 0.15f;
    public float ParachuteDiveOutSpeed => parachutingDiveOutSpeed;
    [SerializeField] private float parachutingDiveSpeedBoost = 3;
    public float ParachuteDiveSpeedBoost => parachutingDiveSpeedBoost;
    [SerializeField] private Easing.EaseType parachutingDiveInEasing = Easing.EaseType.InSine;
    public Easing.EaseType ParachuteDiveInEasing => parachutingDiveInEasing;
    [SerializeField] private Easing.EaseType parachutingDiveOutEasing = Easing.EaseType.OutSine;
    public Easing.EaseType ParachuteDiveOutEasing => parachutingDiveOutEasing;
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
    [SerializeField] private float parachutingPlayerRadius;
    public float ParachutingPlayerRadius => parachutingPlayerRadius;

    [Header("Parachute Model")] 
    [SerializeField] private Transform parachuteModelTransform;
    public Transform ParachuteModelTransform => parachuteModelTransform;
    [SerializeField] private float parachuteModelScaleInDelay = 0.33f;
    public float ParachuteModelScaleInDelay => parachuteModelScaleInDelay;
    [SerializeField] private float parachuteModelScaleInDuration = 0.5f;
    public float ParachuteModelScaleInDuration => parachuteModelScaleInDuration;
    [SerializeField] private Ease parachuteModelScaleInEase = Ease.OutCirc;
    public Ease ParachuteModelScaleInEase => parachuteModelScaleInEase;
    [SerializeField] private float parachuteModelScaleOutDelay = 0.33f;
    public float ParachuteModelScaleOutDelay => parachuteModelScaleOutDelay;
    [SerializeField] private float parachuteModelScaleOutDuration = 0.5f;
    public float ParachuteModelScaleOutDuration => parachuteModelScaleOutDuration;
    [SerializeField] private Ease parachuteModelScaleOutEase = Ease.OutCirc;
    public Ease ParachuteModelScaleOutEase => parachuteModelScaleOutEase;

    [Header("Landing")] 
    [SerializeField] private float landingDuration = 2f;
    public float LandingDuration => landingDuration;
    [SerializeField] private float animBlendTime = 0.25f;
    public float AnimBlendTime => animBlendTime;
}

public class ParachuteState : MovementState
{
    private static readonly int IsParachuting = Animator.StringToHash("IsParachuting");
    
    public ParachuteState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public ParachutingSettings Settings => stateMachine.ParachutingSettings;
    public override bool UseMouseRotatePlayer => !isParachuting;
    public override bool ControlRotation => isParachuting;

    private bool wasDiving;
    private float currentTurnValue = 0.0f;
    private float smoothedTiltAngle = 0.0f;
    private float diveEaseProgress = 0.0f;
    private float currentDive = 0.0f;
    private bool isParachuting;
    private Sequence parachuteScaleSequence;


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
        stateMachine.PlayerAnimator.CrossFadeInFixedTime("Parachuting", Settings.AnimBlendTime);
        
        currentDive = 0.0f;

        stateMachine.ChangeRadius(Settings.ParachutingPlayerRadius);

        isParachuting = true;
        
        if(Settings.ParachuteModelTransform)
        {
            Settings.ParachuteModelTransform.localScale = Vector3.zero;
            parachuteScaleSequence = Sequence.Create()
                .ChainDelay(Settings.ParachuteModelScaleInDelay)
                .Chain(Tween.Scale(Settings.ParachuteModelTransform, Vector3.one, Settings.ParachuteModelScaleInDuration,Settings.ParachuteModelScaleInEase));
        }

    }

    public override void LateTick()
    {
        
    }

    public override void CheckTransitions()
    { 
        // Cancel state
        if (stateMachine.InputController.CrouchDown)
            SwitchState(stateMachine.FallingState);
        
        // If they hit a wall
        if (stateMachine.IsFacingWall() && stateMachine.ClimbingState.CanClimb())
        {
            SwitchState(stateMachine.ClimbingState);
        }
        
        // If they fall after landing
        if(!isParachuting && !stateMachine.IsGrounded)
            SwitchState(stateMachine.FallingState);
    }

    public override void OnExit()
    {
        EndParachute();

    }

    public override void Tick()
    {
        Parachuting();
        
        // Landed
        if (isParachuting && stateMachine.IsGrounded)
            OnLanded();
    }

    private void Parachuting()
    {
        if (!isParachuting)
            return;
        
        Vector2 input = stateMachine.InputController.FrameMove;
        
        // Turn
        float targetTurnValue = input.x * Settings.ParachuteTurnMaxSpeed;
        currentTurnValue = Mathf.MoveTowards(currentTurnValue, targetTurnValue, Settings.ParachuteTurnAcceleration * Time.deltaTime);
        float finalTurnAngle = stateMachine.RotationEuler.y + currentTurnValue;

        
        
        // Dive
        bool diving = Mathf.Abs(input.y) > 0.1f;

        if (diving != wasDiving)
        {
            diveEaseProgress = 0f;
            wasDiving = diving;
        }

        float currentDiveSpeed = diving ? Settings.ParachuteDiveInSpeed : Settings.ParachuteDiveOutSpeed;
        diveEaseProgress = Mathf.Clamp01(diveEaseProgress + Time.deltaTime * currentDiveSpeed);

        var ease = Easing.FindEaseType(diving 
            ? Settings.ParachuteDiveInEasing
            : Settings.ParachuteDiveOutEasing);

        float eased = ease.Invoke(diveEaseProgress);

        float target = diving ? 1f : 0f;
        currentDive = Mathf.Lerp(currentDive, target, eased);
        
        
        float finalDiveAngle = currentDive * Settings.ParachuteDiveMaxAngle;
        
        // Set rotation
        Quaternion finalRotation = Quaternion.Euler(finalDiveAngle, finalTurnAngle, 0);
        stateMachine.SetRotation(finalRotation);
        
        wasDiving = diving;
        
        
        
        // Velocity
        // Speed boost while diving
        float diveBoost = 1f + (currentDive * Settings.ParachuteDiveSpeedBoost);
        
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

    private void OnLanded()
    {
        EndParachute();
        
        stateMachine.SetVelocity(Vector3.zero);
        
        stateMachine.PlayerAnimator.applyRootMotion = true;
        
        Tween.Delay(Settings.LandingDuration, OnLandedFinished);
        
    }

    private void EndParachute()
    {
        if (!isParachuting)
            return;
        
        isParachuting = false;

        
        stateMachine.PlayerAnimator.SetBool(IsParachuting, false);
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);
        
        // Reset visual rotation
        stateMachine.SetVisualRotation(Quaternion.identity);

        stateMachine.ChangeRadiusDefault();
        

        if(Settings.ParachuteModelTransform)
        {
            if(parachuteScaleSequence.isAlive)
                parachuteScaleSequence.Stop();
            
            parachuteScaleSequence = Sequence.Create()
                .ChainDelay(Settings.ParachuteModelScaleOutDelay)
                .Chain(Tween.Scale(Settings.ParachuteModelTransform, Vector3.zero, Settings.ParachuteModelScaleOutDuration,Settings.ParachuteModelScaleOutEase));
        }
    }

    private void OnLandedFinished()
    {
        stateMachine.PlayerAnimator.applyRootMotion = false;
        SwitchState(stateMachine.WalkingState);
    }

}

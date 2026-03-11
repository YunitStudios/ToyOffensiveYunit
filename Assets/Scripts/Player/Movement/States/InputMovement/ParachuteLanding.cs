using PrimeTween;
using UnityEngine;

[System.Serializable]
public class ParachuteLandingSettings : StateSettings
{
    [Header("Landing")] 
    [SerializeField] private float landingDuration = 2f;
    public float LandingDuration => landingDuration;
    [SerializeField] private float animBlendTime = 0.25f;
    public float AnimBlendTime => animBlendTime;
    [SerializeField] private float landingMovementMuiltiplier = 0.5f;
    public float LandingMovementMuiltiplier => landingMovementMuiltiplier;
    [Tooltip("How long it takes the player to return back to their normal movement speed. Behaves so that the multiplier resets right as the landing duration ends")]
    [SerializeField] private float landingMovementMultiplierTransitionTime = 0.5f;
    public float LandingMovementMultiplierTransitionTime => landingMovementMultiplierTransitionTime;
    [SerializeField] private float landingForwardMovementSpeed = 4;
    public float LandingForwardMovementSpeed => landingForwardMovementSpeed;
    [SerializeField] private AnimationCurve landingForwardCurve;
    public AnimationCurve LandingForwardCurve => landingForwardCurve;
}

public class ParachuteLanding : InputMoveState
{
    private static readonly int IsParachuteLanded = Animator.StringToHash("IsParachuteLanded");

    public new ParachuteLandingSettings Settings => stateMachine.ParachuteLandingSettings;
    public ParachuteLanding(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool CanJump => false;
    public override bool CanAim => false;
    public override bool CanShoot => false;

    private float currentMultiplier;
    private float currentTime;
    private Tween endTween;
    private Tween multiplierTween;

    public override void OnEnter()
    {
        base.OnEnter();

        endTween = Tween.Delay(Settings.LandingDuration, EndLanding);
        
        stateMachine.PlayerAnimator.SetBool(IsParachuteLanded, true);

        stateMachine.SetVelocity(Vector3.zero);

        currentTime = 0.0f;
        
        currentMultiplier = Settings.LandingMovementMuiltiplier;
        multiplierTween = Tween.Delay(Settings.LandingDuration - Settings.LandingMovementMultiplierTransitionTime, EndMultiplier);
    }

    private void EndLanding()
    {
        SwitchState(stateMachine.WalkingState);
    }

    private void EndMultiplier()
    {
        float startValue = Settings.LandingMovementMuiltiplier;
        float endValue = 1;
        multiplierTween = Tween.Custom(startValue, endValue, Settings.LandingMovementMultiplierTransitionTime, v =>
        {
            currentMultiplier = v;
        });
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsParachuteLanded, false);
        
        if(endTween.isAlive)
            endTween.Stop();
        if(multiplierTween.isAlive)
            multiplierTween.Stop();
    }

    public override void Tick()
    {
        base.Tick();

        currentTime += Time.deltaTime;
        
        float t = currentTime / Settings.LandingDuration;

        // Boost velocity forwards
        Vector3 worldVelocity = stateMachine.Rotation * Vector3.forward * Settings.LandingForwardMovementSpeed;
        stateMachine.AddFrameVelocity(worldVelocity * Settings.LandingForwardCurve.Evaluate(t));
    }

    public override void LateTick()
    {
    }

    public override float GetSpeedMultiplier => currentMultiplier;
}

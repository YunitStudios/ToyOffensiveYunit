using System;
using UnityEngine;

public abstract class MovementState : State, IMovementState
{
    [HideInInspector] protected new PlayerMovement stateMachine;

    public MovementState(StateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = (PlayerMovement)stateMachine;
    }


    public virtual bool UseGravity => true;
    public virtual bool UseRootMotion => false;
    public virtual bool UseMouseRotatePlayer => true;
    public virtual bool UseMouseRotateVisuals => false;
    public virtual bool RotatePlayerVertically => false;
    public virtual bool ControlRotation => false;
    public virtual bool UseCollision => true;
    public virtual Vector2 CollisionScale => Vector2.one;
    public virtual bool CanAim => true;
    public virtual bool CanShoot => true;
    public virtual bool ShouldDisplayGun => true;
    public virtual bool PlayFootsteps => true;


    public override void OnEnter()
    {
        stateMachine.PlayerAnimator.applyRootMotion = UseRootMotion;
        stateMachine.ToggleCollision(UseCollision);
        stateMachine.SetCollisionScale(CollisionScale, true);
    }




}

// Used for properties common to all movement states
public interface IMovementState
{
    public bool UseGravity { get; }
    public bool UseRootMotion { get; }
    public bool UseMouseRotatePlayer { get; }
    public bool UseMouseRotateVisuals { get; }
    public bool RotatePlayerVertically { get; }
    
    public bool ControlRotation { get; }
    public bool UseCollision { get; }
    public Vector2 CollisionScale { get; }
    public bool CanAim { get; }
    public bool CanShoot { get; }
    public bool ShouldDisplayGun { get; }
    public bool PlayFootsteps { get; }
}
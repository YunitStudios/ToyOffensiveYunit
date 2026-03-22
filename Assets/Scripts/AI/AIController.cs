using System;
using System.Collections.Generic;
using Throwable_System.ThrowableTypes;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamageable
{
    [SerializeField] private Health enemyHealth;
    private AIStateMachine stateMachine;
    [SerializeField] private Animator aiAnimator;
    public Animator AIAnimator => aiAnimator;
    private NavMeshAgent navMeshAgent;
    [Tooltip("Min and Max speed the moving animation can be, the speed is based on their current speed compared to the max speed")]
    [SerializeField] private Vector2 moveAnimSpeedRange = new(0f, 1f);
    public Vector2 MoveAnimSpeedRange => moveAnimSpeedRange;
    [SerializeField] private float animUpdateDelay = 0.1f;

    [Header("Output Events")] 
    [SerializeField] private VoidEventChannelSO onEnemyKilledWithGrenade;
    [SerializeField] private VoidEventChannelSO onEnemyKilledWithGloryKill;
    
    
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    private static readonly int IsAiming = Animator.StringToHash("IsAiming");

    private CapsuleCollider capCollider;
    private float standHeight = 2f;
    private float crouchHeight = 1f;
    private float targetHeight;
    private Transform playerTransform;
    private float animUpdateTime;
    [HideInInspector] public bool isEnemyAiming;
    
    public IDamageSource RecentDamageSource { get; set; }

    public static Action<IObjectiveTarget> OnEnemyKilled;
    
    private bool isFrozen;
    
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<AIStateMachine>();
        capCollider = GetComponent<CapsuleCollider>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        
        Vector3 localDirection = transform.InverseTransformDirection(navMeshAgent.velocity.normalized);

        animUpdateTime += Time.deltaTime;
        if(animUpdateTime > animUpdateDelay)
        {
            InputMoveState.SetAnimatorMovement(aiAnimator,
                navMeshAgent.speed,
                navMeshAgent.velocity.magnitude,
                localDirection,
                navMeshAgent.speed > 2,
                1.5f,
                MoveAnimSpeedRange);
            animUpdateTime = 0.0f;
        }
    }
    
    public void TakeDamage(IDamageSource source, float damage)
    {
        // prevents squad taking damage during glory kill
        if (stateMachine.IsFrozen)
        {
            return;
        }
        
        if(source != null)
            RecentDamageSource = source;
        
        
        enemyHealth.DealDamage(damage, out bool didDie);
        if (!didDie)
        {
            // Alert squad when damaged 
            stateMachine.AlertSquad(playerTransform);
        }
        if (didDie)
        {
            stateMachine.Die();
            
            // Check if the enemy was a target by doing a TryGetComponent check
            // THIS IS SO JANK I KNOW IM SORRY BUT IDK HOW ELSE AND IM TIRED OKAY
            bool isTarget = TryGetComponent<ObjectiveTarget>(out _);
            
            GameManager.ScoreTracker.RegisterKill(ScoreTrackerSO.KillTypes.Generic, RecentDamageSource, isTarget);
            OnEnemyKilled?.Invoke(enemyHealth);
            
            if(source is ExplosiveGrenade)
                onEnemyKilledWithGrenade?.Invoke();
            if(source is GloryKill)
                onEnemyKilledWithGloryKill?.Invoke();
        }
    }

    // Sets crouching/ standing animation and stats
    public void SetCrouching(bool isCrouching)
    {
        if (isCrouching)
        {
            aiAnimator.SetBool(IsCrouching, true);
            targetHeight = crouchHeight;
            Vector3 center = capCollider.center;
            center.y = targetHeight / 2f;
            capCollider.center = -center;
        }
        else
        {
            aiAnimator.SetBool(IsCrouching, false);
            targetHeight =  standHeight;
            Vector3 center = capCollider.center;
            center.y = 0;
            capCollider.center = center;
        }
        capCollider.height = targetHeight;
        navMeshAgent.height = targetHeight;
    }

    public void SetAiming(bool isAiming)
    {
        if (isAiming)
        {
            aiAnimator.SetBool(IsAiming, true);
            isEnemyAiming = true;
        }
        else
        {
            aiAnimator.SetBool(IsAiming, false);
            isEnemyAiming = false;
        }
    }
}

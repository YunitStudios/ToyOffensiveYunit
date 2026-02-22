using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour, IDamageable
{
    [SerializeField] private Health enemyHealth;
    private AIStateMachine stateMachine;
    [SerializeField] private Animator aiAnimator;
    public Animator AIAnimator => aiAnimator;
    private NavMeshAgent navMeshAgent;
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    private CapsuleCollider capCollider;
    private float standHeight = 2f;
    private float crouchHeight = 1f;
    private float targetHeight;
    private Transform playerTransform;
    
    public IDamageSource RecentDamageSource { get; set; }

    public static Action<IObjectiveTarget> OnEnemyKilled;
    
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<AIStateMachine>();
        capCollider = GetComponent<CapsuleCollider>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Walking animation based on current speed
        float horizontalSpeed = navMeshAgent.velocity.magnitude;
        // Divide by max speed to get 0-1 range
        horizontalSpeed /= navMeshAgent.speed;
        // Half to fit walking blend tree
        horizontalSpeed /= 2f;
        // If sprinting, remove multiplier and instead double to get full speed
        if (navMeshAgent.speed > 2)
        {
            horizontalSpeed *= 2f;
        }
        aiAnimator.SetFloat(AnimMoveSpeed, horizontalSpeed , 0.2f, Time.deltaTime);
    }
    
    public void TakeDamage(IDamageSource source, float damage)
    {
        if(source != null)
            RecentDamageSource = source;
        
        // Alert squad when damaged 
        stateMachine.AlertSquad(playerTransform);
        
        enemyHealth.DealDamage(damage, out bool didDie);
        if (didDie)
        {
            stateMachine.Die();
            
            // Check if the enemy was a target by doing a TryGetComponent check
            // THIS IS SO JANK I KNOW IM SORRY BUT IDK HOW ELSE AND IM TIRED OKAY
            bool isTarget = TryGetComponent<ObjectiveTarget>(out _);
            
            GameManager.ScoreTracker.RegisterKill(ScoreTrackerSO.KillTypes.Generic, RecentDamageSource, isTarget);
            OnEnemyKilled?.Invoke(enemyHealth);
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
}

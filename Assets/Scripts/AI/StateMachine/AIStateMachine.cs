using System;
using System.Collections.Generic;
using Player.Inventory.PickupSystem;
using UnityEngine;
using UnityEngine.AI;

public class AIStateMachine : MonoBehaviour
{

    public enum EnemyType
    {
        Patrol,
        Target,
        Guard,
        Stationary
    }
    [Tooltip("Assign the enemy type here")]
    [SerializeField] private EnemyType enemyType;

    public EnemyType Type => enemyType;
    
    [HideInInspector] public NavMeshAgent agent;
    private AIState currentState;
    public AIState CurrentState => currentState;
    [SerializeField] private string debugState;
    [HideInInspector] public Health health;
    
    [Tooltip("Set waypoints by creating empty game objects in the scene, then setting their transform to the waypoint.")]
    [SerializeField] private List<Transform> waypoints;

    [Tooltip("Assign the enemy you want this enemy to follow here")]
    public Transform commander;

    [Tooltip("Assign the X and Z values here for offsets from the commander position. (Y value is actually Z)")]
    public Vector2 formationOffset;
    [HideInInspector] public int currentWaypoint; 
    [HideInInspector] public AIVision vision;
    [HideInInspector] public AIDetection detection;
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    private AIController aiController;

    [Tooltip("Time before enemy is destroyed after being killed")]
    [SerializeField] private float DeathDelay = 0;
    
    [HideInInspector] public Vector3 stationPosition;

    [Header("Weapon Settings")]
    [Tooltip("Damage multiplier for enemy weapons")]
    [SerializeField] private float damageMultiplier = 0.5f;
    public float DamageMultiplier => damageMultiplier;
    [Tooltip("Accuracy multiplier for enemy weapons (higher = less accurate)")]
    [SerializeField] private float accuracyMultiplier = 1f;
    public float AccuracyMultiplier => accuracyMultiplier;
    
    [Header("Healing Settings")]
    [Tooltip("Does this enemy hold a medkit?")]
    [SerializeField] private bool holdMedkit = false;
    public bool HoldMedkit => holdMedkit;
    [Tooltip("Once this health percentage is reached, AI will try heal itself")]
    [SerializeField] private float healAtPercent = 0.3f;
    [Tooltip("Amount of health to heal")] 
    [SerializeField] private float healAmount = 50f;
    public float HealAmount => healAmount;

    [Header("Guard Settings")]
    [Tooltip("The Target to guard")]
    [SerializeField] private AIStateMachine protectedTarget;
    [Tooltip("The distance the guard can move from target")]
    [SerializeField] private float protectRadius = 10f;
    [Tooltip("Once this health percentage is reached, guard will go and try to heal the target")]
    [SerializeField] private float assistAtHealthPercent = 0.5f;

    [HideInInspector] public bool isBeingAssisted;
    [HideInInspector] public bool inCover = false;

    [Tooltip("The distance from the enemy that a throwable can be detected")]
    [SerializeField] private float grenadeCheckRadius = 10f;
    
    [HideInInspector] public static List<AIStateMachine> TargetsAndGuards = new List<AIStateMachine>();

    public static Action<bool> OnFreezeAllAI;
    private bool isFrozen;
    public bool IsFrozen => isFrozen;

    // Sets starting states for AI 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponentInChildren<AIVision>();
        detection = GetComponent<AIDetection>();
        aiController = GetComponent<AIController>();
        health =  GetComponent<Health>();

        // Sets station point to enemies start location
        if (enemyType == EnemyType.Stationary)
        {
            stationPosition = transform.position;
        }
        ReturnToStartingState();
    }

    void Update()
    {
        debugState = currentState.GetType().Name;
        
        if (isFrozen)
        {
            return;
        }
        
        CheckForThreats();
        
        currentState?.Execute();
        
        if (currentState is EvadeState)
        {
            return;
        }

        if (!detection.IsDetected && detection.ShouldInvestigate && !(currentState is SearchState) &&
            !(currentState is AttackState) && !(currentState is MoveToCoverState) &&
            !(currentState is BehindCoverState) && !(currentState is PeekShootState) && !(currentState is FleeState))
        {
            ChangeState(new SearchState(this, agent, detection.LastKnownPosition));
        }

        if (health != null && HoldMedkit && health.CurrentHealth / health.MaxHealth <= healAtPercent &&
            !(currentState is HealingState) && !(currentState is AssistState))
        {
            ChangeState(new HealingState(this, agent));
            return;
        }
        
        // if enemy is a guard start protecting functionality
        if (enemyType == EnemyType.Guard)
        {
            Protecting();
        }
        // if player enters vision collider, switch to their alert state
        if (detection.IsDetected && enemyType == EnemyType.Target && !(currentState is FleeState) && !(CurrentState is MoveToAssistCoverState))
        {
            ChangeState(new FleeState(this, agent, vision.player));
        }
        if (detection.IsDetected && !(currentState is AttackState) && !(currentState is MoveToCoverState) && !(currentState is BehindCoverState) && !(currentState is PeekShootState) && enemyType != EnemyType.Target)
        { 
            ChangeState(new AttackState(this, agent, vision.player));
        }
        // else if cant see player and is in attack state, switch to search state
        else if (!vision.canSeePlayer && (currentState is AttackState || currentState is MoveToCoverState || currentState is BehindCoverState || currentState is PeekShootState))
        {
            if (Time.time - vision.lastSeenTime > vision.SearchTimeout)
            {
                aiController.AIAnimator.SetBool(IsCrouching, false);
                if (enemyType == EnemyType.Stationary)
                {
                    ChangeState(new ReturnToStationState(this, agent, stationPosition));
                }
                else
                {
                    ChangeState(new SearchState(this, agent, detection.LastKnownPosition));
                }
            }
        }
    }

    // function to change state
    public void ChangeState(AIState newState)
    {
        if (newState is PatrolState)
        {
            CommanderController commander = GetComponent<CommanderController>();
            if (commander != null && commander.IsCommander)
            {
                commander.hasGroupedUp = false;
            }
            vision.ResetVision();
        }

        if (newState is AttackState)
        {
            vision.IncreaseVision();
        }

        if ((currentState is MoveToCoverState || currentState is BehindCoverState || currentState is PeekShootState) && !(newState is MoveToCoverState || newState is BehindCoverState || newState is PeekShootState))
        {
            vision.ResetVision();
            CoverPoint[] points = FindObjectsOfType<CoverPoint>();
            foreach (CoverPoint point in points)
            {
                if (point.aiStateMachine == this)
                {
                    point.LeaveCoverPoint();
                }
            }
        }
        currentState = newState;
    }
    
    // When AI dies, it changes state and if it was a commander a new one is set, or if just a follower then it is removed from commanders list
    public void Die()
    {
        isBeingAssisted = false;
        
        ChangeState(new DeathState(this, agent));
        CommanderController commanderController = GetComponent<CommanderController>();
        if (commanderController != null && commanderController.IsCommander)
        {
            foreach (AIStateMachine follower in commanderController.Followers)
            {
                if (follower != null)
                {
                    follower.commander = null;
                }
            }
            commanderController.PromoteNewCommander();
        }
         
        if (commander != null)
        {
            CommanderController followerCommander = commander.GetComponent<CommanderController>();
            if (followerCommander != null && followerCommander.IsCommander)
            {
                followerCommander.Followers.Remove(this);
            }
        }
        
        CoverPoint[] points = FindObjectsOfType<CoverPoint>();
        foreach (CoverPoint point in points)
        {
            if (point.aiStateMachine == this)
            {
                point.LeaveCoverPoint();
            }
        }

        if (GetComponent<PickupSpawner>() is not null)
        {
            PickupSpawner spawner = GetComponent<PickupSpawner>();
            spawner.SpawnPickup(spawner.database.GetRandomPickup(), transform.position);
        }
        Destroy(gameObject, DeathDelay);
    }

    public void ReturnToStartingState()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            ChangeState(new PatrolState(this, agent, waypoints, currentWaypoint));
        }
        else if (commander != null)
        {
            ChangeState(new FollowCommanderState(this, agent, commander, formationOffset));
        }
        else
        {
            ChangeState(new StationaryState(this, agent));
        }
    }
    
    // Alerts enemy squad, setting all of their states to their specific alert state, if not already
    public void AlertSquad(Transform player)
    {
       ReactToAlert(player);
        
        if (commander != null)
        {
            CommanderController commanderController = commander.GetComponent<CommanderController>();
            if (commanderController != null && commanderController.IsCommander)
            {
                // Alerts commander if follower
                AIStateMachine commanderAI = commanderController.GetComponent<AIStateMachine>();
                if (commanderAI != null)
                {
                    commanderAI.ReactToAlert(player);
                }
                // Alerts all followers if follower
                foreach (AIStateMachine follower in commanderController.Followers)
                {
                    if (follower != null)
                    {
                        follower.ReactToAlert(player);
                    }
                }
            }
        }
        
        CommanderController selfCommander = GetComponent<CommanderController>();
        if (selfCommander != null && selfCommander.IsCommander)
        {
            // Alerts all followers if commander
            foreach (AIStateMachine follower in selfCommander.Followers)
            {
                if (follower != null)
                {
                    follower.ReactToAlert(player);
                }
            }
        }
    }

    // When enemy is alerted, new states are set here based on enemy desired behaviour.
    private void ReactToAlert(Transform player)
    {
        vision.canSeePlayer = true;
        detection.AddDetection(100);
        vision.lastSeenTime = Time.time;
        
        // if Enemy is a target it will enter the flee state
        if (enemyType == EnemyType.Target && !(currentState is FleeState))
        {
            ChangeState(new FleeState(this, agent, player));
        }
        // if Enemy is not a target it will enter the attack state
        else
        {
            if (!(currentState is AttackState) && !(currentState is MoveToCoverState) &&
                !(currentState is BehindCoverState) && !(currentState is PeekShootState))
            {
                ChangeState(new AttackState(this, agent, player));
            }
        }
    }

    public List<Transform> Waypoints
    {
        get { return waypoints; }
        set { waypoints = value; }
    }

    private void Protecting()
    {
        // if target is no longer alive then patrol
        if (protectedTarget == null)
        {
            enemyType = EnemyType.Patrol;
            ReturnToStartingState();
            return;
        }
        
        // if too far from target then return
        float distanceToTarget = Vector3.Distance(transform.position, protectedTarget.transform.position);
        if (distanceToTarget > protectRadius)
        {
            if (!(CurrentState is FollowCommanderState))
            {
                ChangeState(new FollowCommanderState(this, agent, protectedTarget.transform, formationOffset));
            }

            return;
        }
        
        // assists target if health is low
        Health targetHealth = protectedTarget.GetComponent<Health>();
        if (targetHealth != null && targetHealth.CurrentHealth / targetHealth.MaxHealth <= assistAtHealthPercent && !protectedTarget.isBeingAssisted && HoldMedkit)
        {
            ChangeState(new AssistState(this, agent, protectedTarget));
        }
    }
    
    private void CheckForThreats()
    {
        float threatCheckRadius = 8f;
        Collider[] hits = Physics.OverlapSphere(transform.position, threatCheckRadius);
        foreach (Collider hit in hits)
        {
            ThrowableTemplate grenade = hit.GetComponent<ThrowableTemplate>();
            if (grenade != null && grenade.Damage > 0f)
            {
                if (!(currentState is EvadeState))
                {
                    ChangeState(new EvadeState(this, agent, grenade.transform, vision.player));
                }

                return;
            }
        }
    }

    public void SetTypeToPatrol()
    {
        enemyType = EnemyType.Patrol;
    }
    
    public void SetTypeToTarget()
    {
        enemyType = EnemyType.Target; 
    }
    
    public void SetTypeToGuard()
    {
        enemyType = EnemyType.Guard;
    }
    
    public void SetTypeToStationary()
    {
        enemyType = EnemyType.Stationary;
    }

    public void SetProtectedTarget(AIStateMachine target)
    {
        protectedTarget = target;
    }

    public void SetMedkit(bool hasMedkit)
    {
        holdMedkit =  hasMedkit;
    }

    void OnEnable()
    {
        OnFreezeAllAI += HandleFreeze;
        if (Type == EnemyType.Guard || Type == EnemyType.Target)
        {
            TargetsAndGuards.Add(this);
        }
    }

    void OnDisable()
    {
        OnFreezeAllAI -= HandleFreeze;
        if (TargetsAndGuards.Contains(this))
        {
            TargetsAndGuards.Remove(this);
        }
    }

    void HandleFreeze(bool frozen)
    {
        isFrozen = frozen;
        agent.isStopped = frozen;
    }
}

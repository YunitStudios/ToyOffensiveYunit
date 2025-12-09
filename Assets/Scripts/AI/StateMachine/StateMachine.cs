using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateMachine : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    private AIState currentState;
    public AIState CurrentState => currentState;
    
    [Tooltip("Set waypoints by creating empty game objects in the scene, then setting their transform to the waypoint.")]
    [SerializeField] private List<Transform> waypoints;

    [Tooltip("Assign the enemy you want this enemy to follow here")]
    public Transform commander;

    [Tooltip("Assign the X and Z values here for offsets from the commander position. (Y value is actually Z)")]
    public Vector2 formationOffset;
    [HideInInspector] public int currentWaypoint; 
    [HideInInspector] public AIVision vision;
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    private AIController aiController;

    // Sets starting states for AI 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponentInChildren<AIVision>();
        Debug.Log(vision);
        aiController = GetComponent<AIController>();
        ReturnToStartingState();
    }

    void Update()
    {
        currentState?.Execute();
        // if player enters vision collider, switch to attack state
        if (vision.canSeePlayer && !(currentState is AttackState) && !(currentState is MoveToCoverState) && !(currentState is BehindCoverState) && !(currentState is PeekShootState))
        { 
            ChangeState(new AttackState(this, agent, vision.player));
        }
        // else if cant see player and is in attack state, switch to search state
        else if (!vision.canSeePlayer && (currentState is AttackState || currentState is MoveToCoverState || currentState is BehindCoverState || currentState is PeekShootState))
        {
            if (Time.time - vision.lastSeenTime > vision.SearchTimeout)
            {
                aiController.AIAnimator.SetBool(IsCrouching, false);
                ChangeState(new SearchState(this, agent, vision.lastSeenPosition));
            }
        }
    }

    // function to change state
    public void ChangeState(AIState newState)
    {
        if (newState is PatrolState)
        {
            CommanderController commander = GetComponent<CommanderController>();
            commander.hasGroupedUp = false;
        }

        if (currentState is MoveToCoverState || currentState is BehindCoverState || currentState is PeekShootState && !(newState is MoveToCoverState || newState is BehindCoverState || newState is PeekShootState))
        {
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
        ChangeState(new DeathState(this, agent));
        CommanderController commanderController = GetComponent<CommanderController>();
        if (commanderController != null)
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
            if (followerCommander != null)
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

        Destroy(gameObject, 2f);
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
    }
    
    // Alerts enemy squad, setting all of their states to attack, if not already
    public void AlertSquad(Transform player)
    {
        if (commander != null)
        {
            CommanderController commanderController = commander.GetComponent<CommanderController>();
            if (commanderController != null)
            {
                // Alerts commander if follower
                AIStateMachine commanderAI = commanderController.GetComponent<AIStateMachine>();
                if (commanderAI != null && !(commanderAI.CurrentState is AttackState) && !(commanderAI.CurrentState is MoveToCoverState) && !(commanderAI.CurrentState is BehindCoverState) && !(commanderAI.CurrentState is PeekShootState))
                {
                    commanderAI.ChangeState(new AttackState(commanderAI, commanderAI.agent, player));
                }
                // Alerts all followers if follower
                foreach (AIStateMachine follower in commanderController.Followers)
                {
                    if (follower != null && !(follower.CurrentState is AttackState) && !(follower.CurrentState is MoveToCoverState) && !(follower.CurrentState is BehindCoverState) && !(follower.CurrentState is PeekShootState))
                    {
                        follower.ChangeState(new AttackState(follower, follower.agent, player));
                    }
                }
            }
        }
        
        CommanderController selfCommander = GetComponent<CommanderController>();
        if (selfCommander != null)
        {
            // Alerts all followers if commander
            foreach (AIStateMachine follower in selfCommander.Followers)
            {
                if (follower != null && !(follower.CurrentState is AttackState) && !(follower.CurrentState is MoveToCoverState) && !(follower.CurrentState is BehindCoverState) && !(follower.CurrentState is PeekShootState))
                {
                    follower.ChangeState(new AttackState(follower, follower.agent, player));
                }
            }
        }
    }

    public List<Transform> Waypoints
    {
        get { return waypoints; }
        set { waypoints = value; }
    }
    
}

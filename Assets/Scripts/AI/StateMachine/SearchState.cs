using UnityEngine;
using UnityEngine.AI;

public class SearchState : AIState
{
    private Vector3 searchPosition;
    private float timer;
    private float searchTime = 3f;
        
    public SearchState(AIStateMachine controller, NavMeshAgent agent, Vector3 searchPosition) : base(controller, agent)
    {
        this.searchPosition = searchPosition;
        agent.isStopped = false;
    }

    public override void Execute()
    {
        agent.SetDestination(searchPosition);
        if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
        { 
            agent.isStopped = true;
            timer += Time.deltaTime;
        }

        if (timer >= searchTime)
        {
            controller.ReturnToStartingState();
        }
    }
}

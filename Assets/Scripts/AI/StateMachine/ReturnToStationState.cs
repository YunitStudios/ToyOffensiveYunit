using UnityEngine;
using UnityEngine.AI;

public class ReturnToStationState : AIState
{
    private Vector3 station;
    
    public ReturnToStationState(AIStateMachine controller, NavMeshAgent agent, Vector3 station) : base(controller,
        agent)
    {
        this.station = station;
        agent.isStopped = false;
        agent.SetDestination(station);
    }

    public override void Execute()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.isStopped = true;
            controller.ReturnToStartingState();
        }
    }
}

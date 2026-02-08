using UnityEngine;
using UnityEngine.AI;

public class MoveToAssistCoverState : AIState
{
    private CoverPoint coverPoint;

    public MoveToAssistCoverState(AIStateMachine controller, NavMeshAgent agent, CoverPoint coverPoint) : base(
        controller, agent)
    {
        this.coverPoint = coverPoint;
        agent.isStopped = false;
        agent.SetDestination(coverPoint.transform.position);
    }

    public override void Execute()
    {
        if (coverPoint == null)
        {
            controller.ReturnToStartingState();
            return;
        }
        
        agent.SetDestination(coverPoint.transform.position);
        if (!agent.pathPending && agent.remainingDistance <= 1.0f)
        {
            agent.isStopped = true;
            controller.GetComponent<AIController>().SetCrouching(true);
            controller.inCover = true;
        }
    }
}

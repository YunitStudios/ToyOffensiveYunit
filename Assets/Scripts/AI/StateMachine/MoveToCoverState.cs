using UnityEngine;
using UnityEngine.AI;

public class MoveToCoverState : AIState
{
    private CoverPoint coverPoint;
    private Transform player;
    private float moveToCoverSpeed = 3f;
    private float regularSpeed = 2f;

    public MoveToCoverState(AIStateMachine controller, NavMeshAgent agent, CoverPoint point, Transform player) : base(controller, agent)
    {
        this.coverPoint = point;
        this.player = player;
        agent.isStopped = false;
    }

    public override void Execute()
    {
        // Sets enemy destination to the cover point and increases enemy speed until it reaches destination
        agent.SetDestination(coverPoint.transform.position);
        agent.speed = moveToCoverSpeed;
        if (agent.remainingDistance <= 0.1f)
        {
            agent.speed = regularSpeed;
            agent.isStopped = true;
            controller.ChangeState(new BehindCoverState(controller, agent, coverPoint, player));
        }
    }
}

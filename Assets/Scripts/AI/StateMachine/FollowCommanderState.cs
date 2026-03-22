using UnityEngine;
using UnityEngine.AI;

public class FollowCommanderState : AIState
{
    private Transform commander;
    private Vector2 offset;
    private float catchupDistance = 2f;
    private float catchupSpeed = 3f;
    private float regularSpeed = 2f;
    
    public FollowCommanderState(AIStateMachine controller, NavMeshAgent agent, Transform commander, Vector2 offset) : base(controller, agent)
    {
        this.commander = commander;
        this.offset = offset;
        agent.isStopped = false;
    }

    // sets AI to follow the offset position from the commander
    public override void Execute()
    {
        Vector3 Offset = commander.right * offset.x + commander.forward * offset.y;
        Vector3 target = commander.position + Offset;
        agent.SetDestination(target);
        
        // checks distance to see if AI needs to speed up to get back to the formation
        float distanceToTarget = Vector3.Distance(agent.transform.position, target);
        if (distanceToTarget > catchupDistance)
        {
            agent.speed = catchupSpeed;
        }
        else
        {
            agent.speed = regularSpeed;
        }
    }
}

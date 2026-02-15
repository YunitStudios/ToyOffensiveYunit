using UnityEngine;
using UnityEngine.AI;

public class EvadeState : AIState
{
    private Transform threat;
    private float evadeDistance = 15f;
    private Transform player;
    private float EvadeSpeed = 4f;
    private float regularSpeed = 2f;
    
    public EvadeState(AIStateMachine controller, NavMeshAgent agent, Transform threat, Transform player) : base(controller, agent)
    {
        this.threat = threat;
        this.player = player;
        // direction from the threat
        Vector3 direction = (controller.transform.position - threat.position).normalized;
        Vector3 targetPosition = controller.transform.position + direction * evadeDistance;
        // Uses nav mesh to find a position close to the target position
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.speed = EvadeSpeed;
            agent.SetDestination(hit.position);
        }
    }

    public override void Execute()
    {
        // if threat no longer exists, change state
        if (threat == null)
        {
            agent.speed = regularSpeed;
            controller.ChangeState(new AttackState(controller, agent, player));
            return;
        }
        // if agent reached evade location, change state
        float distance = Vector3.Distance(controller.transform.position, threat.position);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.speed = regularSpeed;
            controller.ChangeState(new AttackState(controller, agent, player));
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class AttackState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private float stoppingDistance = 12f;
    private CoverPoint coverPoint;
    float distanceToPlayer;
    float distanceToCoverPoint;
    private AIController aiController;


    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
        aiController = controller.GetComponent<AIController>();
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        if (coverPoint == null)
        {
            coverPoint = CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, player, controller);

            if (coverPoint != null)
            {
                distanceToCoverPoint = Vector3.Distance(controller.transform.position, coverPoint.transform.position);
            }
        }
        distanceToPlayer = Vector3.Distance(controller.transform.position, player.position);
        if (coverPoint != null && controller.AttackRange < distanceToPlayer)
        {
            coverPoint.TakeCoverPoint(controller);
            controller.ChangeState(new MoveToCoverState(controller, agent, coverPoint, player));
            return;
        }
        
        agent.SetDestination(player.position);
        weaponSystem.target = player;
        aiController.SetAiming(true);
        weaponSystem.Fire();
        if (agent.remainingDistance <= controller.StoppingDistance && HasLineOfSight())
        {
            agent.isStopped = true;
            RotateTowardsPlayer();
        }
        else
        {
            agent.isStopped = false;
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 from = controller.transform.position + Vector3.up;
        Vector3 to = player.position + Vector3.up;
        Vector3 dir = to - from;
        float dist = dir.magnitude * 2f;

        // Raycast to check it can see player
        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, ~LayerMask.GetMask("Enemy", "Vision")))
        {
            return hit.transform == player;
        }

        return false;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 lookDirection = (player.position - controller.transform.position).normalized;
        lookDirection.y = 0;
        controller.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

}

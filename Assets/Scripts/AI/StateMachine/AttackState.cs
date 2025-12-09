using UnityEngine;
using UnityEngine.AI;

public class AttackState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private float stoppingDistance = 10f;
    private CoverPoint coverPoint;


    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        if (coverPoint == null)
        {
            coverPoint = CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, player);

            if (coverPoint != null)
            {
                coverPoint.TakeCoverPoint(controller);
                controller.ChangeState(new MoveToCoverState(controller, agent, coverPoint, player));
                return;
            }
        }
        
        agent.SetDestination(player.position);
        if (agent.remainingDistance <= stoppingDistance)
        {
            agent.isStopped = true;
            weaponSystem.target = player;
            weaponSystem.Fire();
        }
        else
        {
            agent.isStopped = false;
        }
    }
}

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
    private float coverCheckTime;
    private float reactionTimer = 0f;
    private bool engaging = false;


    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player, bool isEngaging) : base(controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
        aiController = controller.GetComponent<AIController>();
        weaponSystem.target = player;
        agent.speed = 4f;
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        if (controller.IsThreatFound)
        {
            return;
        }
        
        coverCheckTime += Time.deltaTime;
        if(coverCheckTime >= controller.CoverCheckDelay)
        {
            coverCheckTime = 0.0f;
            if (coverPoint == null)
            {
                coverPoint =
                    CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, player, controller);

                if (coverPoint != null)
                {
                    distanceToCoverPoint =
                        Vector3.Distance(controller.transform.position, coverPoint.transform.position);
                }
            }

            distanceToPlayer = Vector3.Distance(controller.transform.position, player.position);
            if (coverPoint != null && controller.AttackRange < distanceToPlayer && !engaging)
            {
                coverPoint.TakeCoverPoint(controller);
                aiController.SetAiming(false);
                controller.ChangeState(new MoveToCoverState(controller, agent, coverPoint, player));
                return;
            }
            
            agent.SetDestination(player.position);
            engaging = true;
        }
        
        if (engaging)
        {
            if (agent.remainingDistance <= controller.StoppingDistance && HasLineOfSight())
            {
                agent.isStopped = true;
                agent.speed = 2f;
            }
            else
            {
                agent.isStopped = false;
                agent.speed = 4f;
            }

            if (HasLineOfSight())
            {
                RotateTowardsPlayer();
                aiController.SetAiming(true);
                weaponSystem.Fire();
            }
            else
            {
                aiController.SetAiming(false);
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 from = controller.transform.position + Vector3.up;
        Vector3 to = player.position + Vector3.up;
        Vector3 dir = to - from;
        float dist = dir.magnitude * 2f;

        // Raycast to check it can see player
        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, LayerMask.GetMask("Default", "Environment")))
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

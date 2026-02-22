using UnityEngine;
using UnityEngine.AI;

public class FleeState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private float fleeSpeed = 4f;
    private float shootCooldown = 2f;
    private float shootPeriod = 2f;
    private float shootCooldownTimer;
    private float shootPeriodTimer;
    private bool destinationSet = false;
    private Vector3 fleeDestination;
    private SafePoint chosenSafePoint;

    public FleeState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(
        controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
        agent.speed  = fleeSpeed;
        agent.isStopped = false;
        controller.GetComponent<AIController>().SetCrouching(false);
        controller.inCover = false;
        ChooseFleeDestination();
    }

    public override void Execute()
    {
        if (chosenSafePoint != null && chosenSafePoint.isCompromised)
        {
            destinationSet = false;
            chosenSafePoint = null;
            return;
        }
        
        if (!destinationSet)
        {
            ChooseFleeDestination();
        }
        
        agent.SetDestination(fleeDestination);
        shootCooldownTimer += Time.deltaTime;
        
        if (shootCooldownTimer >= shootCooldown)
        {
            shootPeriodTimer += Time.deltaTime;
            if (shootPeriodTimer <= shootPeriod)
            {
                if (HasLineOfSight())
                {
                    RotateTowardsPlayer();
                    weaponSystem.target = player;
                    weaponSystem.Fire();
                }
            }
            else
            {
                shootCooldownTimer = 0f;
                shootPeriodTimer = 0f;
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= 1f)
        {
            //controller.ChangeState();
        }
    }

    private void ChooseFleeDestination()
    {
        SafePoint safePoint = SafePointManager.instance.GetNearestSafePoint(controller.transform.position);
        if (safePoint != null)
        {
            chosenSafePoint = safePoint;
            fleeDestination = safePoint.transform.position;
            destinationSet = true;
            return;
        }
        
        CoverPoint coverPoint = CoverPointManager.instance.GetFurthestCoverPoint(controller.transform.position, player);
        if (coverPoint != null)
        {
            coverPoint.TakeCoverPoint(controller);
            fleeDestination = coverPoint.transform.position;
            destinationSet = true;
            return;
        }
        
        Vector3 away = (controller.transform.position - player.position).normalized;
        fleeDestination = controller.transform.position + away * 10f;
        destinationSet = true;
    }
    
    private bool HasLineOfSight()
    {
        Vector3 from = controller.transform.position + Vector3.up * 0.5f;
        Vector3 to = player.position + Vector3.up * 1.5f;
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

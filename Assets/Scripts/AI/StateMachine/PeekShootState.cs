using UnityEngine;
using UnityEngine.AI;

public class PeekShootState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private CoverPoint coverPoint;
    // time before returning to cover
    private float peekDuration = 4f;
    private float timer;
    private Vector3 coverPosition;
    // direction to move to peek around cover
    private Vector3 moveDirection;
    private float stepSize = 1f;
    private float sampleRadius = 10f;
    private AIController aiController;

    public PeekShootState(AIStateMachine controller, NavMeshAgent agent, CoverPoint coverPoint, Transform player) : base(controller, agent)
    {
        this.coverPoint = coverPoint;
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
        aiController = controller.GetComponent<AIController>();
        
        coverPosition = coverPoint.transform.position;
        Vector3 directionToPlayer = (player.position - coverPosition).normalized;
        moveDirection = Vector3.Cross(Vector3.up, directionToPlayer).normalized;
        if (moveDirection == Vector3.zero)
        {
            moveDirection = Vector3.right;
        }
    }

    public override void Execute()
    {
        agent.isStopped = false;
        timer += Time.deltaTime;
        if (timer >= peekDuration)
        {
            weaponSystem.Reload();
            controller.ChangeState(new BehindCoverState(controller, agent, coverPoint, player));
            return;
        }

        if (!coverPoint.IsStandingCover)
        {
            aiController.SetCrouching(false);
            if (HasLineOfSight())
            {
                RotateTowardsPlayer();
                weaponSystem.target = player;
                weaponSystem.Fire();
            }

            return;
        }

        if (coverPoint.IsStandingCover)
        {
            if (!HasLineOfSight())
            {
                // Moves enemy towards target position, 
                Vector3 targetPosition = controller.transform.position + moveDirection * stepSize;
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    // if first direction fails, try other direction
                    Vector3 opposite = controller.transform.position - moveDirection * stepSize;
                    if (NavMesh.SamplePosition(opposite, out hit, sampleRadius, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                    else
                    {
                        // when both direction don't work
                        agent.isStopped = true;
                    }
                }
            }
            else
            {
                // enemy has line of sight on player, so will shoot
                agent.isStopped = true;
                RotateTowardsPlayer();
                weaponSystem.target = player;
                weaponSystem.Fire();
            }
        }
    }
    
    private bool HasLineOfSight()
    {
        Vector3 from = controller.transform.position + Vector3.up;
        Vector3 to = player.position + Vector3.up;
        Vector3 dir = to - from;
        float dist = dir.magnitude;

        // Raycast to check it can see player
        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, ~LayerMask.GetMask("Enemy")))
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

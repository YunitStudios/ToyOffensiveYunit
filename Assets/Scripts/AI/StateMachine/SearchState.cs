using UnityEngine;
using UnityEngine.AI;

public class SearchState : AIState
{
    private Vector3 searchPosition;
    private float timer = 0f;
    private float searchTime = 3f;
    private bool reachedPosition = false;
    private float pauseTimer = 0f;
    private float pauseTime = 1f;
    private bool isPaused = true;
    private float stuckTimer = 0f;
    private float stuckTime = 6f;
    private bool wasShot = false;
    private float originalSpeed;
    private bool usingCover = false;
    private AIController aiController;
        
    public SearchState(AIStateMachine controller, NavMeshAgent agent, Vector3 searchPosition, bool wasShot) : base(controller, agent)
    {
        this.searchPosition = GetRandomPointOnNavMesh(searchPosition, 5f);
        this.wasShot = wasShot;
        agent.isStopped = true;
        controller.detection.isSearching = true;
        originalSpeed = agent.speed;
        if (wasShot)
        {
            agent.speed = 4f;
        }
        aiController = controller.GetComponent<AIController>();
    }

    public override void Execute()
    {
        if (isPaused)
        {
            pauseTimer += Time.deltaTime;
            Vector3 direction = (searchPosition - agent.transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, Time.deltaTime * 3);
            }

            if (pauseTimer >= pauseTime)
            {
                isPaused = false;
                if (wasShot && !CanReachPosition(searchPosition))
                {
                    CoverPoint cover = CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, controller.vision.player , controller);
                    if (cover != null)
                    {
                        searchPosition = cover.transform.position;
                        usingCover = true;
                    }
                }
                agent.isStopped = false;
                agent.SetDestination(searchPosition);
            }

            return;
        }
        
        if (!reachedPosition)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                reachedPosition = true;
                agent.isStopped = true;
                stuckTimer = stuckTime;
                if (usingCover)
                {
                    aiController.SetCrouching(true);
                }
            }
            else
            {
                if (agent.velocity.sqrMagnitude < 0.01f)
                {
                    stuckTimer += Time.deltaTime;
                }
                else
                {
                    stuckTimer = 0f;
                }
            }

            return;
        }

        if (!(stuckTimer >= stuckTime))
        {
            return;
        }
        
        controller.detection.isSearching = false;
        if (controller.detection.DetectionPercent <= controller.detection.InvestigateThreshold)
        {
            aiController.SetCrouching(false);
            agent.speed = originalSpeed;
            controller.ReturnToStartingState();
        }
    }

    // gets random point in a radius around search position
    private Vector3 GetRandomPointOnNavMesh(Vector3 position, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius + position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return position;
    }

    // checks if enemy can reach the search location
    private bool CanReachPosition(Vector3 position)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(position, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
}

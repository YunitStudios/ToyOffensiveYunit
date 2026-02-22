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
        
    public SearchState(AIStateMachine controller, NavMeshAgent agent, Vector3 searchPosition) : base(controller, agent)
    {
        this.searchPosition = GetRandomPointOnNavMesh(searchPosition, 5f);
        agent.isStopped = true;
        controller.detection.isSearching = true;
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
            Debug.Log("ReturningToStart");
            controller.ReturnToStartingState();
        }
    }

    private Vector3 GetRandomPointOnNavMesh(Vector3 position, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius + position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return position;
    }
}

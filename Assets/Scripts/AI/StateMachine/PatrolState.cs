using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : AIState
{
    private List<Transform> waypoints;
    private int currentWaypoint;
    private CommanderController commanderController;

    public PatrolState(AIStateMachine controller, NavMeshAgent agent, List<Transform> waypoints, int currentWaypoint) : base(controller, agent)
    {
        this.waypoints = waypoints;
        this.currentWaypoint = currentWaypoint;
        commanderController = controller.GetComponent<CommanderController>();
        controller.currentWaypoint = currentWaypoint;
        agent.isStopped = false;
    }

    // Sets commander AI to move toward each waypoint and then loop back to start once finished
    public override void Execute()
    {
        // only moves when all the followers and commander are together initially
        if (!commanderController.hasGroupedUp)
        {
            if (!commanderController.AllFollowersInFormation())
            {
                agent.isStopped = true;
                return;
            }
            else
            {
                commanderController.hasGroupedUp = true;
                agent.isStopped = false;
            }
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.SetDestination(waypoints[currentWaypoint].position);
            controller.currentWaypoint = currentWaypoint;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }
    }
}

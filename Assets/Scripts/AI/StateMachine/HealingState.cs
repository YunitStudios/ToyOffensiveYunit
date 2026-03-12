using UnityEngine;
using UnityEngine.AI;

public class HealingState : AIState
{
    private CoverPoint healCover;
    private bool healingCompleted;
    private float healDuration = 3.0f;
    private float healTimer;

    public HealingState(AIStateMachine controller, NavMeshAgent agent) : base(controller, agent)
    {
        healingCompleted = false;
        healCover = CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, controller.vision.player, controller);
    }

    public override void Execute()
    {
        if (!controller.HoldMedkit || healingCompleted)
        {
            agent.isStopped = false;
            controller.ReturnToStartingState();
            controller.GetComponent<AIController>().SetCrouching(false);
            controller.inCover = true;
            return;
        }
        
        agent.SetDestination(healCover.transform.position);
        if (!agent.pathPending && agent.remainingDistance <= 1.0f)
        {
            agent.isStopped = true;
            controller.GetComponent<AIController>().SetCrouching(true);
            controller.inCover = true;
            Heal();
        }
    }
    
    private void Heal()
    {
        healTimer += Time.deltaTime;
        if (healTimer < healDuration)
        {
            return;
        }
        
        Health health = controller.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(controller.HealAmount);
        } 
        healingCompleted = true;
        controller.SetMedkit(false);
    }
}

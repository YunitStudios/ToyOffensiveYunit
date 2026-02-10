using UnityEngine;
using UnityEngine.AI;

public class AssistState : AIState
{
    private AIStateMachine target;
    private CoverPoint assistCover;
    private bool healingCompleted;
    private float healDuration = 3.0f;
    private float healTimer;
    private float healDistance = 2.0f;
    private AIWeaponSystem weaponSystem;
    
    public AssistState(AIStateMachine controller, NavMeshAgent agent, AIStateMachine target) : base(controller, agent)
    {
        healingCompleted = false;
        this.target = target;
        target.isBeingAssisted = true;
        CoverPoint assistCover = CoverPointManager.instance.GetNearestCoverPoint(target.transform.position, controller.vision.player);
        target.ChangeState(new MoveToAssistCoverState(target, target.agent, assistCover));
        
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
        weaponSystem.ResetFireTimers();
    }

    public override void Execute()
    {
        if (target == null)
        {
            controller.GetComponent<AIController>().SetCrouching(false);
            controller.ReturnToStartingState();
            return;
        }

        if (!target.inCover)
        {
            return;
        }
        
        if (healingCompleted)
        {
            Debug.Log("HealingDone");
            target.ChangeState(new FleeState(target, target.agent, controller.vision.player));
            controller.GetComponent<AIController>().SetCrouching(false);
            controller.ReturnToStartingState();
            return;
        }
        
        if (!agent.pathPending)
        {
            agent.SetDestination(target.transform.position);
        }

        if (!agent.pathPending && agent.remainingDistance <= healDistance)
        {
            agent.isStopped = true;
            controller.GetComponent<AIController>().SetCrouching(true);
            HealTarget();
        }
    }

    private void HealTarget()
    {
        healTimer += Time.deltaTime;
        if (healTimer < healDuration)
        {
            return;
        }
        
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(50f);

            if (target != null)
            {
                Debug.Log("assistDun");
                target.isBeingAssisted = false;
            }
        } 
        healingCompleted = true; 
    }
    
}

using UnityEngine;
using UnityEngine.AI;

public class BehindCoverState : AIState
{
    private CoverPoint coverPoint;
    private Transform player;
    private float peekTimer;
    // time the AI stays behind cover 
    private float peekDelay = 3f;
    private AIController aiController;

    public BehindCoverState(AIStateMachine controller, NavMeshAgent agent, CoverPoint coverPoint, Transform player) : base(controller, agent)
    {
        this.coverPoint = coverPoint;
        this.player = player;
        aiController = controller.GetComponent<AIController>();
        agent.SetDestination(coverPoint.transform.position);
    }

    public override void Execute()
    {
        if (!coverPoint.IsStandingCover)
        {
            aiController.SetCrouching(true);
        }
        Vector3 lookDirection = (player.position - controller.transform.position).normalized;
        lookDirection.y = 0;
        controller.transform.rotation = Quaternion.LookRotation(lookDirection);
        peekTimer += Time.deltaTime;

        // after time has passed switch to peek state
        if (peekTimer >= peekDelay)
        {
            controller.ChangeState(new PeekShootState(controller, agent, coverPoint, player));
        }
    }
}

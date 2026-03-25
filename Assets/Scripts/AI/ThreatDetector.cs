using UnityEngine;

public class ThreatDetector : MonoBehaviour
{
    [SerializeField] private AIStateMachine stateMachine;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ThrowableTemplate grenade))
        {
            if (grenade.Damage <= 0f)
            {
                return;
            }

            if (!(stateMachine.CurrentState is EvadeState))
            {
                stateMachine.ThreatFound(grenade);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ThrowableTemplate grenade))
        {
            if (grenade == null)
            {
                stateMachine.LostThreat();
                return;
            }
            
            if (grenade.Damage <= 0f)
            {
                return;
            }
        }

        if (stateMachine.CurrentState is EvadeState)
        {
            stateMachine.LostThreat();
        }
    }
}

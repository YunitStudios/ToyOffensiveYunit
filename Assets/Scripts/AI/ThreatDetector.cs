using UnityEngine;

public class ThreatDetector : MonoBehaviour
{
    [SerializeField] private AIStateMachine stateMachine;
    
    private void OnTriggerEnter(Collider other)
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
}

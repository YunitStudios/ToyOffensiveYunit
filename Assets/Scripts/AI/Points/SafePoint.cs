using UnityEngine;

public class SafePoint : MonoBehaviour
{
    public bool isCompromised = false;
    [Tooltip("The radius of the safe point")]
    [SerializeField] private float radius = 5f;
    public float Radius => radius;
    private Transform player;
    
    void Start()
    {
        // registers safe point in the manager
        SafePointManager.instance.AddCoverPoint(this);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // if already compromised dont check again
        if (isCompromised)
        {
            return;
        }
        // checks for player inside safePoint
        CheckForPlayer();
        // checks for grenades inside safePoint
        CheckForGrenades();
    }

    private void CheckForPlayer()
    {
        // if player doesnt exist, return
        if (player == null)
        {
            return;
        }
        // only checks if player is inside safePoint
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > radius)
        {
            return;
        }
        // Goes through guards and targets
        foreach (AIStateMachine ai in AIStateMachine.TargetsAndGuards)
        {
            if (ai.vision != null && ai.vision.canSeePlayer)
            {
                // will be compromised if player seen
                isCompromised = true;
                return;
            }
        }
    }

    private void CheckForGrenades()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            ThrowableTemplate grenade = hit.GetComponent<ThrowableTemplate>();
            if (grenade == null)
            {
                continue;
            }
            // Goes through guards and targets
            foreach (AIStateMachine ai in AIStateMachine.TargetsAndGuards)
            {
                if (ai.vision != null && ai.vision.CanSeeTarget(grenade.transform))
                {
                    // will be compromised if target seen
                    isCompromised = true;
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (SafePointManager.instance != null)
        {
            SafePointManager.instance.RemoveCoverPoint(this);
        }
    }
    
    // Cover points shown in scene, for easy placement
    void OnDrawGizmos()
    {
        if (!isCompromised)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.brown;
        }
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

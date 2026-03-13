using UnityEngine;

public class AIVision : MonoBehaviour
{
    [Tooltip("Distance the enemy can See")]
    [SerializeField] private float range;

    [Tooltip("Distance the player can See when engaged with player")]
    [SerializeField] private float engagedRange;
    
    [Tooltip("Enemies field of view")]
    [SerializeField] private float FOV;
    
    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Transform player;
    private SphereCollider visionCollider;

    [Tooltip("Everything the enemy cant see through")]
    [SerializeField] private LayerMask visionMask;
    
    [HideInInspector] public Vector3 lastSeenPosition;

    [HideInInspector] public float lastSeenTime;

    [Tooltip("Time before switching to search state")]
    [SerializeField] private float searchTimeout = 5f;
    public float SearchTimeout => searchTimeout;

    private bool playerInsideVision = false;
    [Tooltip("Detection drop per second, when out of sight")]
    //[SerializeField] private float detectionDrop = 10f;
    
    private bool hasAlertedSquad = false;
    private AIStateMachine aiStateMachine;

    private float defaultRange;

    void Awake()
    {
        visionCollider = GetComponent<SphereCollider>();
        visionCollider.radius = range;
        defaultRange = range;
        // Finds the player object using Tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Will activate when player is inside the trigger collider
    void OnTriggerStay(Collider other)
    {
        // if the object inside trigger is not player then end early
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        // Calculates direction to the player
        Vector3 direction = (player.position - transform.position).normalized;
        // Which is then used to calculate the angle between the AI forwards and the players direction
        float angle = Vector3.Angle(transform.forward, direction);
        // If the angle calculated is larger than half the FOV, then the player is outside of its vision cone
        if (angle > FOV * 0.5f)
        {
            canSeePlayer = false;
            return;
        }
        // Does raycast to check if anything is blocking vision between enemy and player
        if (Physics.Raycast(transform.position + Vector3.up * 1.8f, direction, out RaycastHit hit,  range,  visionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                lastSeenPosition = player.position;
                lastSeenTime = Time.time;
                return;
            }
        }
        canSeePlayer = false;
    }

    // Will activate when player Leaves the trigger collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSeePlayer = false;
        }
    }

    public void IncreaseVision()
    {
        range = engagedRange;
        visionCollider.radius = engagedRange;
        visionCollider.enabled = false;
        visionCollider.enabled = true;
    }

    public void ResetVision()
    {
        range = defaultRange;
        visionCollider.radius = defaultRange;
        visionCollider.enabled = false;
        visionCollider.enabled = true;
    }

    public bool CanSeeTarget(Transform target)
    {
        // Calculates direction to the target
        Vector3 direction = (target.position - transform.position).normalized;
        // Which is then used to calculate the angle between the AI forwards and the targets direction
        float angle = Vector3.Angle(transform.forward, direction);
        // If the angle calculated is larger than half the FOV, then the target is outside of its vision cone
        if (angle > FOV * 0.5f)
        {
            return false;
        }
        // Does raycast to check if anything is blocking vision between enemy and target
        if (Physics.Raycast(transform.position + Vector3.up * 1.8f, direction, out RaycastHit hit,  range,  visionMask))
        {
            if (hit.transform == target)
            {
                return true;
            }
        }
        return false;
    }
}

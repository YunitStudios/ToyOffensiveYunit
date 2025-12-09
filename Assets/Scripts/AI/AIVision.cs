using UnityEngine;

public class AIVision : MonoBehaviour
{
    [Tooltip("Distance the enemy can See")]
    [SerializeField] private float Range;
    
    [Tooltip("Enemies field of view")]
    [SerializeField] private float FOV;
    
    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Transform player;
    private SphereCollider visionCollider;

    [Tooltip("Everything the enemy cant see through")]
    [SerializeField] private LayerMask visionMask;
    
    [Tooltip("Time before the player is seen inside vision")]
    [SerializeField] private float aggressionTime = 1.0f;
    private float visibleTimer = 0f;
    
    [HideInInspector] public Vector3 lastSeenPosition;

    [HideInInspector] public float lastSeenTime;

    [Tooltip("Time before switching to search state")]
    [SerializeField] private float searchTimeout = 20f;
    public float SearchTimeout => searchTimeout;

    private bool playerInsideVision = false;
    [Tooltip("Detection drop per second, when out of sight")]
    [SerializeField] private float detectionDrop = 10f;
    
    private bool hasAlertedSquad = false;
    private AIStateMachine aiStateMachine;

    void Start()
    {
        visionCollider = GetComponent<SphereCollider>();
        visionCollider.radius = Range;
        // Finds the player object using Tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // increases timer if inside vision, or decrease timer if out of vision
        if (playerInsideVision)
        {
            visibleTimer += Time.deltaTime;
        }
        else
        {
            visibleTimer = Mathf.Clamp(visibleTimer -= Time.deltaTime * detectionDrop, 0, aggressionTime);
        }

        // Alert the enemies squad
        if (canSeePlayer)
        {
            if (!hasAlertedSquad)
            { 
                aiStateMachine = GetComponentInParent<AIStateMachine>();
                aiStateMachine.AlertSquad(player);
                hasAlertedSquad = true;
            }
        }
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
            playerInsideVision = false;
            canSeePlayer = false;
            return;
        }
        // Does raycast to check if anything is blocking vision between enemy and player
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out RaycastHit hit,  Range,  visionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // When player has been inside vision for the aggression time, AI can see the player.
                playerInsideVision = true;
                if (visibleTimer >= aggressionTime)
                {
                    canSeePlayer = true;
                    lastSeenPosition = player.position;
                    lastSeenTime = Time.time;
                }
                return;
            }
        }
        canSeePlayer = false;
        playerInsideVision = false;
    }

    // Will activate when player Leaves the trigger collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideVision = false;
            canSeePlayer = false;
        }
    }
}

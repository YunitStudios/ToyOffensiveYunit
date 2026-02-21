using UnityEngine;

public class AIDetection : MonoBehaviour
{
    [SerializeField] private float visionIncreaseRate = 50f;
    [SerializeField] private float hearingMultiplier = 20f;
    [SerializeField] private float detectionDropPerSecond = 10f;
    [SerializeField] private float engageThreshold = 100f;
    [SerializeField] private float disengageThreshold = 50f;
    [SerializeField] private float detectionPercent = 0f;
    
    private AIVision aiVision;
    private AIStateMachine aiStateMachine;
    private float heardTimer = 0f;
    private float heardTimerDuration = 2f;
    private bool heardRecently = false;
    private bool isDetected = false;
    public bool IsDetected => isDetected;
    
    void Awake()
    {
        aiVision = GetComponentInChildren<AIVision>();
        aiStateMachine = GetComponent<AIStateMachine>();
    }

    void Update()
    {
        VisionDetection();
        HeardRecently();
        DetectionDrop();
        Detected();
        detectionPercent = Mathf.Clamp(detectionPercent, 0f, 100f);
    }

    // Vision
    public void VisionDetection()
    {
        if (!aiVision.canSeePlayer)
        {
            return;
        }
        
        float distance = Vector3.Distance(transform.position, aiVision.player.position);
        float maxRange = aiVision.GetComponent<SphereCollider>().radius;
        float distanceFactor = 1f - (distance / maxRange);
        float increase = distanceFactor * visionIncreaseRate * Time.deltaTime;
        detectionPercent += increase;
    }
    
    // Hearing
    public void HearingDetection(float perceivedLoudness)
    {
        float increase = perceivedLoudness * hearingMultiplier;
        detectionPercent += increase;
        heardRecently = true;
        heardTimer = heardTimerDuration;
    }

    // Hearing memory, stops detection dropping instantly
    public void HeardRecently()
    {
        if (!heardRecently)
        {
            return;
        }
        
        heardTimer -= Time.deltaTime;
        if (heardTimer <= 0f)
        {
            heardRecently = false;
        }
    }

    // Losing Detection
    public void DetectionDrop()
    {
        if (!(aiVision.canSeePlayer || heardRecently || aiStateMachine.CurrentState is SearchState))
        {
            detectionPercent -= detectionDropPerSecond * Time.deltaTime;
        }
    }

    // Detected?
    public void Detected()
    {
        if (detectionPercent >= engageThreshold)
        {
            isDetected = true;
        }

        if (isDetected && detectionPercent < disengageThreshold)
        {
            isDetected = false;
        }
    }

    // Add an amount of detection
    public void AddDetection(float amount)
    {
        detectionPercent += amount;
        detectionPercent = Mathf.Clamp(detectionPercent, 0f, 100f);
    }
}

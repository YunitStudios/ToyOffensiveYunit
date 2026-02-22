using TMPro;
using UnityEngine;

public class AIDetection : MonoBehaviour
{
    [SerializeField] private float visionIncreaseRate = 50f;
    [SerializeField] private float hearingMultiplier = 20f;
    [SerializeField] private float detectionDropPerSecond = 10f;
    [SerializeField] private float engageThreshold = 100f;
    [SerializeField] private float disengageThreshold = 50f;
    [SerializeField] private float detectionPercent = 0f;
    public float DetectionPercent => detectionPercent;
    [SerializeField] private float investigateThreshold = 50f;
    public float InvestigateThreshold => investigateThreshold;
    
    private AIVision aiVision;
    private AIStateMachine aiStateMachine;
    private float heardTimer = 0f;
    private float heardTimerDuration = 2f;
    private bool heardRecently = false;
    private bool isDetected = false;
    public bool IsDetected => isDetected;
    private Vector3 lastKnownPosition;
    public Vector3 LastKnownPosition => lastKnownPosition;
    private bool shouldInvestigate = false;
    public bool ShouldInvestigate => shouldInvestigate;
    [HideInInspector] public bool isSearching = false;
    
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private float alertDisplayTime = 1.5f;

    
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
        if (detectionPercent >= investigateThreshold && detectionPercent < engageThreshold && !isDetected)
        {
            shouldInvestigate = true;
        }
        else
        {
            shouldInvestigate = false;
        }
        
        UpdateAlertText();
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
        lastKnownPosition = aiVision.player.position;
    }
    
    // Hearing
    public void HearingDetection(float perceivedLoudness, Vector3 soundPosition)
    {
        float increase = perceivedLoudness * hearingMultiplier;
        detectionPercent += increase;
        heardRecently = true;
        heardTimer = heardTimerDuration;
        lastKnownPosition = soundPosition;
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
        if (!(aiVision.canSeePlayer || heardRecently || isSearching))
        {
            detectionPercent -= detectionDropPerSecond * Time.deltaTime;
        }
    }

    // Detected?
    public void Detected()
    {
        if (detectionPercent >= engageThreshold && aiVision.canSeePlayer)
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

    public void UpdateAlertText()
    {
        if (isDetected)
        {
            alertText.text = "!";
            alertText.color = Color.red;
            
        }
        else if (aiStateMachine.CurrentState is SearchState)
        {
            alertText.text = "?";
            alertText.color = Color.white;
        }
        else
        {
            alertText.text = "";
            alertText.color = Color.white;
        }
    }
}

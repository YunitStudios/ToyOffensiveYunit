using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [Tooltip("Enemies maximum health")]
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    [Tooltip("Amount of health regenerated every second")]
    [SerializeField] private float healthRegen = 5;
    [Tooltip("Time before regenerating health after being attacked")]
    [SerializeField] private float healthRegenDelay = 8;
    [Tooltip("Maximum health that can be regenerated")]
    [SerializeField] private float maxHealthRegenerated = 50;
    private float regenTimerAfterDamage = 0f;
    private float regenTimer = 0f;

    private AIStateMachine stateMachine;
    [SerializeField] private Animator aiAnimator;
    public Animator AIAnimator => aiAnimator;
    private NavMeshAgent navMeshAgent;
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    private CapsuleCollider capCollider;
    private float standHeight = 2f;
    private float crouchHeight = 1f;
    private float targetHeight;
    private Transform playerTransform;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<AIStateMachine>();
        capCollider = GetComponent<CapsuleCollider>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Walking animation based on current speed
        float horizontalSpeed = navMeshAgent.velocity.magnitude;
        // Divide by max speed to get 0-1 range
        horizontalSpeed /= navMeshAgent.speed;
        // Half to fit walking blend tree
        horizontalSpeed /= 2f;
        // If sprinting, remove multiplier and instead double to get full speed
        if (navMeshAgent.speed > 2)
        {
            horizontalSpeed *= 2f;
        }
        aiAnimator.SetFloat(AnimMoveSpeed, horizontalSpeed , 0.2f, Time.deltaTime);
        // After timer trigger health regen
        regenTimerAfterDamage += Time.deltaTime;
        if (regenTimerAfterDamage >= healthRegenDelay)
        {
            regenTimer += Time.deltaTime;
            RegenHealth();
        }
    }
    
    // Enemy take damage function, and resets timer for regen
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        regenTimerAfterDamage = 0;

        // Alert squad when damaged 
        stateMachine.AlertSquad(playerTransform);
        
        if (currentHealth <= 0f)
        {
            stateMachine.Die();
        }
    }

    private void RegenHealth()
    {
        // end early if healing is not needed
        if (currentHealth >= maxHealthRegenerated)
        {
            return;
        }
        // regen health every second
        if (regenTimer >= 1)
        {
            currentHealth += healthRegen;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealthRegenerated);
            regenTimer = 0;
        }
    }

    // Sets crouching/ standing animation and stats
    public void SetCrouching(bool isCrouching)
    {
        if (isCrouching)
        {
            aiAnimator.SetBool(IsCrouching, true);
            targetHeight = crouchHeight;
            Vector3 center = capCollider.center;
            center.y = targetHeight / 2f;
            capCollider.center = -center;
        }
        else
        {
            aiAnimator.SetBool(IsCrouching, false);
            targetHeight =  standHeight;
            Vector3 center = capCollider.center;
            center.y = 0;
            capCollider.center = center;
        }
        capCollider.height = targetHeight;
        navMeshAgent.height = targetHeight;
    }
}

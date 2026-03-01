using UnityEngine;

public class GloryKill : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float maxDistance = 1f;
    [SerializeField] private float angleBehind = 80f;
    [SerializeField] private float cooldown = 3f;

    [Header("References")] 
    [SerializeField] private Animator playerAnimator;

    private AIStateMachine currentTarget;
    
    private LayerMask enemyLayerMask;
    private float lastGloryTime = 0f;
    private Animator targetAnimator;
    private GloryKillPromptUI gloryKillPromptUI;
    private bool isGloryKilling = false;
    
    
    void Awake()
    {
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    void Start()
    {
        gloryKillPromptUI = FindObjectOfType<GloryKillPromptUI>(true);
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnGloryKillAction += OnGloryKillTriggered;
        }
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnGloryKillAction -= OnGloryKillTriggered;
        }
    }

    void Update()
    {
        if (isGloryKilling)
        {
            return;
        }
        DetectGloryTarget();
        gloryKillPromptUI.SetGloryKillPrompt(currentTarget != null);
    }

    void DetectGloryTarget()
    {
        currentTarget = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance, enemyLayerMask);
        foreach (Collider hit in colliders)
        {
            AIStateMachine aiStateMachine = hit.GetComponent<AIStateMachine>();
            if (aiStateMachine == null)
            {
                continue;
            }

            if (aiStateMachine.detection.IsDetected)
            {
                continue;
            }
            
            Vector3 direction = (aiStateMachine.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(aiStateMachine.transform.forward, direction);
            if (angle < angleBehind)
            {
                currentTarget = aiStateMachine;
                targetAnimator = currentTarget.GetComponent<Animator>();
                return;
            }
        }
    }

    void OnGloryKillTriggered()
    {
        if (currentTarget == null)
        {
            return;
        }

        if (Time.time < lastGloryTime + cooldown)
        {
            return;
        }

        TriggerGloryKill();
    }

    void TriggerGloryKill()
    {
        isGloryKilling = true;
        lastGloryTime = Time.time;
        AIStateMachine.OnFreezeAllAI?.Invoke(true);
        gloryKillPromptUI.SetGloryKillPrompt(false);
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        WeaponsSystem weaponsSystem = GetComponentInChildren<WeaponsSystem>();
        playerMovement.SetMovementFrozen(true);
        weaponsSystem.SetWeaponFrozen(true);
        playerAnimator.SetTrigger("GloryKill1");
        targetAnimator.SetTrigger("GloryKill2");
       
       // play animation on the target ai and the player, freeze player movement somewhere too, maybe change camera for the glory kill
    }

    // Will be called by end of glory kill animation
    public void OnGloryKillFinished()
    {
        AIStateMachine.OnFreezeAllAI?.Invoke(false);
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        WeaponsSystem weaponsSystem = GetComponentInChildren<WeaponsSystem>();
        playerMovement.SetMovementFrozen(false);
        weaponsSystem.SetWeaponFrozen(false);
        currentTarget.health.DealDamage(100f);
        currentTarget = null;
        isGloryKilling = false;
    }
}

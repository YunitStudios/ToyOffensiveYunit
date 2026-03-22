using System.Collections;
using UnityEngine;

public class GloryKill : MonoBehaviour, IDamageSource
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
    private Transform snapPoint;
    private AIController currentTargetController;
    [SerializeField] private GameObject gunMesh;
    [SerializeField] private GameObject gloryKillCamera; 
    private CanvasGroup hud;
    
    
    void Awake()
    {
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    void Start()
    {
        gloryKillPromptUI = FindObjectOfType<GloryKillPromptUI>(true);
        hud = GameObject.Find("HUD").GetComponent<CanvasGroup>();
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
                currentTargetController =currentTarget.GetComponent<AIController>(); 
                targetAnimator = currentTarget.GetComponentInChildren<Animator>();
                snapPoint = currentTarget.transform.Find("GloryKillPoint");
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
        playerAnimator.SetBool("IsAiming", false);
        DisableTargetColliders(true);
        gunMesh.SetActive(false);
        gloryKillCamera.SetActive(true);
        hud.alpha = 0.0f;
        isGloryKilling = true;
        lastGloryTime = Time.time;
        AIStateMachine.OnFreezeAllAI?.Invoke(true);
        gloryKillPromptUI.SetGloryKillPrompt(false);
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        WeaponsSystem weaponsSystem = GetComponentInChildren<WeaponsSystem>();
        playerMovement.SetMovementFrozen(true);
        weaponsSystem.SetWeaponFrozen(true);
        Vector3 snapPosition = snapPoint.position;
        snapPosition.y = transform.position.y;
        //playerMovement.SetPosition(snapPosition);
        StartCoroutine(MovePlayerToPoint(snapPosition, () =>
        {
            playerAnimator.applyRootMotion = true; 
            playerMovement.SetRotation(snapPoint.rotation);
            playerAnimator.CrossFade("GloryKill1", 0f);
            targetAnimator.CrossFade("GloryKill2", 0f);
        }));
        //playerMovement.SetRotation(snapPoint.rotation);
        //playerAnimator.applyRootMotion = true;
        //playerAnimator.CrossFade("GloryKill1", 0f);
        //targetAnimator.CrossFade("GloryKill2", 0f);
       
       // play animation on the target ai and the player, freeze player movement somewhere too, maybe change camera for the glory kill
    }

    // Will be called by end of glory kill animation
    public void OnGloryKillFinished()
    {
        DisableTargetColliders(false);
        AIStateMachine.OnFreezeAllAI?.Invoke(false);
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        WeaponsSystem weaponsSystem = GetComponentInChildren<WeaponsSystem>();
        playerMovement.SetMovementFrozen(false);
        weaponsSystem.SetWeaponFrozen(false);
        playerAnimator.applyRootMotion = false;
        damageSourcePos = currentTargetController.transform.position;
        currentTargetController.TakeDamage(this, 100f);
        currentTarget = null;
        isGloryKilling = false;
        gunMesh.SetActive(true);
        gloryKillCamera.SetActive(false);
        hud.alpha = 1.0f;
    }

    private IEnumerator MovePlayerToPoint(Vector3 targetPosition, System.Action onComplete)
    {
        playerAnimator.SetFloat("MoveSpeed", 1f);
        float duration = 0.5f;
        float timeElapsed = 0f;
        Vector3 startPosition = transform.position;
        targetPosition.y = startPosition.y;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        transform.position = targetPosition;
        playerAnimator.SetFloat("MoveSpeed", 0f);
        onComplete?.Invoke();
    }

    private void DisableTargetColliders(bool disable)
    {
        Collider[] colliders = currentTarget.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = !disable;
        }
    }

    public Vector3 damageSourcePos { get; set; }
}

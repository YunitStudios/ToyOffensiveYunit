using UnityEngine;

public class MouseTrap : MonoBehaviour
{
    [Header("Launch Forces")]
    [SerializeField] private float yAxisLaunchForce;
    [SerializeField] private float xAxisLaunchForce;
    [SerializeField] private float zAxisLaunchForce;

    [SerializeField] private Animator animator;
    private string Activate = "Activate";
    private string animProgress = "Progress";
    [SerializeField] private bool canRecharge = true;
    private bool isRecharging = false;
    [SerializeField] private float timeToRecharge = 5f;
    private float rechargeSpeed;
    private float rechargeTimer;
    private float animationTimer;
    private bool activated = false;
    private float progress;
    private float activationTime = 0.15f;

    void Start()
    {
        animator.Play(Activate,0,0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isRecharging)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            Vector3 launchVelocity = xAxisLaunchForce * transform.right + yAxisLaunchForce * transform.up + zAxisLaunchForce * transform.forward;
            player.ImpulseVelocity(launchVelocity);

            ActivateTrap();
        }
    }

    private void ActivateTrap()
    {
        progress = 0f;
        activated = true;
    }

    public void StartRecharging()
    {
        if (!canRecharge)
        {
            return;
        }
        
        rechargeTimer = timeToRecharge;
        isRecharging = true;
    }

    private void Update()
    {
        if (activated)
        {
            progress += Time.deltaTime / activationTime;
            progress = Mathf.Clamp01(progress);
            animator.SetFloat(animProgress, progress);
            if (progress >= 1f)
            {
                activated = false;
                StartRecharging();
            }

            return;
        }

        if (!isRecharging)
        {
            return;
        }
        rechargeTimer -= Time.deltaTime;
        progress = Mathf.Clamp01(rechargeTimer / timeToRecharge);
        animator.SetFloat(animProgress, progress);

        if (rechargeTimer <= 0f)
        {
            isRecharging = false;
            progress = 0f;
            animator.SetFloat(animProgress, progress);
        }
    }
}

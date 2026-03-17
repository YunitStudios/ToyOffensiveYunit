using UnityEngine;

public class MouseTrap : MonoBehaviour
{
    [SerializeField] private float yAxisLaunchForce;
    [SerializeField] private float xAxisLaunchForce;
    [SerializeField] private float zAxisLaunchForce;

    [SerializeField] private Animator animator;
    private string Activate = "Activate";
    [SerializeField] private bool canRecharge = true;
    private bool isRecharging = false;
    [SerializeField] private float timeToRecharge = 5f;
    private float rechargeSpeed;
    private float rechargeTimer;
    private float animationTimer;
    private bool activated = false;

    private void Start()
    {
        //animator.runtimeAnimatorController.
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
            Vector3 launchVelocity = new Vector3(xAxisLaunchForce, yAxisLaunchForce, zAxisLaunchForce);
            player.ImpulseVelocity(launchVelocity);

            animator.speed = 1f;
            animator.Play(Activate, 0, 0);
            activated = true;
            animationTimer = 1f;
        }
    }

    public void StartRecharging()
    {
        if (!canRecharge)
        {
            return;
        }

        rechargeSpeed = 1 / timeToRecharge;
        animator.speed = -rechargeSpeed;
        animator.Play(Activate, 0, 1);
        rechargeTimer = timeToRecharge;
        isRecharging = true;
    }

    private void Update()
    {
        if (activated)
        {
            animationTimer -= Time.deltaTime;
            if (animationTimer <= 0f)
            {
                activated = false;
                StartRecharging();
            }
        }

        if (!isRecharging)
        {
            return;
        }
        rechargeTimer -= Time.deltaTime;

        if (rechargeTimer <= 0f)
        {
            isRecharging = false;
        }
    }
}

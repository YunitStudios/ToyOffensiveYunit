using UnityEngine;

public class MouseTrap : MonoBehaviour
{
    [SerializeField] private float yAxisLaunchForce;
    [SerializeField] private float xAxisLaunchForce;
    [SerializeField] private float zAxisLaunchForce;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            Vector3 launchVelocity = new Vector3(xAxisLaunchForce, yAxisLaunchForce, zAxisLaunchForce);
            player.AddVelocity(launchVelocity);
        }
    }
}

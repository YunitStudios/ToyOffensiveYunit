using UnityEngine;

public class Waypoint : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.lightGoldenRodYellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

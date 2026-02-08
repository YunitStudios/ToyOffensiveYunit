using UnityEngine;

public class SafePoint : MonoBehaviour
{
    public bool isCompromised = false;
    [Tooltip("The radius of the safe point")]
    [SerializeField] private float radius = 5f;
    public float Radius => radius;
    
    void Start()
    {
        SafePointManager.instance.AddCoverPoint(this);
    }

    private void OnDestroy()
    {
        if (SafePointManager.instance != null)
        {
            SafePointManager.instance.RemoveCoverPoint(this);
        }
    }
    
    // Cover points shown in scene, for easy placement
    void OnDrawGizmos()
    {
        if (!isCompromised)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.brown;
        }
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

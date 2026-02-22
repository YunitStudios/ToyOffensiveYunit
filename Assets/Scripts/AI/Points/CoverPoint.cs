using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    // is cover point taken?
    [HideInInspector] public bool isTaken = false;
    [HideInInspector] public AIStateMachine aiStateMachine = null;
    [Tooltip("Set to true if enemy stands up when hiding behind cover, otherwise will crouch")]
    [SerializeField] private bool standingCover = true;
    public bool IsStandingCover => standingCover;

    void Start()
    {
        CoverPointManager.instance.AddCoverPoint(this);
    }

    private void OnDestroy()
    {
        if (CoverPointManager.instance != null)
        {
            CoverPointManager.instance.RemoveCoverPoint(this);
        }
    }
    
    // claims cover for an enemy
    public bool TakeCoverPoint(AIStateMachine ai)
    {
        if (isTaken)
        {
            return false;
        }
        isTaken = true;
        aiStateMachine = ai;
        return true;
    }

    public void LeaveCoverPoint()
    {
        isTaken = false;
        aiStateMachine = null;
    }

    // Cover points shown in scene, for easy placement
    void OnDrawGizmos()
    {
        if (standingCover)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.orangeRed;
        }
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

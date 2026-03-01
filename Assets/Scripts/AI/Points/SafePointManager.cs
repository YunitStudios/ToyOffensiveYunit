using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SafePointManager : MonoBehaviour
{
    public static SafePointManager instance;
    private List<SafePoint> safePoints = new List<SafePoint>();
    
    void Awake()
    {
        instance = this;
    }

    public void AddCoverPoint(SafePoint safePoint)
    {
        safePoints.Add(safePoint);
    }

    public void RemoveCoverPoint(SafePoint safePoint)
    {
        safePoints.Remove(safePoint);
    }
    
    // finds closest safe point from the enemy
    public SafePoint GetNearestSafePoint(Vector3 fromPosition)
    {
        SafePoint best = null;
        float bestDistance = Mathf.Infinity;
        NavMeshPath path = new NavMeshPath();

        foreach (SafePoint safePoint in safePoints)
        {
            if (safePoint.isCompromised)
            {
                continue;
            }
            
            float distance = Vector3.Distance(fromPosition, safePoint.transform.position);
            if (distance < bestDistance)
            {
                // checks the safe point is reachable
                if (NavMesh.CalculatePath(fromPosition, safePoint.transform.position, NavMesh.AllAreas, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    bestDistance = distance;
                    best = safePoint;
                }
            }
        }
        return best;
    }
}

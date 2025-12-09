using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CoverPointManager : MonoBehaviour
{
    public static CoverPointManager instance;
    private List<CoverPoint> coverPoints = new List<CoverPoint>();
    [SerializeField] private LayerMask ignoreLayers;

    void Awake()
    {
        instance = this;
    }

    public void AddCoverPoint(CoverPoint coverPoint)
    {
        coverPoints.Add(coverPoint);
    }

    public void RemoveCoverPoint(CoverPoint coverPoint)
    {
        coverPoints.Remove(coverPoint);
    }

    // finds closest cover point from the enemy
    public CoverPoint GetNearestCoverPoint(Vector3 fromPosition, Transform player)
    {
        CoverPoint best = null;
        float bestDistance = Mathf.Infinity;
        NavMeshPath path = new NavMeshPath();

        foreach (CoverPoint coverPoint in coverPoints)
        {
            // skips already taken cover points
            if (coverPoint.isTaken)
            {
                continue;
            }

            // skips cover points that don't block line of sight from player
            if (!HasCoverFrom(player, coverPoint.transform.position))
            {
                continue;
            }
            
            float distance = Vector3.Distance(fromPosition, coverPoint.transform.position);
            if (distance < bestDistance)
            {
                // checks the cover point is reachable
                if (NavMesh.CalculatePath(fromPosition, coverPoint.transform.position, NavMesh.AllAreas, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    bestDistance = distance;
                    best = coverPoint;
                }
            }
        }
        return best;
    }

    // checks if the cover point provides cover from the player
    public bool HasCoverFrom(Transform player, Vector3 fromPoint)
    {
        Vector3 from = fromPoint + Vector3.up * 0.5f;
        Vector3 to = player.position + Vector3.up;
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        // raycast from cover point to player, ignoring enemy 
        if (Physics.Raycast(from, direction, out RaycastHit hit, distance,  ~ignoreLayers))
        {
            return hit.transform != player;
        }
        return false;
    }
}

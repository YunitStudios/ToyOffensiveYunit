using System;
using System.Collections.Generic;
using UI.HUD;
using UnityEngine;
using UnityEngine.Serialization;

public class RadialHUDDisplay : MonoBehaviour
{
    [FormerlySerializedAs("markerPrefab")] [SerializeField] private GameObject damageMarkerPrefab;
    [SerializeField] private GameObject targetMarkerPrefab;
    [SerializeField] private GameObject exfilMarkerPrefab;
    [SerializeField] private Transform markerParent;
    [SerializeField] private float markerShowTime;
    private Transform player;

    [Header("Grouping Settings")]
    [SerializeField] private int sectorCount = 36; // 36 sectors = 10 degrees each

    public static Action<Vector3, MarkerTypes> SpawnMarker;
    public static Action<Transform, MarkerTypes> SpawnLiveMarker;

    // Tracks active markers by their sector index
    private readonly Dictionary<int, DirectionalMarker> activeMarkers = new();

    private void OnEnable()
    {
        SpawnMarker -= SpawnMarkerUI;
        SpawnMarker += SpawnMarkerUI;
        SpawnLiveMarker -= SpawnLiveMarkerUI;
        SpawnLiveMarker += SpawnLiveMarkerUI;
    }

    private void OnDisable()
    {
        SpawnMarker -= SpawnMarkerUI;
        SpawnLiveMarker -= SpawnLiveMarkerUI;
    }

    void SpawnMarkerUI(Vector3 targetPosition, MarkerTypes markerType)
    {
        player = GameManager.PlayerData.RotationRootTransform;
        if (player == null) return;

        // calculate angle same as marker
        Vector3 dirToTarget = targetPosition - player.position;
        dirToTarget.y = 0;
        Vector3 forward = player.forward;
        forward.y = 0;
        float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);

        // convert to a 360 degree range for the sectors
        float normalizedAngle = angle;
        if (normalizedAngle < 0) normalizedAngle += 360f;

        // work out what sector it goes in
        float sectorSize = 360f / sectorCount;
        int sectorIndex = Mathf.FloorToInt(normalizedAngle / sectorSize);

        // see if theres already one in the sector
        if (activeMarkers.TryGetValue(sectorIndex, out var existingMarker) && existingMarker != null)
        {
            // update the position
            existingMarker.targetPosition = targetPosition;
            // extend the life
            existingMarker.ResetTimer(markerShowTime);
            return;
        }

        // if no marker make one
        GameObject markerToSpawn;
        bool shouldExpire = true;
        switch (markerType)
        {
            case MarkerTypes.Damage:
                markerToSpawn = damageMarkerPrefab;
                break;
            case MarkerTypes.Target:
                markerToSpawn = targetMarkerPrefab;
                shouldExpire = false;
                break;
            case MarkerTypes.Exfil:
                markerToSpawn = exfilMarkerPrefab;
                shouldExpire = false;
                break;
            default:
                markerToSpawn = damageMarkerPrefab;
                break;
        }
        
        GameObject marker = Instantiate(markerToSpawn, markerParent);
        DirectionalMarker markerComponent = marker.GetComponent<DirectionalMarker>();

        markerComponent.player = player;
        markerComponent.targetPosition = targetPosition;
        markerComponent.showTime = markerShowTime;
        markerComponent.expires = shouldExpire;
        markerComponent.type = markerType;

        // store it in the dict
        activeMarkers[sectorIndex] = markerComponent;

        // remove from dict when marker is destroyed
        markerComponent.OnExpired += () => {
            if (activeMarkers.ContainsKey(sectorIndex))
            {
                activeMarkers.Remove(sectorIndex);
            }
        };
    }

    void SpawnLiveMarkerUI(Transform targetTransform, MarkerTypes markerType)
    {
        player = GameManager.PlayerData.RotationRootTransform;
        if (player == null || targetTransform == null) return;

        // if no marker make one
        GameObject markerToSpawn;
        bool shouldExpire = true;
        switch (markerType)
        {
            case MarkerTypes.Damage:
                markerToSpawn = damageMarkerPrefab;
                break;
            case MarkerTypes.Target:
                markerToSpawn = targetMarkerPrefab;
                shouldExpire = false;
                break;
            case MarkerTypes.Exfil:
                markerToSpawn = exfilMarkerPrefab;
                shouldExpire = false;
                break;
            default:
                markerToSpawn = damageMarkerPrefab;
                break;
        }

        GameObject marker = Instantiate(markerToSpawn, markerParent);
        DirectionalMarker markerComponent = marker.GetComponent<DirectionalMarker>();

        markerComponent.player = player;
        markerComponent.targetTransform = targetTransform;
        markerComponent.isLive = true;
        markerComponent.showTime = markerShowTime;
        markerComponent.expires = shouldExpire;
        markerComponent.type = markerType;
    }
}

public enum MarkerTypes
{
    Damage,
    Target,
    Exfil
}

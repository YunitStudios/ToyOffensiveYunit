using UnityEngine;
using System;
using System.Collections.Generic;

namespace UI.HUD
{
    public class DirectionalDamageUI : MonoBehaviour
    {
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private Transform markerParent;
        [SerializeField] private float markerShowTime;
        private Transform player;

        [Header("Grouping Settings")]
        [SerializeField] private int sectorCount = 36; // 36 sectors = 10 degrees each

        public static Action<Vector3> SpawnMarker;

        // Tracks active markers by their sector index
        private readonly Dictionary<int, DirectionalDamageMarker> activeMarkers = new();

        private void OnEnable()
        {
            SpawnMarker -= SpawnMarkerUI;
            SpawnMarker += SpawnMarkerUI;
        }

        private void OnDisable()
        {
            SpawnMarker -= SpawnMarkerUI;
        }

        void SpawnMarkerUI(Vector3 attackerPosition)
        {
            player = GameManager.PlayerData.RotationRootTransform;
            if (player == null) return;

            // 1. Calculate the angle exactly like your Marker does
            Vector3 dirToTarget = attackerPosition - player.position;
            dirToTarget.y = 0;
            Vector3 forward = player.forward;
            forward.y = 0;

            // Get angle (-180 to 180)
            float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);

            // 2. Convert to 0-360 for mathematical sectoring
            float normalizedAngle = angle;
            if (normalizedAngle < 0) normalizedAngle += 360f;

            // 3. Determine which sector this hit falls into
            float sectorSize = 360f / sectorCount;
            int sectorIndex = Mathf.FloorToInt(normalizedAngle / sectorSize);

            // 4. Grouping Logic: Check if a marker is already active in this sector
            if (activeMarkers.TryGetValue(sectorIndex, out var existingMarker) && existingMarker != null)
            {
                // Update the existing marker's position (in case the enemy moved)
                existingMarker.targetPosition = attackerPosition;
                // Extend the life of the existing marker
                existingMarker.ResetTimer(markerShowTime);
                return;
            }

            // 5. If no existing marker, spawn a new one
            GameObject marker = Instantiate(markerPrefab, markerParent);
            DirectionalDamageMarker markerComponent = marker.GetComponent<DirectionalDamageMarker>();

            markerComponent.player = player;
            markerComponent.targetPosition = attackerPosition;
            markerComponent.showTime = markerShowTime;

            // Store it in the dictionary
            activeMarkers[sectorIndex] = markerComponent;

            // Important: Remove from dictionary when the marker is destroyed
            markerComponent.OnExpired += () => {
                if (activeMarkers.ContainsKey(sectorIndex))
                {
                    activeMarkers.Remove(sectorIndex);
                }
            };
        }
    }
}
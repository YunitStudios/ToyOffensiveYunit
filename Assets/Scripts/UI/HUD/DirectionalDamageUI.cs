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

            // calculate angle same as marker
            Vector3 dirToTarget = attackerPosition - player.position;
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
                existingMarker.targetPosition = attackerPosition;
                // extend the life
                existingMarker.ResetTimer(markerShowTime);
                return;
            }

            // if no marker make one
            GameObject marker = Instantiate(markerPrefab, markerParent);
            DirectionalDamageMarker markerComponent = marker.GetComponent<DirectionalDamageMarker>();

            markerComponent.player = player;
            markerComponent.targetPosition = attackerPosition;
            markerComponent.showTime = markerShowTime;

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
    }
}
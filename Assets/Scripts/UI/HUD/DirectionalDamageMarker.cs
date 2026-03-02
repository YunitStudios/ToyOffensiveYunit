using System;
using UnityEngine;

namespace UI.HUD
{
    public class DirectionalDamageMarker : MonoBehaviour
    {
        public float showTime;

        public Vector3 targetPosition;
        public Transform player;

        public Action OnExpired;

        float timer;

        void Start()
        {
            // initialise countdown
            timer = showTime;
        }

        void Update()
        {
            if (player == null) return;

            Vector3 dirToTarget = targetPosition - player.position;
            dirToTarget.y = 0;
            Vector3 forward = player.forward; 
            forward.y = 0;

            // horizontal angle between player look and enemy
            float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);

            transform.localRotation = Quaternion.Euler(0f, 0f, -angle);

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                OnExpired?.Invoke();
                Destroy(gameObject);
            }
        }

        public void ResetTimer(float time)
        {
            timer = time;
        }
    }
}
using System;
using UnityEngine;

namespace UI.HUD
{
    public class DirectionalMarker : MonoBehaviour
    {
        public MarkerTypes type;
        public bool expires = true;
        public float showTime;


        public bool isLive = false;
        public Vector3 targetPosition;
        public Transform targetTransform;
        public Transform player;

        public Action OnExpired;

        float timer;
        
        [Header("Only for exfil markers")]
        [SerializeField] private VoidEventChannelSO onMissionCompleted;
        [SerializeField] private GameObject image;

        void Start()
        {
            // initialise countdown
            timer = showTime;
            if (type == MarkerTypes.Exfil)
            {
                onMissionCompleted.OnEventRaised += ShowMarker;
            }
        }

        private void OnDisable()
        {
            if(onMissionCompleted != null)
                onMissionCompleted.OnEventRaised -= ShowMarker;
        }

        void ShowMarker()
        {
            image.SetActive(true);
        }

        void Update()
        {
            if (player == null) return;
            
            if (isLive && targetTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            // use transform position if live, otherwise use the vector
            Vector3 currentTargetPos = (isLive && targetTransform != null) ? targetTransform.position : targetPosition;

            Vector3 dirToTarget = currentTargetPos - player.position;
            dirToTarget.y = 0;
            Vector3 forward = player.forward; 
            forward.y = 0;

            // horizontal angle between player look and enemy
            float angle = Vector3.SignedAngle(forward, dirToTarget, Vector3.up);

            transform.localRotation = Quaternion.Euler(0f, 0f, -angle);

            if (expires)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    OnExpired?.Invoke();
                    Destroy(gameObject);
                }
            }
            
        }

        public void ResetTimer(float time)
        {
            timer = time;
        }
    }
}
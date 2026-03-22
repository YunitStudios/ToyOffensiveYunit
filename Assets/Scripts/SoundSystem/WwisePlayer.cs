using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SoundSystem
{
    public class WwisePlayer : MonoBehaviour
    {
        [SerializeField] private FootData[] feet;
        [SerializeField] private float footSoundThreshold = 0;
        [SerializeField] private SoundDataSO footstep;

        [Serializable]
        private class FootData
        {
            public Transform transform;
            public bool onGround;
        }
        
        public void PlaySound(SoundDataSO SoundData)
        {
            if (SoundData == null || string.IsNullOrEmpty(SoundData.WwiseName))
            {
                Debug.LogWarning("SoundData or WwiseName is not set.");
                return;
            }
            
            // use main listener if 2D, otherwise self-emitting
            // TODO: see if this actually works and is useful
            GameObject target = SoundData.Is2D ? Camera.main.gameObject : this.gameObject;
            
            // TODO: add more types if needed
            switch (SoundData.Type)
            {
                case SoundType.WwiseEvent:
                    AkSoundEngine.PostEvent(SoundData.WwiseName, target);
                    break;
                case SoundType.WwiseTrigger:
                    AkSoundEngine.PostTrigger(SoundData.WwiseName, target);
                    break;
                default:
                    Debug.LogWarning("Unsupported SoundType.");
                    break;
            }
            
            // Debug.Log(SoundData.WwiseName);
        }

        private void Update()
        {
            PlayFootsteps();
        }

        private void PlayFootsteps()
        {
            // Check if any feet are below the Y threshold
            foreach (var foot in feet)
            {

                Vector3 playerPosition = transform.position;
                Vector3 footPosition = foot.transform.position;
                float difference = footPosition.y - playerPosition.y;
                print(difference);
                
                if(foot.onGround && difference >= footSoundThreshold)
                {
                    foot.onGround = false;
                }
                if (!foot.onGround && difference < footSoundThreshold)
                {
                    PlaySound(footstep);
                    foot.onGround = true;
                }

            }
            
        }
    }
}
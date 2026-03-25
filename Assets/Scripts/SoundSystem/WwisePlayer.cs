using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SoundSystem
{
    public class WwisePlayer : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private FootData[] feet;
        [SerializeField] private float footSoundThreshold = 0;
        [SerializeField] private SoundDataSO footstep;
        
        private SpatialAudioCue _spatialAudioCue = new SpatialAudioCue();

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
            
            // check if we should use the spatial cue for AI hearing
            if (SoundData.MaxHearingRadius > 0 && SoundData.BaseLoudness > 0)
            {
                _spatialAudioCue.PlayCue(this, SoundData.MaxHearingRadius, SoundData.BaseLoudness, transform.position);
            }
            
            // this is a bodge but if sounds are over a level of 5 loudness its a gunshot so just pray that never gets set below 5
            if(SoundData.BaseLoudness > 5f)
                AkSoundEngine.SetSwitch("Suppression", "Not_Suppressed", gameObject);
            
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
        }

        public void PlaySupressedSound(SoundDataSO SoundData, float multiplier)
        {
            if (SoundData == null || string.IsNullOrEmpty(SoundData.WwiseName))
            {
                Debug.LogWarning("SoundData or WwiseName is not set.");
                return;
            }
            
            // check if we should use the spatial cue for AI hearing
            if (SoundData.MaxHearingRadius > 0 && SoundData.BaseLoudness > 0)
            {
                if (multiplier != 1f)
                    AkSoundEngine.SetSwitch("Suppression", "Suppressed", gameObject);
                else
                {
                    AkSoundEngine.SetSwitch("Suppression", "Not_Suppressed", gameObject);
                }
                _spatialAudioCue.PlayCue(this, SoundData.MaxHearingRadius * multiplier, SoundData.BaseLoudness * multiplier, transform.position);
                
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
        }
        
        public void ChangeRTPC(SoundDataSO SoundData, float value)
        {
            if (SoundData == null || string.IsNullOrEmpty(SoundData.WwiseName))
            {
                Debug.LogWarning("SoundData or WwiseName is not set.");
                return;
            }
            
            switch (SoundData.Type)
            {
                case SoundType.WwiseRTPC:
                    AkSoundEngine.SetRTPCValue(SoundData.WwiseName, value);
                    break;
                default:
                    Debug.LogWarning("Unsupported SoundType.");
                    break;
            }
        }

        private void Update()
        {
            PlayFootsteps();
        }

        private void PlayFootsteps()
        {
            if (!playerMovement ||  playerMovement.CurrentState is IMovementState { PlayFootsteps: false })
                return;
            
            // Check if any feet are below the Y threshold
            foreach (var foot in feet)
            {

                Vector3 playerPosition = transform.position;
                Vector3 footPosition = foot.transform.position;
                float difference = footPosition.y - playerPosition.y;
                
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
using System;
using UnityEngine;

namespace SoundSystem
{
    public class WwisePlayer : MonoBehaviour
    {
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
            
            Debug.Log(SoundData.WwiseName);
        }
    }
}
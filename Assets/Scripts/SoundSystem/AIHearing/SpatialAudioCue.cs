using UnityEngine;

namespace SoundSystem
{
    public class SpatialAudioCue
    {
        private static Collider[] _overlapBuffer = new Collider[32];
        private static LayerMask _hearingLayerMask;

        public void PlayCue(WwisePlayer player, SoundDataSO SoundData, Vector3 position)
        {
            _hearingLayerMask = LayerMask.GetMask("AI_Hearing");
            
            // first play the sound using the wwise player as you normally would
            player.PlaySound(SoundData);
            
            // get the loudness from the SounDataSO for the max radius
            float radius = SoundData.MaxHearingRadius;
            
            // do the sphere call
            int hitCount = Physics.OverlapSphereNonAlloc(
                position,
                radius,
                _overlapBuffer,
                _hearingLayerMask
            );
            Debug.Log(hitCount);

            // forward to AI in range
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _overlapBuffer[i];
                if (col.TryGetComponent(out SpatialAudioReciever reciever))
                {
                    reciever.OnHeardSound(new SoundStimulus
                    {
                        position = position,
                        radius = radius,
                        baseLoudness = SoundData.BaseLoudness,
                    });
                }
            }
            
        }
    }
}
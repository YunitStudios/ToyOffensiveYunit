using UnityEngine;

namespace SoundSystem
{
    public class SpatialAudioCue
    {
        private static Collider[] _overlapBuffer = new Collider[32];
        private static LayerMask _hearingLayerMask;

        public void PlayCue(WwisePlayer player, float MaxHearingRadius, float BaseLoudness, Vector3 position)
        {
            _hearingLayerMask = LayerMask.GetMask("AI_Hearing");
            
            // get the loudness from the SoundDataSO for the max radius
            
            // do the sphere call
            int hitCount = Physics.OverlapSphereNonAlloc(
                position,
                MaxHearingRadius,
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
                        radius = MaxHearingRadius,
                        baseLoudness = BaseLoudness,
                        source = player.transform.root
                    });
                }
            }
        }
    }
}
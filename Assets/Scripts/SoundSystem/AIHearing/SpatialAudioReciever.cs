using UnityEngine;

public class SpatialAudioReciever : MonoBehaviour
{
    private float obfuscationMultiplier = 1f;
    private static LayerMask hearingLayerMask;

    private void Awake()
    {
        hearingLayerMask = LayerMask.GetMask("Environment");
    }
    
    public void OnHeardSound(SoundStimulus stimulus)
    {
        // ignore self
        if (stimulus.source == transform.root) return;

        // ignore other AI footsteps, but hear shots. This is bodged because shots are all over 5 base loudness and adding a half decent filter is hard
        if (stimulus.baseLoudness <= 5 && stimulus.source.CompareTag("Enemy")) return;

        Vector3 listenerPos = transform.position;
        Vector3 soundPos = stimulus.position;
    
        float distance = Vector3.Distance(listenerPos, soundPos);
    
        // if we are outside the max distance, it is silent
        if (distance > stimulus.radius) return;

        // calculate log falloff: (1 - log(dist)/log(max)) * baseLoudness
        // we use a small offset (1f) to avoid log(0) errors and keep the curve smooth
        float normalizedDist = distance / stimulus.radius;
        float falloff = 1f - Mathf.Log10(1f + 9f * normalizedDist); 
    
        float percievedLoudness = stimulus.baseLoudness * falloff;

        if (percievedLoudness < 0.01f) return;

        // check if sound is occluded with a linecast on the environment layer
        if (Physics.Linecast(soundPos, listenerPos, out RaycastHit _, hearingLayerMask))
        {
            // if it is reduce it by a multiplier
            percievedLoudness *= obfuscationMultiplier;
        }
    
        // give this percieved loudness to the ai and it can handle it
        AIDetection detection = GetComponentInParent<AIDetection>();
        detection.HearingDetection(percievedLoudness, soundPos);
        Debug.Log(percievedLoudness);
    }
}
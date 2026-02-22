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
        Vector3 listenerPos = transform.position;
        Vector3 soundPos = stimulus.position;
        
        // calculate the relative loudness using inverse square and the sounds baseLoudness
        float distanceSqr = (listenerPos - soundPos).sqrMagnitude;
        if (distanceSqr < 0.0001f) distanceSqr = 0.0001f;
        
        float percievedLoudness = stimulus.radius * stimulus.radius / distanceSqr;

        // check if sound is occluded with a linecast on the environment layer
        RaycastHit hit;
        if (Physics.Linecast(soundPos, listenerPos, out hit, hearingLayerMask))
        {
            // if it is reduce it by a multiplier
            percievedLoudness *= obfuscationMultiplier;
        }
        
        // give this percieved loudness to the ai and it can handle it
        // for now just debug log it until ollie comes up with an implementation
        AIDetection detection = GetComponentInParent<AIDetection>();
        detection.HearingDetection(percievedLoudness, soundPos);
        Debug.Log(percievedLoudness);
    }
}
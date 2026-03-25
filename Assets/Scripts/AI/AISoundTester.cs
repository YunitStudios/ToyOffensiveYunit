using SoundSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class AISoundTester : MonoBehaviour
{
    [SerializeField] private WwisePlayer wwisePlayer;
    [SerializeField] private SoundDataSO soundData;

    private SpatialAudioCue cue;

    void Awake()
    {
        cue = new SpatialAudioCue();
    }

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log(wwisePlayer);
            Debug.Log(soundData);
            cue.PlayCue(wwisePlayer, soundData.MaxHearingRadius, soundData.BaseLoudness, transform.position);
        }
    }
}
// REMOVE! 
// just a test script until the audio is in and uses the spatial sound system
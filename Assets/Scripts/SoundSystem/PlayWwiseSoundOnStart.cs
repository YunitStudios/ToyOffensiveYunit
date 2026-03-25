using SoundSystem;
using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
[RequireComponent(typeof(WwisePlayer))]
public class PlayWwiseSoundOnStart : MonoBehaviour
{ 
    WwisePlayer wwisePlayer;
    [SerializeField] SoundDataSO soundData;
    void Start()
    {
        wwisePlayer = gameObject.GetComponent<WwisePlayer>();
        wwisePlayer.PlaySound(soundData);
    }
}
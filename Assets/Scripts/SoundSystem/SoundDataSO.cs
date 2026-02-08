using UnityEngine;

public enum SoundType
{
    WwiseEvent,
    WwiseTrigger
}

[CreateAssetMenu(fileName = "NewSound", menuName = "ScriptableObjects/Sound")]
public class SoundDataSO : ScriptableObject
{
    [Header("Sound Data")]
    [Tooltip("The actual name of the sound trigger etc in Wwise")]
    public string WwiseName;
    [Tooltip("What does this do?")]
    public string Description;
    [Tooltip("What type is this? Event trigger etc")]
    public SoundType Type;
    [Tooltip("Is the sound 2D? Non 3D directional, can still be stereo will just play on the player")]
    public bool Is2D;
}
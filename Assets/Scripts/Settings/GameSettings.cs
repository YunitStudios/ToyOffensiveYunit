using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

// Code taken from Voidloop

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
[JsonObject(MemberSerialization.OptIn)]
public class GameSettings : ScriptableObject
{
    [Header("Controls")] 
    [JsonProperty] [Range(0, 100)] public float sensitivity;
    [JsonProperty] public bool toggleADS;
    [JsonProperty] public bool inverseLook;
    [JsonProperty] [Range(0,0.4f)] public float deadzone;

    [Header("Video")] 
    [JsonProperty] public int resolutionWidth = 1920;
    [JsonProperty] public int resolutionHeight = 1080;
    [JsonProperty] public bool fullScreen = true;
    [JsonProperty] [Range(40,90)] public float fov = 60;
    [JsonProperty] [Range(0,100)] public float brightness = 50;
    [JsonProperty] public QualitySettingValue quality = QualitySettingValue.Balanced;
    [JsonProperty] public bool motionBlur = false;
    
    [Header("Audio")]
    [JsonProperty] [Range(0, 100)] public float masterVolume;
    [JsonProperty] [Range(0, 100)] public float musicVolume;
    
    [Header("Accessibility")]
    [JsonProperty] public bool cameraShake = true;
    [JsonProperty] public bool colorBlindMode = false;
    [JsonProperty] public ColorBlindType colorBlindType;
    [JsonProperty] public bool hapticFeedback = true;

    public static void CopySettings(GameSettings a, GameSettings b)
    {
        a.sensitivity = b.sensitivity;
        a.toggleADS = b.toggleADS;
        a.inverseLook = b.inverseLook;
        a.deadzone = b.deadzone;
        
        a.resolutionWidth = b.resolutionWidth;
        a.resolutionHeight = b.resolutionHeight;
        a.fullScreen = b.fullScreen;
        a.fov = b.fov;
        a.brightness = b.brightness;
        a.quality = b.quality;
        a.motionBlur = b.motionBlur;
        
        a.masterVolume = b.masterVolume;
        a.musicVolume = b.musicVolume;
        
        a.cameraShake = b.cameraShake;
        a.colorBlindMode = b.colorBlindMode;
        a.colorBlindType = b.colorBlindType;
        a.hapticFeedback = b.hapticFeedback;
    }

}

public enum QualitySettingValue
{
    High,
    Balanced,
    Low
}

public enum ColorBlindType
{
    Protanopia,
    Deuteranopia,
    Tritanopia
}

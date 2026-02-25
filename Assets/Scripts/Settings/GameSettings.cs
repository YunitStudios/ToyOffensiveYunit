using Newtonsoft.Json;
using UnityEngine;

// Code taken from Voidloop

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Controls")] 
    [Range(0, 100)] public float sensitivty;
    public bool toggleADS;
    public bool inverseLook;
    [Range(0,0.4f)] public float deadzone;

    [Header("Video")] 
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public bool fullScreen = true;
    [Range(60,110)] public int fov = 90;
    public float brightness = 50;
    public QualitySettings quality = QualitySettings.Quality;
    public bool motionBlur = false;
    
    [Header("Audio")]
    [Range(0, 1)] public float masterVolume;
    [Range(0, 1)] public float musicVolume;
    
    [Header("Accessibility")]
    public bool cameraShake = true;
    public bool colorBlindMode = false;
    public ColorBlindType colorBlindType;
    public bool hapticFeedback = true;

    public static void CopySettings(GameSettings a, GameSettings b)
    {
        a.sensitivty = b.sensitivty;
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

public enum QualitySettings
{
    Balanced,
    Quality,
    Performance,
    UltraPerformance,
    Off
}

public enum ColorBlindType
{
    Protanopia,
    Deuteranopia,
    Tritanopia
}

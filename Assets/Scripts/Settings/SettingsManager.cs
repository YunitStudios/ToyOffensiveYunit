using System;
using EditorAttributes;
using Newtonsoft.Json;
using SoundSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettings playerSettings;
    [SerializeField] private GameSettings defaultSettings;
    [SerializeField] private WwisePlayer wwisePlayer;
    
    [SerializeField] private SoundDataSO masterRTPC;
    [SerializeField] private SoundDataSO worldRTPC;
    [SerializeField] private SoundDataSO nonWorldRTPC;
    
    public GameSettings GetSettings => playerSettings;
    public GameSettings GetDefaultSettings => defaultSettings;
    public static Action OnSettingsChanged;
    
    [Header("Config")] 
    [field: SerializeField] public Vector2 SensitivityRange = new(0.1f, 1f);
    public float GetSensitivityValue => Mathf.Lerp(SensitivityRange.x, SensitivityRange.y, playerSettings.sensitivity/100);
    [field: SerializeField] public Vector2 BrightnessRange = new(0.5f, 1.5f);
    public float GetBrightnessValue => Mathf.Lerp(BrightnessRange.x, BrightnessRange.y, playerSettings.brightness/100);


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);
        

    }

    private void OnEnable()
    {
        LoadSettings();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SaveSettings();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySettingsToScene();
    }
    
    [Button]
    private void ResetSettings()
    {
        GameSettings.CopySettings(playerSettings, defaultSettings);
        ApplySettingsToScene();
    }

    [Button]
    public void ApplySettingsToScene()
    {
        if (!playerSettings)
        {
            Debug.LogError("No valid settings SO on SettingsManager");
            return;
        }
        
        OnSettingsChanged?.Invoke();
        
        // Apply deadzone
        if (InputManager.Instance)
        {
            //InputManager.Instance.SetSensitivity(playerSettings.sensitivity);
            InputManager.Instance.ToggleInverted(playerSettings.inverseLook);
            InputManager.Instance.SetDeadzone(playerSettings.deadzone);
        }
        
        // Clamp resolution to 640 x 480 minimum
        playerSettings.resolutionWidth = Mathf.Max(playerSettings.resolutionWidth, 640);
        playerSettings.resolutionHeight = Mathf.Max(playerSettings.resolutionHeight, 480);
        
        // Apply resolution
        Screen.SetResolution(playerSettings.resolutionWidth, playerSettings.resolutionHeight, playerSettings.fullScreen);
        
        // Apply quality
        SetGameQuality(playerSettings.quality);
        
        // Apply volumes
        wwisePlayer.ChangeRTPC(masterRTPC, playerSettings.masterVolume);
        wwisePlayer.ChangeRTPC(worldRTPC, playerSettings.worldVolume);
        wwisePlayer.ChangeRTPC(nonWorldRTPC, playerSettings.nonWorldVolume);
    }
    
    public void SetGameQuality(QualitySettingValue type)
    {
        switch (type)
        {
            case QualitySettingValue.Low:
                QualitySettings.SetQualityLevel(2, true);
                break;
            case QualitySettingValue.Balanced:
                QualitySettings.SetQualityLevel(1, true);
                break;
            case QualitySettingValue.High:
                QualitySettings.SetQualityLevel(0, true);
                break;
        }
    }
    
    private void LoadSettings()
    {
        try
        {
            // Load settings from player prefs
            if (PlayerPrefs.HasKey("PlayerSettings"))
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                
                settings.Converters.Add(new GameSettingsConverter(defaultSettings));

                string dataToLoad = PlayerPrefs.GetString("PlayerSettings");
                GameSettings.CopySettings(playerSettings, JsonConvert.DeserializeObject<GameSettings>(dataToLoad, settings));
            }
            else
            {
                // If no saved settings, use defaults
                playerSettings = defaultSettings;
            }
        }
        catch (Exception e)
        {
            playerSettings = defaultSettings;
            Debug.LogError("Failed to load settings, using defaults. Error: " + e.Message);
        }

    }
    private void SaveSettings()
    {
        // Store settings into player prefs as JSON
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };
            
        // Convert data to string
        string dataToStore = JsonConvert.SerializeObject(playerSettings, settings);
        PlayerPrefs.SetString("PlayerSettings", dataToStore);
        PlayerPrefs.Save();
    }
    
}

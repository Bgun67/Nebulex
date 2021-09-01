using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Game_Settings : MonoBehaviour
{
    public GameObject[] settingsPanels;
    [Header("Graphics Settings")]
    public UDBox MSAAUD;
    public UDBox VSyncUD;
    public UDBox qualityUD;
    public UDBox fullscreenModeUD;
    public bool useGUI = true;

    [Space]
    [Header("Gameplay Settings")]
    public Slider lookSensitivitySlider;

    [Space]
    [Header("Audio Settings")]
    public AudioMixer masterMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider voicePromptVolumeSlider;

    

    public struct GraphicsSettings{
        public int MSAA;
        public int VSync;
        public int qualityLevel;
        public int fullScreenMode;
    }
    public struct GameplaySettings{
        public float lookSensitivity;
    }

    public struct AudioSettings{
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float voicePromptVolume;
    }
    public static GraphicsSettings currGraphicsSettings;
    public static GameplaySettings currGameplaySettings;
    public static AudioSettings currAudioSettings;

    public static void LoadGraphicsSettings(){
        //Pull game settings from file
        string _graphicsSettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/graphicssettings.json");
        currGraphicsSettings = JsonUtility.FromJson<GraphicsSettings>(_graphicsSettings);
    }
    public static void LoadGameplaySettings(){
        //Pull game settings from file
        string _gameplaySettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/gameplaysettings.json");
        currGameplaySettings = JsonUtility.FromJson<GameplaySettings>(_gameplaySettings);
    }
    public static void LoadAudioSettings(){
        //Pull game settings from file
        string _audioSettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/audiosettings.json");
        currAudioSettings = JsonUtility.FromJson<AudioSettings>(_audioSettings);
    }
    
    public static void SaveGameSettings(){
        //Pull game settings from file
        string _graphicsSettings =JsonUtility.ToJson(currGraphicsSettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphicssettings.json", _graphicsSettings);
        //Pull game settings from file
        string _gameplaySettings =JsonUtility.ToJson(currGameplaySettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/gameplaysettings.json", _gameplaySettings);
        //Pull game settings from file
        string _audioSettings =JsonUtility.ToJson(currAudioSettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/audiosettings.json", _audioSettings);
    }

     public static void RestoreGraphicsSettings(){
        //Restore default settings
        currGraphicsSettings.MSAA = 1;
        currGraphicsSettings.VSync = 1;
        currGraphicsSettings.qualityLevel = 6;
        currGraphicsSettings.fullScreenMode = 1;

        string _graphicsSettings = JsonUtility.ToJson(currGraphicsSettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphicssettings.json", _graphicsSettings);
    }
    public static void RestoreGameplaySettings(){
        //Restore default settings
        currGameplaySettings.lookSensitivity = 0.5f;
        SaveGameSettings();
    }
    public static void RestoreAudioSettings(){
        //Restore default settings
        currAudioSettings.masterVolume = 0.5f;
        currAudioSettings.musicVolume = 0.5f;
        currAudioSettings.sfxVolume = 0.5f;
        currAudioSettings.voicePromptVolume = 0.5f;
        SaveGameSettings();
    }
    // Start is called before the first frame update
    void Start()
    {
        try{
            LoadGraphicsSettings();
        }
        catch{
            print("Restoring Graphics Settings");
            RestoreGraphicsSettings();
        }
        try{
            LoadGameplaySettings();
        }
        catch{
            RestoreGameplaySettings();
        }
        try{
            LoadAudioSettings();
        }
        catch{
            RestoreAudioSettings();
        }
        ApplySettings();
        if(useGUI)
            UpdateGUI();
    }

    public void ChangeMSAALevel(string level)
    {
        currGraphicsSettings.MSAA = int.Parse(level);
        QualitySettings.antiAliasing = currGraphicsSettings.MSAA;
    }
    public void ChangeVSyncLevel(string level)
    {
        currGraphicsSettings.VSync = int.Parse(level);
        QualitySettings.vSyncCount = currGraphicsSettings.VSync;
    }
    public void ChangeQualityLevel(string level)
    {
        int _qualityLevel = 5;
        switch(level){
            case "Very Low":
            _qualityLevel = 0;
            break;
            case "Low":
            _qualityLevel = 1;
            break;
            case "Medium":
            _qualityLevel = 2;
            break;
            case "High":
            _qualityLevel = 3;
            break;
            case "Very High":
            _qualityLevel = 4;
            break;
            case "Ultra":
            _qualityLevel = 5;
            break;


        }
        currGraphicsSettings.qualityLevel = _qualityLevel;
        ApplySettings();
        
    }

    public void ChangeFullScreenMode(string mode)
    {
        int _fullscreenMode = 5;
        switch(mode){
            case "Exclusive Fullscreen":
            _fullscreenMode = 0;
            break;
            case "Fullscreen Window":
            _fullscreenMode = 1;
            break;
            case "Maximized Window":
            _fullscreenMode = 2;
            break;
            case "Windowed":
            _fullscreenMode = 3;
            break;

        }

        currGraphicsSettings.fullScreenMode = _fullscreenMode;
        Screen.fullScreenMode = (FullScreenMode)currGraphicsSettings.fullScreenMode;
    }

    public void ChangeLookSensitivity(){
        currGameplaySettings.lookSensitivity = lookSensitivitySlider.value;
		MInput.sensitivity = currGameplaySettings.lookSensitivity;
        ApplySettings();
    }

    #region Audio Settings
    public void ChangeMasterVolume(){
        currAudioSettings.masterVolume = masterVolumeSlider.value;
		masterMixer.SetFloat("Master Volume", Mathf.Log10(currAudioSettings.masterVolume)*20.0f);
        ApplySettings();
    }
    public void ChangeMusicVolume(){
        currAudioSettings.musicVolume = musicVolumeSlider.value;
		masterMixer.SetFloat("Music Volume", Mathf.Log10(currAudioSettings.musicVolume)*20.0f);
        ApplySettings();
    }
    public void ChangeSFXVolume(){
        currAudioSettings.sfxVolume = sfxVolumeSlider.value;
		masterMixer.SetFloat("SFX Volume", Mathf.Log10(currAudioSettings.sfxVolume)*20.0f);
        ApplySettings();
    }
    public void ChangeVoicePromptVolume(){
        currAudioSettings.voicePromptVolume = voicePromptVolumeSlider.value;
		//masterMixer.SetFloat("Voice Prompt Volume", Mathf.Log10(currAudioSettings.voicePromptVolume)*20.0f);
        ApplySettings();
    }
    #endregion

    public void ApplySettings(){
        QualitySettings.antiAliasing = currGraphicsSettings.MSAA;
        QualitySettings.vSyncCount = currGraphicsSettings.VSync;
        QualitySettings.SetQualityLevel(currGraphicsSettings.qualityLevel);
        Screen.fullScreenMode = (FullScreenMode)currGraphicsSettings.fullScreenMode;

        MInput.sensitivity = currGameplaySettings.lookSensitivity;

        //Audio Settings
        masterMixer.SetFloat("Master Volume", Mathf.Log10(currAudioSettings.masterVolume)*20.0f);
        masterMixer.SetFloat("Music Volume", Mathf.Log10(currAudioSettings.musicVolume)*20.0f);
        masterMixer.SetFloat("SFX Volume", Mathf.Log10(currAudioSettings.sfxVolume)*20.0f);
        //masterMixer.SetFloat("Voice Prompt Volume", Mathf.Log10(currAudioSettings.voicePromptVolume)*20.0f);

        SaveGameSettings();
    }

    void UpdateGUI(){
        //Graphics Settings
        MSAAUD.index = currGraphicsSettings.MSAA;
        VSyncUD.index = currGraphicsSettings.VSync;
        qualityUD.index = currGraphicsSettings.qualityLevel;
        fullscreenModeUD.index = currGraphicsSettings.fullScreenMode;

        //Gameplay Settings
        lookSensitivitySlider.value = currGameplaySettings.lookSensitivity;

        //Audio Settings
        masterVolumeSlider.value = currAudioSettings.masterVolume;
        musicVolumeSlider.value = currAudioSettings.musicVolume;
        sfxVolumeSlider.value = currAudioSettings.sfxVolume;
        voicePromptVolumeSlider.value = currAudioSettings.voicePromptVolume;
    }

    public void ShowSettingsPanel(int panelNumber){
        for(int i = 0; i<settingsPanels.Length; i++){
            if(i == panelNumber){
                settingsPanels[i].SetActive(true);
            }
            else{
                settingsPanels[i].SetActive(false);
            }
        }
    }
}

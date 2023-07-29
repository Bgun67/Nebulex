using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using DOMJson;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class Game_Settings : MonoBehaviour
{
    const string AUDIO_SETTINGS_PATH = "audio_settings.json";

    [SerializeField]
    private UIDocument m_UIDocument;
    private VisualElement m_Root;

    public GameObject[] settingsPanels;
    [Header("Graphics Settings")]
    [SerializeField]
    private VolumeProfile m_PostProcessProfile;

    [Space]
    [Header("Gameplay Settings")]

    [Space]
    [Header("Audio Settings")]
    public static AudioMixer masterMixer;
    static Dictionary<string, FullScreenMode> FULLSCREEN_MODES = new Dictionary<string, FullScreenMode>(){
        {"Exclusive FullScreen", FullScreenMode.ExclusiveFullScreen},
        {"Fullscreen Window", FullScreenMode.FullScreenWindow},
        {"Maximized Window", FullScreenMode.MaximizedWindow},
        {"Windowed", FullScreenMode.Windowed}
    };
    

    public struct GraphicsSettings{
        public int MSAA;
        public int VSync;
        public int qualityLevel;
        public int fullScreenMode;
    }
    public struct GameplaySettings{
        public float lookSensitivity;
		public bool holdToGroundLock;
		public bool firstTime;
	}

    public struct AudioSettings{
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float voicePromptVolume;
    }
    public static JsonObject currGraphicsSettings = new JsonObject();
    public static JsonObject currGameplaySettings = new JsonObject();
    public static JsonObject currAudioSettings = new JsonObject();

    public static void LoadGraphicsSettings(){
        //Pull game settings from file
        string _graphicsSettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/graphics_settings.json");
        currGraphicsSettings = JsonObject.FromJson(_graphicsSettings);
    }
    public static void LoadGameplaySettings(){
        //Pull game settings from file
        string _gameplaySettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/gameplay_settings.json");
        currGameplaySettings = JsonObject.FromJson(_gameplaySettings);
    }
    public static void LoadAudioSettings(){
        //Pull game settings from file
        string _audioSettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + AUDIO_SETTINGS_PATH);
        currAudioSettings = JsonObject.FromJson(_audioSettings);
    }
    
    public static void SaveGameSettings(){
        //Pull game settings from file
        string _graphicsSettings = currGraphicsSettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphics_settings.json", _graphicsSettings);
        //Pull game settings from file
        string _gameplaySettings = currGameplaySettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/gameplay_settings.json", _gameplaySettings);
        //Pull game settings from file
        string _audioSettings = currAudioSettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + AUDIO_SETTINGS_PATH, _audioSettings);
    }

     public static void RestoreGraphicsSettings(){
        //Restore default settings
        currGraphicsSettings = new JsonObject();
        currGraphicsSettings["msaa"] = 2;
        currGraphicsSettings["vsync"] = 1;
        currGraphicsSettings["quality_level"] = QualitySettings.names.Length - 1;
        currGraphicsSettings["fullscreen_mode"] = (int)FullScreenMode.ExclusiveFullScreen;

        string _graphicsSettings = currGraphicsSettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphics_settings.json", _graphicsSettings);
    }
    public static void RestoreGameplaySettings(){
        //Restore default settings
        currGameplaySettings["look_sensitivity"] = 0.5f;
		currGameplaySettings["hold_to_ground_lock"] = true;
		currGameplaySettings["first_time"] = true;
		
        string _gameplaySettings = currGameplaySettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/gameplay_settings.json", _gameplaySettings);
    }
    public static void RestoreAudioSettings(){
        //Restore default settings
        currAudioSettings["master_volume"] = 0.5f;
        currAudioSettings["music_volume"] = 0.5f;
        currAudioSettings["sfx_volume"] = 0.5f;
        currAudioSettings["voice_prompt_volume"] = 0.5f;
        
        string audioSettings = currGameplaySettings.ToJson();
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + AUDIO_SETTINGS_PATH, audioSettings);
    }
    
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void LoadSettings()
	{
        print("Loading game settings");
		try
		{
			LoadGraphicsSettings();
		}
		catch
		{
			print("Restoring Graphics Settings");
			RestoreGraphicsSettings();
		}
		try
		{
			LoadGameplaySettings();

		}
		catch
		{
			print("Restoring Gameplay Settings");
			RestoreGameplaySettings();
		}
		try
		{
			LoadAudioSettings();
		}
		catch
		{
			print("Restoring Audio Settings");
			RestoreAudioSettings();
		}
		ApplySettings();
	}

    //Must be reenabled every time it's turned back on
    void OnEnable()
    {
        m_Root = m_UIDocument.rootVisualElement;

        BuildPage();
    }

    private void BuildPage()
    {
        //REMEMBER: Callbacks get unset when the UI GAMEOBJECT is hidden so:
        //Set slider value, THEN add the callbacks

        //Settings menu buttons
        var btn_Graphics = m_Root.Q<Button>("btn_Graphics");
        btn_Graphics.clickable.clicked += () => {ToggleMenuVisibility(m_Root, "menu_Graphics");};

        var btn_Gameplay = m_Root.Q<Button>("btn_Gameplay");
        btn_Gameplay.clickable.clicked += () => {ToggleMenuVisibility(m_Root, "menu_Gameplay");};

        var btn_Audio = m_Root.Q<Button>("btn_Audio");
        btn_Audio.clickable.clicked += () => {ToggleMenuVisibility(m_Root, "menu_Audio");};

        ToggleMenuVisibility(m_Root, "");

        //Graphics Settings
        var sldr_MSAASamples = m_Root.Q<Slider>("sldr_MSAASamples");
        sldr_MSAASamples.value = (int)currGraphicsSettings.Get("msaa", 2);
        sldr_MSAASamples.RegisterValueChangedCallback(v => {ChangeMSAALevel(Mathf.RoundToInt(v.newValue));});

        var sldr_VSyncPerFrame = m_Root.Q<Slider>("sldr_VSyncPerFrame");
        sldr_VSyncPerFrame.value = currGraphicsSettings.Get("vsync", 1);
        sldr_VSyncPerFrame.RegisterValueChangedCallback(v => {ChangeVSyncLevel(Mathf.RoundToInt(v.newValue));});

        var cbo_Quality = m_Root.Q<DropdownField>("cbo_Quality");
        cbo_Quality.choices = new List<string>(QualitySettings.names);
        cbo_Quality.value = QualitySettings.names[currGraphicsSettings.Get("quality_level", QualitySettings.names.Length - 1)];
        cbo_Quality.RegisterValueChangedCallback(v => {ChangeQualityLevel(v.newValue);});

        var cbo_FullScreenMode = m_Root.Q<DropdownField>("cbo_FullScreenMode");
        cbo_FullScreenMode.choices = new List<string>(FULLSCREEN_MODES.Keys);
        cbo_FullScreenMode.value = FULLSCREEN_MODES.FirstOrDefault(
            x=>x.Value == (FullScreenMode)(int)currGraphicsSettings.Get("fullscreen_mode", (int)FullScreenMode.ExclusiveFullScreen)).Key;
        cbo_FullScreenMode.RegisterValueChangedCallback(v => {ChangeFullScreenMode(v.newValue);});

        var sldr_MotionBlur = m_Root.Q<Slider>("sldr_MotionBlur");
        sldr_MotionBlur.value = currGraphicsSettings.Get("motion_blur", 0.5);
        sldr_MotionBlur.RegisterValueChangedCallback(v => {ChangeMotionBlur(v.newValue);});

        //Gameplay Settings
        var sldr_MouseSensitivity = m_Root.Q<Slider>("sldr_MouseSensitivity");
        sldr_MouseSensitivity.value = currGameplaySettings.Get("look_sensitivity", 0.5f);
        sldr_MouseSensitivity.RegisterValueChangedCallback(v => {ChangeLookSensitivity(v.newValue);});

        var chk_GroundLock = m_Root.Q<Toggle>("chk_GroundLock");
        chk_GroundLock.value = currGameplaySettings.Get("hold_to_ground_lock", true);
        chk_GroundLock.RegisterValueChangedCallback(v => {ChangeHoldToGroundLock(v.newValue);});

        //Audio Settings
        var sldr_MasterVolume = m_Root.Q<Slider>("sldr_MasterVolume");
        sldr_MasterVolume.value = currAudioSettings.Get("master_volume", 0.5f);
        sldr_MasterVolume.RegisterValueChangedCallback(v => {ChangeMasterVolume(v.newValue);});

        var sldr_MusicVolume = m_Root.Q<Slider>("sldr_MusicVolume");
        sldr_MusicVolume.value = currAudioSettings.Get("music_volume", 0.5f);
        sldr_MusicVolume.RegisterValueChangedCallback(v => {ChangeMusicVolume(v.newValue);});

        var sldr_SFXVolume = m_Root.Q<Slider>("sldr_SFXVolume");
        sldr_SFXVolume.value = currAudioSettings.Get("sfx_volume", 0.5f);
        sldr_SFXVolume.RegisterValueChangedCallback(v => {ChangeSFXVolume(v.newValue);});

        var sldr_VoicePromptVolume = m_Root.Q<Slider>("sldr_VoicePromptVolume");
        sldr_VoicePromptVolume.value = currAudioSettings.Get("voice_prompt_volume", 0.5f);
        sldr_VoicePromptVolume.RegisterValueChangedCallback(v => {ChangeVoicePromptVolume(v.newValue);});
        
    }

    public void ShowOptionsOnly(){
		this.gameObject.SetActive (true);

		MInput.inputLock = MInput.InputLock.LockAll;
        UnityEngine.Cursor.visible = true;

        var txt_Paused = m_Root.Q<Label>("txt_Paused");
        txt_Paused.style.display = DisplayStyle.None;
        var btn_Resume = m_Root.Q<Button>("btn_Resume");
        btn_Resume.clickable.clicked += () => {this.transform.parent.gameObject.SetActive(false);};
		var btn_Options = m_Root.Q<Button>("btn_Options");
        btn_Options.style.display = DisplayStyle.None;
        var btn_Loadout = m_Root.Q<Button>("btn_Loadout");
        btn_Loadout.style.display = DisplayStyle.None;
        var btn_Recall = m_Root.Q<Button>("btn_Recall");
        btn_Recall.style.display = DisplayStyle.None;
        var btn_Desert = m_Root.Q<Button>("btn_Desert");
        btn_Desert.style.display = DisplayStyle.None;	
	}

    void ToggleMenuVisibility(VisualElement root, string element){
        foreach(string menuString in new[] {"menu_Graphics", "menu_Gameplay","menu_Audio"}){
            var menu_Graphics = root.Q<VisualElement>(menuString);
            if(menuString == element)
                menu_Graphics.style.display = DisplayStyle.Flex;
            else
                menu_Graphics.style.display = DisplayStyle.None;
        }
        
        
    }

    public void ChangeMSAALevel(int level)
    {
        currGraphicsSettings["msaa"] = level;
        QualitySettings.antiAliasing = level;
        ApplySettings();
    }
    public void ChangeVSyncLevel(int level)
    {
        currGraphicsSettings["vsync"] = level;
        QualitySettings.vSyncCount = level;
        ApplySettings();
    }
    public void ChangeMotionBlur(float level){
        currGraphicsSettings["motion_blur"] = level;
        MotionBlur tmp;
        if(m_PostProcessProfile.TryGet<MotionBlur>(out tmp)){
            tmp.intensity.value = level;
        }
        ApplySettings();
    }
    public void ChangeQualityLevel(string level)
    {
        int _qualityLevel = Array.IndexOf(QualitySettings.names, level);

        if (_qualityLevel < 0){
            _qualityLevel = QualitySettings.names.Length - 1;
        }
        
        currGraphicsSettings["quality_level"] = _qualityLevel;
        ApplySettings();
        
    }

    public void ChangeFullScreenMode(string mode)
    {
        FullScreenMode _fullscreenMode = FULLSCREEN_MODES.GetValueOrDefault(mode, FullScreenMode.ExclusiveFullScreen);

        currGraphicsSettings["fullscreen_mode"] = (int)_fullscreenMode;
        Screen.fullScreenMode = _fullscreenMode;
    }

    //TODO: Move these to the new JSON serialized and set the slider value at init
    public void ChangeLookSensitivity(float value){
        currGameplaySettings["look_sensitivity"] = value;
		MInput.sensitivity = value;
        ApplySettings();
    }

	public void ChangeHoldToGroundLock(bool mode){
        currGameplaySettings["hold_to_ground_lock"] = mode;
        ApplySettings();
    }

    #region Audio Settings
    public void ChangeMasterVolume(float volumeLinear){
        currAudioSettings["master_volume"] = volumeLinear;
		masterMixer.SetFloat("Master Volume", Mathf.Log10(volumeLinear)*20.0f);
        ApplySettings();
    }
    public void ChangeMusicVolume(float volumeLinear){
        currAudioSettings["music_volume"] = volumeLinear;
		masterMixer.SetFloat("Music Volume", Mathf.Log10(volumeLinear)*20.0f);
        ApplySettings();
    }
    public void ChangeSFXVolume(float volumeLinear){
        currAudioSettings["sfx_volume"] = volumeLinear;
		masterMixer.SetFloat("SFX Volume", Mathf.Log10(volumeLinear)*20.0f);
        ApplySettings();
    }
    public void ChangeVoicePromptVolume(float volumeLinear){
        currAudioSettings["voice_prompt_volume"] = volumeLinear;
		//masterMixer.SetFloat("Voice Prompt Volume", Mathf.Log10(currAudioSettings.voicePromptVolume)*20.0f);
        ApplySettings();
    }
    #endregion

    public static void ApplySettings(){
        QualitySettings.antiAliasing = (int)currGraphicsSettings.Get("msaa", 2);
        QualitySettings.vSyncCount = currGraphicsSettings.Get("vsync", 1);
        QualitySettings.SetQualityLevel(currGraphicsSettings.Get("quality_level", QualitySettings.names.Length - 1));
        Screen.fullScreenMode = (FullScreenMode)(int)currGraphicsSettings.Get("fullscreen_mode", (int)FullScreenMode.ExclusiveFullScreen);

        MInput.sensitivity = currGameplaySettings.Get("look_sensitivity", 0.5f);

		//Audio Settings
		masterMixer = (AudioMixer)Resources.Load("_Mixers/Game");
        float masterVol = currAudioSettings.Get("master_volume", 0.5f);
        float musicVol = currAudioSettings.Get("music_volume", 0.5f);
        float sfxVol = currAudioSettings.Get("sfx_volume", 0.5f);

		masterMixer.SetFloat("Master Volume", Mathf.Log10(masterVol)*20.0f);
        masterMixer.SetFloat("Music Volume", Mathf.Log10(musicVol)*20.0f);
        masterMixer.SetFloat("SFX Volume", Mathf.Log10(sfxVol)*20.0f);
        //masterMixer.SetFloat("Voice Prompt Volume", Mathf.Log10(currAudioSettings.voicePromptVolume)*20.0f);

        SaveGameSettings();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Settings : MonoBehaviour
{
    public struct GraphicsSettings{
        public int MSAA;
        public int VSync;
        public int qualityLevel;
        public int fullScreenMode;
    }
    public static GraphicsSettings currGraphicsSettings;

    public static void LoadGameSettings(){
        //Pull game settings from file
        string _graphicsSettings = System.IO.File.ReadAllText(Application.persistentDataPath + "/graphicssettings.json");
        currGraphicsSettings = JsonUtility.FromJson<GraphicsSettings>(_graphicsSettings);
    }
    public static void SaveGameSettings(){
        //Pull game settings from file
        string _graphicsSettings =JsonUtility.ToJson(currGraphicsSettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphicssettings.json", _graphicsSettings);
    }

     public static void RestoreGameSettings(){
        //Restore default settings
        currGraphicsSettings.MSAA = 1;
        currGraphicsSettings.VSync = 1;
        currGraphicsSettings.qualityLevel = 6;
        currGraphicsSettings.fullScreenMode = 1;

        string _graphicsSettings = JsonUtility.ToJson(currGraphicsSettings);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/graphicssettings.json", _graphicsSettings);
    }
    // Start is called before the first frame update
    void Start()
    {
        try{
            LoadGameSettings();
        }
        catch{
            RestoreGameSettings();
        }
        ApplySettings();
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

    public void ApplySettings(){
        QualitySettings.antiAliasing = currGraphicsSettings.MSAA;
        QualitySettings.vSyncCount = currGraphicsSettings.VSync;
        QualitySettings.SetQualityLevel(currGraphicsSettings.qualityLevel);
        Screen.fullScreenMode = (FullScreenMode)currGraphicsSettings.fullScreenMode;
        SaveGameSettings();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Options_Menu : MonoBehaviour {
	public Button controlsButton;
	public Button qualityButton;
<<<<<<< HEAD
=======
	public Button fullScreenButton;

>>>>>>> Local-Git
	public Slider volumeSlider;
	public Slider sensitivitySlider;
	static string[] optionsInfo;
	//Runs when the program starts
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void LoadStaticData()
	{
		try
		{
<<<<<<< HEAD
			optionsInfo = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/Options Settings.txt"));
=======
			optionsInfo = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Options Settings.txt"));
			LoadSettings();
>>>>>>> Local-Git
		}
		catch
		{
			optionsInfo = Profile.RestoreOptionsFile();
<<<<<<< HEAD
		}
		AudioListener.volume = float.Parse(optionsInfo[0]);
		MInput.useMouse = (optionsInfo[1] == "True");
		MInput.sensitivity = float.Parse(optionsInfo[2]);
		QualitySettings.SetQualityLevel( int.Parse(optionsInfo[3]));

=======
			LoadSettings();

		}


	}
	static void LoadSettings()
	{
		AudioListener.volume = float.Parse(optionsInfo[0]);
		MInput.useMouse = (optionsInfo[1] == "True");
		MInput.sensitivity = float.Parse(optionsInfo[2]);
		QualitySettings.SetQualityLevel(int.Parse(optionsInfo[3]));
		if (optionsInfo[4] == "True")
		{
			//Screen.fullScreenMode = FullScreenMode.FullScreenWindow;//ExclusiveFullScreen;
			//Screen.fullScreen = true;
		}
		else
		{
			//Screen.fullScreen = false;
			//Screen.fullScreenMode = FullScreenMode.Windowed;
		}
>>>>>>> Local-Git
	}
	void Reset () {
		controlsButton = GameObject.Find("Controls Button").GetComponent<Button>();
		qualityButton = GameObject.Find("Quality Button").GetComponent<Button>();
<<<<<<< HEAD
=======
		fullScreenButton = GameObject.Find("Full Screen Button").GetComponent<Button>();

>>>>>>> Local-Git

		volumeSlider = GameObject.Find("Volume Slider").GetComponent<Slider>();
		sensitivitySlider = GameObject.Find("Sensitivity Slider").GetComponent<Slider>();

		//AddButtonFunction(controlsButton, (UnityAction) ControlsClicked);
	}
	void Start()
	{
		UpdateControlText();
		UpdateQualityText();
		volumeSlider.value = AudioListener.volume;
		sensitivitySlider.value = MInput.sensitivity;
	}
	public void ControlsClicked() {
		MInput.useMouse = !MInput.useMouse;
		UpdateControlText();
		SaveData();
	}
<<<<<<< HEAD
=======
	public void FullScreenClicked() {
		if (!Screen.fullScreen)
		{
		//	Screen.fullScreenMode = FullScreenMode.FullScreenWindow;//ExclusiveFullScreen;
		//	Screen.fullScreen = true;

		}
		else
		{
		//	Screen.fullScreen = false;
		//	Screen.fullScreenMode = FullScreenMode.Windowed;
		}
		UpdateFullScreenText();
		SaveData();
	}
>>>>>>> Local-Git
	void UpdateControlText()
	{
		Text _text = controlsButton.GetComponentInChildren<Text>();
		if (MInput.useMouse)
		{
			_text.text = "Mouse";
		}
		else
		{
			_text.text = "Joystick";
		}
	}
<<<<<<< HEAD
=======
	void UpdateFullScreenText()
	{
		Text _text = fullScreenButton.GetComponentInChildren<Text>();
		if (Screen.fullScreen)
		{
			_text.text = "Off";
		}
		else
		{
			_text.text = "On";
		}
	}
>>>>>>> Local-Git
	public void UpdateVolume()
	{
		AudioListener.volume = volumeSlider.value;
		SaveData();
	}
	public void UpdateSensitivity()
	{
		MInput.sensitivity = sensitivitySlider.value;
		SaveData();
	}
	public void SetQualityLevel()
	{
		string[] names = QualitySettings.names;
		if (QualitySettings.GetQualityLevel() < names.Length-1)
		{
			QualitySettings.IncreaseLevel(true);
		}
		else
		{
			QualitySettings.SetQualityLevel(0);
		}
		UpdateQualityText();
		SaveData();
	}
	void UpdateQualityText()
	{
		Text _text = qualityButton.GetComponentInChildren<Text>();

		_text.text = QualitySettings.names[QualitySettings.GetQualityLevel()];
	}
	public void Close()
	{
		SceneManager.UnloadScene("Options");
	}
	void AddButtonFunction(Button _button, UnityAction _functionName)
	{
		if (_button.onClick.GetPersistentEventCount() > 0)
		{
			Debug.Log("Already a listener in" +_button.gameObject.name);
			return;
		}
		print(_functionName);
		_button.onClick.AddListener(delegate { _functionName(); });

	}
	void SaveData()
	{
		optionsInfo[0] = AudioListener.volume.ToString();
		optionsInfo[1] = MInput.useMouse.ToString();
		optionsInfo[2] = MInput.sensitivity.ToString();
		optionsInfo[3] = QualitySettings.GetQualityLevel().ToString();
<<<<<<< HEAD
		System.IO.File.WriteAllLines (Application.streamingAssetsPath+"/Options Settings.txt",Util.ThiccWatermelon(optionsInfo) );
=======
		optionsInfo[4] = Screen.fullScreen.ToString();

		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Options Settings.txt",Util.ThiccWatermelon(optionsInfo) );
>>>>>>> Local-Git

	}
}

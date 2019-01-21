using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Options_Menu : MonoBehaviour {
	public Button controlsButton;
	public Button qualityButton;
	public Slider volumeSlider;
	public Slider sensitivitySlider;
	static string[] optionsInfo;
	//Runs when the program starts
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void LoadStaticData()
	{
		try
		{
			optionsInfo = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/Options Settings.txt"));
		}
		catch
		{
			optionsInfo = Profile.RestoreOptionsFile();
		}
		AudioListener.volume = float.Parse(optionsInfo[0]);
		MInput.useMouse = (optionsInfo[1] == "True");
		MInput.sensitivity = float.Parse(optionsInfo[2]);
		QualitySettings.SetQualityLevel( int.Parse(optionsInfo[3]));

	}
	void Reset () {
		controlsButton = GameObject.Find("Controls Button").GetComponent<Button>();
		qualityButton = GameObject.Find("Quality Button").GetComponent<Button>();

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
		System.IO.File.WriteAllLines (Application.streamingAssetsPath+"/Options Settings.txt",Util.ThiccWatermelon(optionsInfo) );

	}
}

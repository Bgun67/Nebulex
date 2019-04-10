using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Profile : MonoBehaviour {
	public InputField nameInput;
	public Toggle mouseToggle;
	public string[] playerInfo;
	public Text playerScore;
	// Use this for initialization
	void Start () {
		try
		{
			playerInfo = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Player Data.txt"));
			print("Info Length" + playerInfo.Length);
			playerScore.text = playerInfo[1];
			nameInput.text = playerInfo[0];
		}
		catch
		{
			RestoreDataFile();
		}

	}
	public void SaveChanges(){
		playerInfo [0] = nameInput.text;
		print ("Info Length 2:" + playerInfo.Length);

		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Player Data.txt",Util.ThiccWatermelon(playerInfo));

	}
	public void OpenOptions()
	{
		SceneManager.LoadScene("Options", LoadSceneMode.Additive);
	}
	public void Back(){
		SceneManager.LoadScene ("Start Scene");
	}
	public static string[] RestoreDataFile(){
		string[] playerData = new string[]{ "Unnamed Player", "10", "", "192.168.2.40", "true"};
		System.IO.File.WriteAllLines (Application.persistentDataPath + "/Player Data.txt", Util.ThiccWatermelon (playerData));
		return playerData;
	}
	public static string[] RestoreMatchFile(){
		
		string[] matchSettings = new string[]{ "1200", "Destruction" };
		System.IO.File.WriteAllLines (Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon (matchSettings));
		return matchSettings;
	}
	public static string[] RestoreLoadoutFile(){

		print("Restoring loadout file");
		string[] loadoutSettings = new string[]{ "SRR-3", "FN-227", "None", "None"};
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Loadout Settings.txt",Util.ThiccWatermelon(loadoutSettings) );
		return loadoutSettings;

	}
	public static string[] RestoreOptionsFile()
	{
		print("Restoring Options file");
		string[] optionsSettings = new string[]{ "1", "True", "1", "2", "False"};
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Options Settings.txt",Util.ThiccWatermelon(optionsSettings) );
		return optionsSettings;
	}
	public static string[,] RestoreControlsFile()
	{
		print("Restoring Controls file");
		string[,] controlsSettings = new string[,]{
			{"Fire1", "Fire1"},
			{ "Sprint", "Sprint"},
			{ "Jump", "Jump"},
			{ "Left Trigger", "Left Trigger"}
		};
		
		return controlsSettings;
	}
	public static void SaveControls(string[,] _controlsSettings)
	{
		string settingsString = "";
		for (int i = 0; i < _controlsSettings.GetLength(0); i++)
		{
			settingsString += _controlsSettings[i, 0] + "-" + _controlsSettings[i, 1] + "\r\n";
		}
		System.IO.File.WriteAllText(Application.persistentDataPath + "/Controller Button Mappings.txt", settingsString);
	}


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Profile : MonoBehaviour {
	public InputField nameInput;
	public Toggle mouseToggle;
	public string[] playerInfo = new string[5];
	public Text playerScore;
	public Text playerKills;
	public Text playerDeaths;
	public Text playerKD;
	// Use this for initialization
	void Start () {
		try
		{
			playerInfo = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Player Data.txt"));
			print("Info Length" + playerInfo.Length);
			nameInput.text = playerInfo[0];
			playerScore.text = playerInfo[1];
			playerKills.text = playerInfo[2];
			playerDeaths.text = playerInfo[3];
			if (float.Parse(playerInfo[3]) < 1)
			{
				playerKD.text = "<color=yellow>N/A</color>";
			}
			else
			{
				playerKD.text = (float.Parse(playerInfo[2]) / float.Parse(playerInfo[3])).ToString();
			}
		}
		catch
		{
			
			playerInfo = RestoreDataFile();
		}

		GameObject.FindObjectOfType<Player_Controller>().anim.SetFloat("Look Speed", 0.5f);

	}
	public void SaveChanges(){
		//Strip Characters
		nameInput.text = nameInput.text.Replace("[", "");
		nameInput.text = nameInput.text.Replace("]", "");
		nameInput.text = nameInput.text.Replace("%", "");

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
		Debug.LogWarning("Restoring Data File");
		string[] playerData = new string[]{ "Unnamed Player", "10", "0", "0","True"};
		System.IO.File.WriteAllLines (Application.persistentDataPath + "/Player Data.txt", Util.ThiccWatermelon (playerData));
		return playerData;
	}
	public static string[] RestoreMatchFile(){
		
		string[] matchSettings = new string[]{ "1200", "Destruction", "Space" };
		System.IO.File.WriteAllLines (Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon (matchSettings));
		return matchSettings;
	}
	public static string[] RestoreLoadoutFile(){
		
		string[] loadoutSettings = new string[]{"SRR-3", "FN-227", "None", "None"};
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Loadout Settings.txt",Util.ThiccWatermelon(loadoutSettings) );
		return loadoutSettings;

	}
	public static string[] RestoreOptionsFile()
	{
		print("Restoring Options file");

		string[] optionsSettings = new string[]{ "1", "True", "1", "3", "False"};
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Options Settings.txt",Util.ThiccWatermelon(optionsSettings) );
		return optionsSettings;
	}


}

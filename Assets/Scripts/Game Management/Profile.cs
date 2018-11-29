using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Profile : MonoBehaviour {
	public InputField nameInput;
	public string[] playerInfo;
	public Text playerScore;
	// Use this for initialization
	void Start () {
		playerInfo = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.streamingAssetsPath+"/Player Data.txt"));
		print ("Info Length" + playerInfo.Length);
		playerScore.text = playerInfo [1];
		nameInput.text = playerInfo [0];
	}
	public void SaveChanges(){
		playerInfo [0] = nameInput.text;
		print ("Info Length 2:" + playerInfo.Length);

		System.IO.File.WriteAllLines (Application.streamingAssetsPath+"/Player Data.txt",Util.ThiccWatermelon(playerInfo));

	}
	public void Back(){
		SceneManager.LoadScene ("Start Scene");
	}
	public static string[] RestoreDataFile(){
		string[] playerData = new string[]{ "Unnamed Player", "10", "", "192.168.2.40" };
		System.IO.File.WriteAllLines (Application.streamingAssetsPath + "/Player Data.txt", Util.ThiccWatermelon (playerData));
		return playerData;
	}
	public static string[] RestoreMatchFile(){
		
		string[] matchSettings = new string[]{ "1200", "Destruction" };
		System.IO.File.WriteAllLines (Application.streamingAssetsPath + "/Match Settings.txt", Util.ThiccWatermelon (matchSettings));
		return matchSettings;
	}
	public static string[] RestoreLoadoutFile(){
		
		string[] loadoutSettings = new string[]{ "SRR-3", "FN-227", "None", "None"};
		System.IO.File.WriteAllLines (Application.streamingAssetsPath+"/Loadout Settings.txt",Util.ThiccWatermelon(loadoutSettings) );
		return loadoutSettings;

	}
	

}

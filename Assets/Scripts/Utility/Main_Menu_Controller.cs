using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu_Controller : MonoBehaviour {

	public GameObject creditsPanel;
	
	// Use this for initialization
	void Reset(){
		creditsPanel = GameObject.Find("Credits Panel");
	}
	void Start(){
		if (System.IO.File.Exists(Application.streamingAssetsPath + "/server_config.json")){
			this.LoadMatchScene();
		}
	}
	

	// Update is called once per frame
	public void StartMultiplayer () {

		SceneManager.LoadScene ("Space");
	}
	public void StartLobby () {
		SceneManager.LoadScene ("LobbyScene");
	}
	public void LoadMatchScene () {
		SceneManager.LoadScene ("MatchScene");
	}
	public void LoadLoadout(){
		SceneManager.LoadScene ("Loadout Scene");

	}
	public void LoadProfile(){
		SceneManager.LoadScene ("Profile Scene");

	}
	public void ShowCredits()
	{
		//creditsPanel = GameObject.Find("Credits Panel");
		creditsPanel.SetActive(true);
	}
	public void HideCredits()
	{
		//creditsPanel = GameObject.Find("Credits Panel");
		creditsPanel.SetActive(false);
	}

	public void ExitGame(){
		print("Quitting Game");
		Application.Quit();
	}
}

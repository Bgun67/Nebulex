using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu_Controller : MonoBehaviour {

	public GameObject creditsPanel;
	float volume = 0f;
	// Use this for initialization
	void Reset(){
		creditsPanel = GameObject.Find("Credits Panel");
	}
	void Start () {
		if(FindObjectsOfType<Main_Menu_Controller>().Length > 1){
			Destroy(this.gameObject);
		}
		else{
			volume = this.GetComponent<AudioSource>().volume;
			DontDestroyOnLoad (this.gameObject);
		}
	}
	void Update()
	{
		this.GetComponent<AudioSource>().volume = volume;
		if (SceneManager.GetActiveScene().name == "LobbyScene")
		{
			volume = Mathf.Lerp(volume, 0f, 0.05f);
		}
		
		if(volume < 0.05f){
			Destroy(this.gameObject);
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
		creditsPanel.SetActive(true);
	}
	public void HideCredits()
	{
		creditsPanel.SetActive(false);
	}

	public void ExitGame(){
		print("Quitting Game");
		Application.Quit();
	}
}

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
	void Start () {
		DontDestroyOnLoad (this.gameObject);
	}
	void Update()
	{
		if (SceneManager.GetActiveScene().name == "LobbyScene")
		{
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
}

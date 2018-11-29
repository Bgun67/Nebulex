using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu_Controller : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//DontDestroyOnLoad (FindObjectOfType<AudioSource> ());
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour {
	public GameObject confirmQuitPanel;
	public GameObject confirmRecallPanel;
	public GameObject eventSystem;
	public GameObject player;

	// Use this for initialization
	void OnEnable(){
	}
	public void Pause (GameObject _player) {
		this.gameObject.SetActive (true);
		player = _player;
		//eventSystem.SetActive (true);

		//edit to remove all looking later
		player.GetComponent<Player_Controller> ().enabled = false;
	}
	public void Recall(){
		confirmRecallPanel.SetActive (true);
	}
	public void Quit(){
		confirmQuitPanel.SetActive (true);
	}
	public void Deny(){
		confirmQuitPanel.SetActive (false);
		confirmRecallPanel.SetActive (false);

	}
	public void KillPlayer(){
		player.GetComponent<Damage> ().TakeDamage (200, 0);
		this.gameObject.SetActive (false);
	}
	public void GoToMainMenu(){
		try{
			//Cleanup Metwork
			Metwork.Disconnect();
			Destroy(GameObject.FindObjectOfType<Metwork>().gameObject);
			
			//Destroy(GameObject.Find("WebRtcNetworkFactory").gameObject);
		}
		catch{}
		SceneManager.LoadScene ("Start Scene");
	}
	public void Resume(){
		this.gameObject.SetActive(false);
		player.GetComponent<Player_Controller> ().enabled = true;

		//eventSystem.SetActive (false);
	}
}

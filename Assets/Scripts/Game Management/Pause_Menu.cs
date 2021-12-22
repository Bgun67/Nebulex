using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour {
	public GameObject confirmQuitPanel;
	public GameObject confirmRecallPanel;
	public GameObject settingsPanel;
	public GameObject eventSystem;

	// Use this for initialization
	void OnEnable(){
	}
	public void Pause () {
		this.gameObject.SetActive (true);
		//eventSystem.SetActive (true);

		MInput.inputLock = MInput.InputLock.LockAll;
		Cursor.visible = true;
	}
	public void Recall(){
		confirmRecallPanel.SetActive (true);
	}
	public void Options()
	{
		//SceneManager.LoadScene("Options", LoadSceneMode.Additive);
		settingsPanel.SetActive(true);
	}
	public void Quit(){
		confirmQuitPanel.SetActive (true);
	}
	public void Deny(){
		confirmQuitPanel.SetActive (false);
		confirmRecallPanel.SetActive (false);

	}
	public void KillPlayer(){
		Player_Controller player = FindObjectOfType<Game_Controller>().localPlayer;

		MInput.inputLock = MInput.InputLock.None;
		if (player)
		{
			player.GetComponent<Damage>().TakeDamage(200, 0, player.transform.position + player.transform.forward, true);
		}
		confirmRecallPanel.SetActive(false);
		Resume();
	}
	public void GoToMainMenu(){
		try{
			//Cleanup Metwork
			Metwork.Disconnect();
			//Destroy(GameObject.FindObjectOfType<Metwork>().gameObject);
			//Destroy(GameObject.Find("WebRtcNetworkFactory").gameObject);
		}
		catch{
			Debug.LogWarning("Failed to find one or more network components when quitting");
		}
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene ("Start Scene");
	}
	public void Resume(){
		settingsPanel.SetActive(false);
		this.gameObject.SetActive(false);
		MInput.inputLock = MInput.InputLock.None;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		UI_Manager._instance.isPaused = false;

		//eventSystem.SetActive (false);
	}
	public void GoToLoadoutScene()
	{
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene("Loadout Scene", LoadSceneMode.Additive);
	}

}

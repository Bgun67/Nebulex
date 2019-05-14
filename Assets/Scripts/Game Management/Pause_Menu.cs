﻿using System.Collections;
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

		MInput.inputLock = MInput.InputLock.LockAll;
		Cursor.visible = true;
	}
	public void Recall(){
		confirmRecallPanel.SetActive (true);
	}
	public void Options()
	{
		SceneManager.LoadScene("Options", LoadSceneMode.Additive);
	}
	public void Quit(){
		confirmQuitPanel.SetActive (true);
	}
	public void Deny(){
		confirmQuitPanel.SetActive (false);
		confirmRecallPanel.SetActive (false);

	}
	public void KillPlayer(){
		MInput.inputLock = MInput.InputLock.None;
		player.GetComponent<Damage> ().TakeDamage (200, 0, transform.position);
		confirmRecallPanel.SetActive(false);
		this.gameObject.SetActive (false);
	}
	public void GoToMainMenu(){
		try{
			//Cleanup Metwork
			Metwork.Disconnect();
			//Destroy(GameObject.FindObjectOfType<Metwork>().gameObject);
			//Destroy(GameObject.Find("WebRtcNetworkFactory").gameObject);
		}
		catch{
			Debug.LogError("Failed to find one or more network components when quitting");
		}
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene ("Start Scene");
	}
	public void Resume(){
		this.gameObject.SetActive(false);
		MInput.inputLock = MInput.InputLock.None;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		//eventSystem.SetActive (false);
	}
	public void GoToLoadoutScene()
	{
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene("Loadout Scene", LoadSceneMode.Additive);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Spawn_Scene_Manager : MonoBehaviour {

	public GameObject spawnPointPrefab;
	public GameObject deadPlayer;

	public GameObject[] spawnButtons;
	public Transform[] spawnPositions;

	Game_Controller gameController;
	public GameObject eventSystem;


	// Use this for initialization
	void Awake () {
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
		if (eventSystem == null) {
			eventSystem = GameObject.Find ("EventSystem");

		}
		if (MInput.useMouse)
		{
			Cursor.visible = true;
		}
		eventSystem.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] spawnPoints;
	
		spawnPoints = GameObject.FindGameObjectsWithTag ("Spawn Point " + gameController.localPlayer.GetComponent<Player_Controller>().team);
		if (spawnPoints.Length < 1) {
			spawnPoints = GameObject.FindGameObjectsWithTag ("Spawn Point 1");
		}
		
		for (int i = 0; i< spawnButtons.Length; i++) {
			if (i >= spawnPoints.Length) {
				spawnButtons [i].SetActive (false);
				continue;
			}
			spawnPositions[i] = spawnPoints[i].transform;
			//TODO: Unsafe code implementation
			Vector3 buttonPos = gameController.sceneCam.GetComponent<Camera>().WorldToScreenPoint (spawnPoints[i].transform.position);
			buttonPos.z = 0f;
			spawnButtons [i].SetActive (true);
			spawnButtons[i].transform.position = buttonPos;
		}
	}

	public void Spawn(int index){
		if (MInput.useMouse)
		{
			Cursor.visible = false;
		}
		Player_Controller _player = gameController.localPlayer.GetComponent<Player_Controller>();
		_player.transform.position = spawnPositions [index].position;
		_player.damageScript.initialPosition = null;
		_player.damageScript.Reactivate();
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			gameController.netView.RPC ("RPC_ActivatePlayer", MRPCMode.AllBuffered, new object[]{ _player.netObj.owner});
			if (_player.netObj.isLocal) {
				_player.sceneCam.enabled = false;
			}
		} else {
			gameController.RPC_ActivatePlayer (_player.netObj.owner);
			_player.sceneCam.enabled = false;

		}
		SceneManager.UnloadSceneAsync ("SpawnScene");
			
		
	}
}

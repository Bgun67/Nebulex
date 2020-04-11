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
	public GameObject sceneCam;

	Game_Controller gameController;
	public GameObject eventSystem;


	// Use this for initialization
	void Awake () {
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
		if (eventSystem == null) {
			eventSystem = GameObject.Find ("EventSystem");

		}
		sceneCam = gameController.sceneCam;
		eventSystem.SetActive (true);
		//sceneCam.transform.position = Vector3.Lerp(Vector3.zero, sceneCam.transform.position, 0.5f);
		sceneCam.GetComponent<Camera>().orthographic = true;
		sceneCam.GetComponent<Camera>().orthographicSize = 5f;

	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] spawnPoints;

		spawnPoints = GameObject.FindGameObjectsWithTag ("Spawn Point " + gameController.GetLocalTeam());
		
		

		if (spawnPoints.Length < 1) {
			spawnPoints = GameObject.FindGameObjectsWithTag ("Spawn Point 1");
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Bounds _bounds = new Bounds(spawnPoints[0].transform.position, Vector3.one);

		
		for (int i = 0; i< spawnButtons.Length; i++) {
			if (i >= spawnPoints.Length) {
				spawnButtons [i].SetActive (false);
				continue;
			}
			spawnPositions[i] = spawnPoints[i].transform;
			_bounds.Encapsulate(spawnPoints[i].transform.position);

			//TODO: Unsafe code implementation
			Vector3 buttonPos = gameController.sceneCam.GetComponent<Camera>().WorldToScreenPoint (spawnPoints[i].transform.position);
			buttonPos.z = 0f;
			spawnButtons [i].SetActive (true);
			spawnButtons[i].transform.position = buttonPos;
		}
		sceneCam.GetComponent<Camera>().orthographicSize = Mathf.Lerp(sceneCam.GetComponent<Camera>().orthographicSize,Mathf.Max(_bounds.extents.x,_bounds.extents.z)*1.5f, 0.3f);
		_bounds.center = new Vector3(_bounds.center.x, 1500f, _bounds.center.z);
		sceneCam.transform.position = _bounds.center;
		
		

	}



	public void Spawn(int index){
		if (MInput.useMouse)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		Player_Controller _player = gameController.localPlayer.GetComponent<Player_Controller>();
		_player.transform.rotation = Quaternion.LookRotation(spawnPositions [index].forward,Vector3.up);
		//_player.transform.up = Vector3.up;
		_player.transform.position = spawnPositions [index].position;

		_player.damageScript.initialPosition = null;
		_player.damageScript.Reactivate();
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			gameController.netView.RPC ("RPC_ActivatePlayer", MRPCMode.AllBuffered, new object[]{ _player.netObj.owner});
			if (_player.netObj.isLocal) {
				_player.sceneCam.enabled = false;
				_player.sceneCam.GetComponent<AudioListener>().enabled = false;

			}
		} else {
			//Enables the player
			gameController.RPC_ActivatePlayer (_player.netObj.owner);
			_player.sceneCam.enabled = false;
			_player.sceneCam.GetComponent<AudioListener>().enabled = false;

		}
		//zoom down effect
		_player.mainCamObj.transform.position = _player.sceneCam.transform.position;
		_player.mainCamObj.transform.rotation = _player.sceneCam.transform.rotation;
		SceneManager.UnloadSceneAsync ("SpawnScene");
		this.enabled = (false);


	}
}

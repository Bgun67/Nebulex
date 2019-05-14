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
		sceneCam.transform.position = Vector3.Lerp(Vector3.zero, sceneCam.transform.position, 0.5f);

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
		PositionSceneCam();
	}

	void PositionSceneCam()
	{

		
		GameObject[] spawn0Points = GameObject.FindGameObjectsWithTag("Spawn Point 0");
		Vector3 max = spawn0Points[0].transform.position;
		Vector3 min = spawn0Points[0].transform.position;
		FindMax(min, max, spawn0Points, out min, out max);
		FindMax(min, max, GameObject.FindGameObjectsWithTag("Spawn Point 1"), out min, out max);

		Vector3 center = Vector3.Lerp(min, max, 0.5f);
		center.y = 0f;
		print(center);

		sceneCam.transform.position = Vector3.Lerp(sceneCam.transform.position, center + Vector3.up * (max - center).magnitude / Mathf.Min(Screen.height, Screen.width)*2000f, 0.1f);

	}
	void FindMax(Vector3 initialMin, Vector3 initialMax, GameObject[] arrayToSearch, out Vector3 finalMin, out Vector3 finalMax)
	{
		finalMax = initialMax;
		finalMin = initialMin;
		foreach (GameObject spawnPoint in arrayToSearch)
		{
			Vector3 _pos = spawnPoint.transform.position;
			if (_pos.x > finalMax.x)
			{
				finalMax.x = _pos.x;
			}
			if (_pos.x < finalMin.x)
			{
				finalMin.x = _pos.x;
			}
			if (_pos.z > finalMax.z)
			{
				finalMax.z = _pos.z;
			}
			if (_pos.z < finalMin.z)
			{
				finalMin.z = _pos.z;
			}
		}
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

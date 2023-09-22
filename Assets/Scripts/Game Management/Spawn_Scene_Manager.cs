using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Spawn_Scene_Manager : MonoBehaviour {

	public GameObject spawnPointPrefab;
	public GameObject deadPlayer;

	public GameObject[] spawnButtons;
	public Transform[] spawnPositions;
	public Cinemachine.CinemachineVirtualCamera sceneCam;

	Game_Controller gameController;
	public GameObject eventSystem;


	// Use this for initialization
	void Awake () {
		gameController = Game_Controller.Instance;
		if (eventSystem == null) {
			eventSystem = GameObject.Find ("EventSystem");

		}
		eventSystem.SetActive (true);
		//sceneCam.transform.position = Vector3.Lerp(Vector3.zero, sceneCam.transform.position, 0.5f);
		sceneCam.enabled = true;

	}
	
	// Update is called once per frame
	void Update () {
		List<Spawn_Point> _allSpawns = new List<Spawn_Point>(FindObjectsOfType<Spawn_Point>());
		_allSpawns.RemoveAll(x => x.team != gameController.localTeam);

		Spawn_Point[] spawnPoints = _allSpawns.ToArray();
		if(spawnPoints.Length < 1){
			spawnPoints = FindObjectsOfType<Spawn_Point>();
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
			Vector3 buttonPos = FindObjectOfType<CinemachineBrain>().GetComponent<Camera>().WorldToScreenPoint (spawnPoints[i].transform.position);
			buttonPos.z = 0f;
			spawnButtons [i].SetActive (true);
			spawnButtons[i].transform.position = buttonPos;
		}
		sceneCam.m_Lens.OrthographicSize = Mathf.Lerp(sceneCam.m_Lens.OrthographicSize ,Mathf.Max(_bounds.extents.x,_bounds.extents.z)*1.5f, 0.3f);
		_bounds.center = new Vector3(_bounds.center.x, 1500f, _bounds.center.z);
		sceneCam.transform.position = _bounds.center;
		
		

	}



	public void Spawn(int index){
		if (MInput.useMouse)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		Player_Controller _player = Game_Controller.Instance.localPlayer;
		_player.transform.rotation = Quaternion.LookRotation(spawnPositions [index].forward,Vector3.up);
		_player.transform.position = spawnPositions [index].position;

		
		_player.Cmd_ActivatePlayer();
		

		//zoom down effect
		sceneCam.enabled = false;
		SceneManager.UnloadSceneAsync ("SpawnScene");
		this.enabled = (false);


	}
}

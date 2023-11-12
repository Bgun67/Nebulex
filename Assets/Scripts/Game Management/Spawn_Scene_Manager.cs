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
	List<Spawn_Point> spawnPoints;
	public Cinemachine.CinemachineVirtualCamera sceneCam;

	Game_Controller gameController;


	// Use this for initialization
	void Awake () {
		gameController = Game_Controller.Instance;
	}
	void OnEnable(){
		sceneCam.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {

		spawnPoints = new List<Spawn_Point>(FindObjectsOfType<Spawn_Point>());
		spawnPoints.RemoveAll(x => x.team != gameController.localTeam);

		if(spawnPoints.Count < 1){
			new List<Spawn_Point>(FindObjectsOfType<Spawn_Point>());
		}
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Bounds _bounds = new Bounds(spawnPoints[0].transform.position, Vector3.one);

		
		for (int i = 0; i< spawnButtons.Length; i++) {
			if (i >= spawnPoints.Count) {
				spawnButtons [i].SetActive (false);
				continue;
			}
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
		Player_Controller player = Game_Controller.Instance.localPlayer;
		player.transform.rotation = Quaternion.LookRotation(spawnPoints [index].transform.forward,Vector3.up);
		player.transform.position = spawnPoints [index].transform.position;
		
		//Needs to be enabled here so transistion works ok
		//This makes it extremeemly laggy for some reason?
		player.virtualCam.enabled = true;
		
		player.Cmd_ActivatePlayer();
		

		//zoom down effect
		sceneCam.enabled = false;
		this.gameObject.SetActive(false);


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transistion : MonoBehaviour {

	public GameObject[] sceneObjs;
	public Camera cam;
	AsyncOperation loadOperation;
	public MetworkView metView;
	Network_Manager netManager;

	// Use this for initialization
	void Start () {
		//unlock cursor
		if (Input.GetKey(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		//This could go horribly wrong, if it does change it back to loadscenemode.single
		loadOperation = SceneManager.LoadSceneAsync ("Space", LoadSceneMode.Single);
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		loadOperation.allowSceneActivation = false;
		loadOperation.priority = 0;
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		netManager = FindObjectOfType<Network_Manager>();
		Invoke("ContingencyLoad", 10f);
	}

	//Checks to see if the load has gone horribley wrong
	void ContingencyLoad(){
		if(this.enabled){
			SceneManager.UnloadSceneAsync ("TransistionScene");
			loadOperation.allowSceneActivation = true;

			if(Metwork.peerType == MetworkPeerType.Connected && Metwork.player != null){
				//SceneManager.LoadSceneAsync ("SpawnScene", LoadSceneMode.Additive);
				SceneManager.UnloadSceneAsync ("TransistionScene");
				loadOperation.allowSceneActivation = true;
			}
		}
	}

	void SceneManager_sceneLoaded (Scene _scene, LoadSceneMode _loadMode)
	{
		SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

		if (_scene.name == "Space") {
			
			//SceneManager.LoadSceneAsync ("SpawnScene", LoadSceneMode.Additive);
			


			RemoveScene();
		}


	}

	void RemoveScene(){
		if (Metwork.isServer) {
			metView.RPC ("RPC_LoadScene", MRPCMode.OthersBuffered, new object[]{ });
			print ("Queuing [Space] loading on clients");
		}
		//yield return new WaitForSeconds (3f);

		SceneManager.UnloadSceneAsync ("TransistionScene");
	}

	[MRPC] 
	void RPC_LoadScene(){
		loadOperation.allowSceneActivation = true;
	}
	
	// Update is called once per frame
	void Update () {
		print (loadOperation.progress);
		cam.farClipPlane = 300 + (700) * loadOperation.progress;
		if (loadOperation.progress > 0.8f && (Metwork.isServer||Metwork.players.Count >netManager.minStartingPlayers)) {
			loadOperation.allowSceneActivation = true;
		}
	}
}

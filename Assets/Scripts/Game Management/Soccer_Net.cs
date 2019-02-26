using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soccer_Net : MonoBehaviour {

	public Transform ballSpawn;
	Game_Controller gameController;
	public int team;
	public MetworkView netView;
	bool scored;
	public Carrier_Controller carrier;
	bool gravOn;
	public float untouchedTime;
	public Soccer_Ball ball;
	public GameObject[] spawns;
	// Use this for initialization
	void Start () {
		gameController = FindObjectOfType<Game_Controller> ();

		ball.gameObject.SetActive (true);
		if (team == 0) {
			RenderSettings.ambientLight = Color.black;
			LightmapSettings.lightmaps = new LightmapData[]{};
			try{
			GameObject.Find ("Directional Light").SetActive (false);
			}
			catch{

			}


			foreach (Door_Controller door in FindObjectsOfType<Door_Controller> ()) {
				door.locked = true;
			}
			foreach (Airlock_Controller airlock in FindObjectsOfType<Airlock_Controller>()) {
				airlock.enabled = false;
			}
			Elevator_Controller elevator = carrier.GetComponentInChildren<Elevator_Controller> ();

			elevator.hasPower = false;
			elevator.transform.position = elevator.top.position;
		 
			carrier.GetComponent<Rigidbody> ().isKinematic = true;
			gameController.shipTwoTransform.gameObject.SetActive (false);
			gameController.shipTwoTransform = gameController.shipOneTransform;
			gameController.sceneCam.GetComponent<Camera> ().fieldOfView = 28f;
			foreach (Player_Controller player in FindObjectsOfType<Player_Controller> ()) {
				player.GetComponent<Player_Controller> ().suffocationTime = 20000f;
			}
			StartCoroutine (CheckForSwitch ());
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("Spawn Point 0")) {
				go.SetActive (false);
			}
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("Spawn Point 1")) {
				go.SetActive (false);
			}
			foreach (GameObject go in spawns) {
				go.SetActive (true);
			}

			//Hide all the reflection probes blend modes
			MeshRenderer[] meshes = GameObject.FindObjectsOfType<MeshRenderer>();
			for (int i = 0; i < meshes.Length; i++) {
				meshes [i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				meshes [i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				
			}

			//Hide all the reflection probes blend modes
			SkinnedMeshRenderer[] skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
			for (int i = 0; i < skinnedMeshes.Length; i++) {
				skinnedMeshes [i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				skinnedMeshes [i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
			}
			//Hide all the reflection probes
			ReflectionProbe[] _probes = GameObject.FindObjectsOfType<ReflectionProbe>();
			for (int i = 0; i < _probes.Length; i++) {
				_probes [i].enabled = false;
			}

			//Hide all the lights
			Light[] _lights = GameObject.FindObjectsOfType<Light>();
			for (int i = 0; i < _lights.Length; i++) {
				_lights [i].enabled = false;
			}



			RenderSettings.fog = true;
			RenderSettings.fogMode = FogMode.Exponential;
			RenderSettings.fogColor = new Color (0, 0, 0, 57);
			RenderSettings.fogDensity = 0.01f;

			Invoke ("Register", 1f);
			InvokeRepeating ("IncreaseFlash",1f,5f);
			InvokeRepeating("CheckBall", 1f, 1f);

		}
	}

	void CheckBall()
	{
		if (Metwork.isServer)
		{
			
			if (ball.transform.position.sqrMagnitude > 40000f)
			{
				print("Too far out at:" + ball.transform.position);
				StartCoroutine(ResetBall());
				CancelInvoke("CheckBall");
				InvokeRepeating("CheckBall", 3.1f, 1f);
			}
			float sqrVelocity = ball.GetComponent<Rigidbody>().velocity.sqrMagnitude;
			if (sqrVelocity > 0.2f)
			{
				untouchedTime = 0f;
			}
			else
			{
				untouchedTime += 1f;
			}
			if (untouchedTime >= 30f)
			{
				StartCoroutine(ResetBall());
				untouchedTime = 0;
			}
		}

	}
	void Register(){
		Navigation.RegisterTarget (ball.transform, "Ball", 20f, Color.yellow);
	}
	void IncreaseFlash(){
		foreach (Fire fireScript in FindObjectsOfType<Fire>()) {
			if (fireScript.muzzleFlash != null) {
				ParticleSystem.LightsModule lights = fireScript.muzzleFlash.lights;
				lights.rangeMultiplier = 4f;

			}
		}
		//Hide all the reflection probes blend modes
		Fire[] meshes = GameObject.FindObjectsOfType<Fire>();
		for (int i = 0; i < meshes.Length; i++) {
			foreach(MeshRenderer _mesh in meshes[i].GetComponentsInChildren<MeshRenderer>()){
			
				if(_mesh != null){
					_mesh.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
					_mesh.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
					
				}
			}
			
		}

		//Hide all the reflection probes blend modes
		//SkinnedMeshRenderer[] skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
		//for (int i = 0; i < skinnedMeshes.Length; i++) {
		//	skinnedMeshes [i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		//	skinnedMeshes [i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
		//}
		//Hide all the reflection probes
		ReflectionProbe[] _probes = GameObject.FindObjectsOfType<ReflectionProbe>();
		for (int i = 0; i < _probes.Length; i++) {
			_probes [i].enabled = false;
		}

		//Hide all the lights
		Light[] _lights = GameObject.FindObjectsOfType<Light>();
		for (int i = 0; i < _lights.Length; i++) {
			_lights [i].enabled = false;
		}

		LightmapData _lightM = new LightmapData();
		_lightM.lightmapColor = Texture2D.blackTexture; 
		//Clear Lightmaps
		LightmapSettings.lightmaps = new LightmapData[40]{_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM,_lightM};
	}
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		if (!scored) {
			if (other.GetComponent<Soccer_Ball> () != null) {
				if (Metwork.peerType != MetworkPeerType.Disconnected) {
					
					if (Metwork.isServer) {
						netView.RPC ("RPC_Score", MRPCMode.AllBuffered, team);
						scored = true;

					} 

				} else {
					RPC_Score (team);

				}
			
			}
		}
	}
	[MRPC]
	public void RPC_Score(int _team){

		if (_team == 0) {
			
			gameController.scoreA+= 20;
		} else {
			gameController.scoreB+=20;
		}
		StartCoroutine (ResetBall ());


	}
	IEnumerator ResetBall(){
		yield return new WaitForSeconds (3f);
		ball.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		ball.transform.position = ballSpawn.position;
		scored = false;

	}
	IEnumerator CheckForSwitch(){
		yield return new WaitForSeconds (60f);
		while (true) {
				
			yield return new WaitForSeconds (Random.Range (25f, 35f));
				
			if (!gravOn) {
				yield return new WaitForSeconds (50f);

			} 
			if (Metwork.isServer) {
				netView.RPC ("RPC_SwitchGravity", MRPCMode.AllBuffered, new object[]{ gravOn });
			} else if (Metwork.peerType == MetworkPeerType.Disconnected) {
				RPC_SwitchGravity (gravOn);
			}
			gravOn = !gravOn;
			

		}
		

	}
	[MRPC]
	public void RPC_SwitchGravity(bool gravity){
		if (!gravity) {
			carrier.ShutdownGravity ();
		} else {
			carrier.ReactivateGravity ();
		}

	}

	
}

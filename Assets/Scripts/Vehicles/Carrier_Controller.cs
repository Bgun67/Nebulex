using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Michael Gunther: 2018-01-25
 * Purpose: Controls the main body of the venom carrier, mostly concerning the engines
 * Notes: 
 * Improvements/Fixes: render a predicted path to the flight deck
 * */

public class Carrier_Controller : MonoBehaviour {

	//This script will have the three engines to see if they are powered up and force at their location
	public Engine_Controller portEngine;
	public Engine_Controller middleEngine;
	public Engine_Controller starboardEngine;
	public bool hasPower = true;
	public Plasma_Cannon cannon;
	//public ParticleSystem plasmaParticles;
	public Light fireCannonLight;
	public Light chargeCannonLight;
	public Transform pilotPosition;

	public GameObject explosionPrefab;
	public GameObject subExplosionPrefab;

	public Rigidbody rb;

	public float steering = 0f;
	float thrust = 1E7f;
	public float throttle = 1f;
	public float vertical;
	public float exitWait;

	public bool shieldActive;
	//Carrier Bridge Systems
	public LineRenderer predictedPath;

	public Vector3 previousVelocity = Vector3.zero;
	public Vector3 acceleration;
	public GameObject pilot;

	Metwork_Object netObj;
	Metwork_Object playerNetObj;

	public bool threeAxis;
	public bool destroyAfterDeath;
	public float lastTime;

	public Light[] lights;
	public Animator leftGauge;
	public Animator centerGauge;
	public Animator rightGauge;
	bool reverse = false;

	public void Start(){
		netObj = this.GetComponent<Metwork_Object>();
		InvokeRepeating ("SendControls", 0f, 1f);


	}

	public void Activate(GameObject player){
		if (pilot != null) {
			return;
		}
		if (Time.time - lastTime < 2f) {
			return;
		}
		pilot = player;
		playerNetObj = player.GetComponent<Metwork_Object> ();
		player.GetComponent<Player_Controller> ().inVehicle = true;
		exitWait = Time.time + 2f;
		if (playerNetObj.isLocal) {
			predictedPath.enabled = true;
		}

	}
	public void Exit(){
		//exit over the network
		if (playerNetObj.isLocal && Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("RPC_Exit", MRPCMode.OthersBuffered, new object[]{  });
		}
		lastTime = Time.time;
		pilot.GetComponent<Player_Controller> ().inVehicle = false;
		pilot = null;
		playerNetObj = null;
		predictedPath.enabled = false;
		
	}
	[MRPC]
	void RPC_Exit()
	{
		pilot.GetComponent<Player_Controller>().inVehicle = false;
		pilot = null;
		playerNetObj = null;
	}

	public void SendControls(){
		if (pilot != null && playerNetObj.isLocal && !Metwork.isServer && Metwork.peerType != MetworkPeerType.Disconnected) {
			netObj.netView.RPC ("SetControls", MRPCMode.Server, new object[]{ throttle, steering });
		}
	}

	[MRPC]
	public void SetControls(float _throttle, float _steering){
		throttle = _throttle;
		steering = _steering;
	}

	void Update(){
		
		if (pilot != null && playerNetObj.isLocal)
		{

			DrawPredictedPath ();
		}
	}

	void FixedUpdate(){

		if (pilot != null && playerNetObj.isLocal)
		{
			pilot.GetComponent<Rigidbody>().velocity = rb.GetPointVelocity(pilotPosition.position);
			if (Time.frameCount % 3f == 0)
			{
				pilot.transform.forward = this.transform.forward;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				thrust = 1.5E7f;
			}
			else
			{
				thrust = 0.75E7f;
			}

			throttle = Mathf.Clamp(throttle + Input.GetAxis("Move Z") / 10f, -1f, 1f);
			steering = Mathf.Clamp(steering + Input.GetAxis("Move X") / 10f, -1f, 1f);
			if (threeAxis)
			{

				vertical = Input.GetAxis("Move Y");
			}
			if (Input.GetButtonDown("Use Item") && Time.time > exitWait)
			{
				Exit();
			}
			if (Input.GetButtonDown("Fire1"))
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_Fire", MRPCMode.AllBuffered, new object[] { });
				}
				else
				{
					RPC_Fire();
				}
			}
			if (Input.GetButtonDown("Jump"))
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_ChargeCannon", MRPCMode.AllBuffered, new object[] { });
				}
				else
				{
					RPC_ChargeCannon();
				}
			}

			//try {
			if (throttle > 0)
			{
				if (reverse)
				{
					reverse = false;
					leftGauge.GetComponent<MeshRenderer>().material.color = new Color(0.286f, 0.49f, 1f);
					centerGauge.GetComponent<MeshRenderer>().material.color = new Color(0.286f, 0.49f, 1f);
					rightGauge.GetComponent<MeshRenderer>().material.color = new Color(0.286f, 0.49f, 1f);
				}
				leftGauge.SetFloat("Throttle", (steering + throttle) * 0.5f);
				centerGauge.SetFloat("Throttle", throttle);
				rightGauge.SetFloat("Throttle", (throttle - steering) * 0.5f);
			}
			else
			{
				if (!reverse)
				{
					leftGauge.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.294f, 0.293f);
					centerGauge.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.294f, 0.293f);
					rightGauge.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.294f, 0.293f);

					reverse = true;
				}
				leftGauge.SetFloat("Throttle", (steering - throttle) * 0.5f);
				centerGauge.SetFloat("Throttle", -throttle);
				rightGauge.SetFloat("Throttle", (-throttle - steering) * 0.5f);
			}
			//} catch {

			//}

		}
		if (!Metwork.isServer && Metwork.peerType != MetworkPeerType.Disconnected) {
			return;
		}

		//Current number of engines firing
		float numEngines = 0f;
		//The engines that provide a rotational force
		float rotEngines = 0f;


		if (portEngine.isRunning) {
			//Add a force to the number 1 engine (Change to force at position)
			numEngines += 1f;
			rotEngines += 1f;
			//rb.AddRelativeTorque (thrust * (throttle + steering) * 100f * Time.smoothDeltaTime * 30f * Vector3.up);
		}

		if (middleEngine.isRunning) {
			//Add a force to the number 2 engine
			//rb.AddRelativeForce (throttle * thrust * Time.smoothDeltaTime * 30f * Vector3.forward);
			numEngines += 1f;
		}

		if (starboardEngine.isRunning) {
			//Add a force to the number 1 engine (Change to force at position)
			//rb.AddRelativeForce (thrust * throttle * Time.smoothDeltaTime * 30f * Vector3.forward);
			//rb.AddRelativeTorque (thrust * (-throttle + steering) * 100f * Time.smoothDeltaTime * 30f * Vector3.up);
			numEngines += 1f;
			rotEngines -= 1f;
		}
		if (threeAxis) {
			rb.AddRelativeForce (vertical * thrust * Time.smoothDeltaTime * 30f * Vector3.up);

		}

		//Here we simplify three forces and two moments to one force and one moment to make it easier on our physics engine
		rb.AddRelativeForce (thrust * throttle * numEngines * Time.smoothDeltaTime * 30f * Vector3.forward);
		rb.AddRelativeTorque (thrust * (rotEngines * throttle + steering) * 200f * Time.smoothDeltaTime * 30f * Vector3.up);




	}
	[MRPC]
	public void RPC_Fire(){
		cannon.fire = !cannon.fire;
		fireCannonLight.enabled = cannon.fire;
	}
	[MRPC]
	public void RPC_ChargeCannon(){
		cannon.charge = !cannon.charge;
		chargeCannonLight.enabled = cannon.charge;
	}

	public void OnDie(){
		StartCoroutine (Die ());
	}

	IEnumerator Die(){
		GameObject explosion;
		rb.AddExplosionForce (10000000f, rb.transform.position, 10000f);
		rb.constraints = RigidbodyConstraints.None;

		for (int i = -2; i < 4; i++) {
			explosion = (GameObject)Instantiate (explosionPrefab, this.transform.position + transform.forward * 100f * i, Quaternion.identity);
			explosion.transform.localScale = Vector3.one * 1E8f;
			Destroy (explosion, 3f);
			for (int j = -1; j < 2; j++) {
				explosion = (GameObject)Instantiate (subExplosionPrefab, this.transform.position + transform.forward * (100f * i + 10f * j), Quaternion.identity);
				explosion.transform.parent = this.transform;
				//explosion.transform.localScale = Vector3.one * 0.0001f;
				yield return new WaitForSeconds (0.05f);
			}

			yield return new WaitForSeconds (0.4f);
		}
		yield return new WaitForSeconds (1.5f);
		foreach (Collider collider in Physics.OverlapSphere(this.transform.position, 200f))
		{
			if (collider.GetComponent<Damage>() != null)
			{
				collider.GetComponent<Damage>().TakeDamage(100, 0, transform.position, true);
			}
		}
		if (destroyAfterDeath) {
			yield return new WaitForSeconds (4f);
			//Destroy (this.gameObject);
			gameObject.SetActive(false);
		}
		//gameObject.SetActive (false);
	}

	void DrawPredictedPath(){
        float startTime = 0f;
        float endTime = 10f;
        int timeSteps = 100;
        float stepWidth = (startTime - endTime) / timeSteps;

        float velocity = rb.velocity.magnitude;
        Vector3 rotVelocity = rb.angularVelocity;
        Vector3 previousPosition = predictedPath.transform.position;
        Vector3 currentForward = -rb.velocity;

        predictedPath.positionCount = timeSteps;
        predictedPath.SetPosition(0, previousPosition);

        for (int i = 1; i < timeSteps; i++)
        {
			
            float sine = Mathf.Sin(i * stepWidth * rotVelocity.y);
            float cosine = Mathf.Cos(i * stepWidth * rotVelocity.y);

            //Rotate the forward vector
            currentForward = new Vector3(currentForward.x * cosine - currentForward.z * sine, 0, (currentForward.x * sine + currentForward.z * cosine));

            predictedPath.SetPosition(i, predictedPath.GetPosition(i - 1) + (i * stepWidth * velocity * currentForward));
        }
    }
	public void ShutdownGravity(){
		print ("RPC invokes");
		this.GetComponentInChildren<Gravity_Controller>().useGravity = false;
		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = false;
		Time.timeScale = 0.5f;
		hasPower = false;
		Invoke ("IgnoreInternal", 1f);
		print ("Completed");

	}

	public void ReactivateGravity(){
		this.GetComponentInChildren<Gravity_Controller>().useGravity = true;
		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = false;
		//Time.timeScale = 0.5f;
		hasPower = true;

		Invoke ("IgnoreInternal", 1f);
	}
	#region shield
	public void ShutdownShield()
	{
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netObj.netView.RPC("RPC_ShutdownShield", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_ShutdownShield();
		}
	}
	[MRPC]
	public void RPC_ShutdownShield()
	{
		GetComponentInChildren<Poynting_Shield>().StartCoroutine("ShutdownShield");
	}

	public void ReactivateShield()
	{
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netObj.netView.RPC("RPC_ReactivateShield", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_ReactivateShield();
		}
	}
	[MRPC]
	public void RPC_ReactivateShield()
	{
		GetComponentInChildren<Poynting_Shield>().StartCoroutine("ReactivateShield");
	}
	#endregion
	void IgnoreInternal(){
		Time.timeScale = 1f;

		this.GetComponentInChildren<Gravity_Controller>().ignoreInternalObjects = true;

	}
	public void ShutdownPower(){
		print ("RPC invokes");
		StartCoroutine (TurnOffLights ());
		this.hasPower = false;
		print ("Completed");

	}

	IEnumerator TurnOffLights(){
		lights = transform.GetComponentsInChildren<Light> ();
		for (int i = 0; i<5; i++){
			lights [i].enabled = false;
		}
		yield return new WaitForSeconds (1f);
		for (int j = 5; j<15; j++){
			lights [j].enabled = false;
		}
		yield return new WaitForSeconds (2f);

		for (int k = 15; k<lights.Length; k++){
			lights [k].enabled = false;
		}
	}
	public void ReactivateLights(){
		print ("RPC invokes");
		this.hasPower = true;
		foreach (Light shipLight in lights) {
			shipLight.enabled = true;
		}
		print ("Completed");

	}
	public static void FlashWarningLights(bool _active, Transform carrier)
	{
		GameObject[] _lights = GameObject.FindGameObjectsWithTag("Warning Light");
		foreach (GameObject go in _lights)
		{
			if (carrier == null || go.transform.root == carrier)
			{
				Animator _anim = go.GetComponentInChildren<Animator>();
				if (_anim != null)
				{
					_anim.SetBool("Is Flashing", _active);
				}
			}
		}
	}


}

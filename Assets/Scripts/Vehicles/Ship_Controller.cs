using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/*Michael Gunther: 2018-02-05
 * Purpose: Ship class to be derived from to make the fighter ship, bombers etc
 * Notes: 
 * Improvements/Fixes:
 * */

public class Ship_Controller : Vehicle {

	
	[HideInInspector] public List<Vector3> route;
	[SerializeField] protected ShipMovementProperties moveFactors;

	protected float deltaThrustForce;


	public bool isAI = false;
	public bool isTransport = false;
	public Transform target;
	public AudioSource engineSound;
	protected float previousEnginePitch;
	//Engine Exhaust
	
	public Transform[] passengerSeats;
	bool holdPosition = false;
	Vector3 holdLocation;
	bool distanceNotified = false;

	 
	bool landMode = false;


	// Use this for initialization
	protected void Start () {
		rb = this.GetComponent<Rigidbody> ();
		anim = this.GetComponentInChildren<Animator> ();
		engineSound = this.GetComponent<AudioSource> ();
		InvokeRepeating ("FindDirection", 0f, 2f);
		InvokeRepeating ("CheckOccupied", 1f, 1f);
		damageScript = this.GetComponent<Damage> ();
		this.enabled = false;
		engineSound.pitch = Mathf.Clamp(Mathf.Abs(moveZ+moveX+moveY),0,0.1f) + (Time.frameCount % 5f)*0.003f  + 0.85f;
		previousEnginePitch = engineSound.pitch;

	}


	// Update is called once per frame
	protected void Update()
	{
		ShipUpdate();
	}
	protected virtual void ShipUpdate(){
		if (!hasAuthority) {
			return;
		}
		if (isAI) {
			AI ();
		} else {
			Fly ();
		}

		try{ SimulateParticles (); } catch{}
		moveY = Input.GetAxis ("Move Y");
		moveZ = Input.GetAxis ("Move Z");
		deltaThrustForce = Time.deltaTime * moveFactors.thrustToMassRatio;
		rb.velocity = Vector3.ClampMagnitude (rb.velocity, 1000f);
		rb.angularVelocity = Vector3.ClampMagnitude (rb.angularVelocity, 10f);


		if (Input.GetButton ("Fire1")) {
			Fire ();
		}
		if(Input.GetButtonDown("Use Item") && Time.time - getInTime > 1f){
			Exit ();
		}
		
	}


	protected virtual void Fly(){

		rb.AddRelativeForce (0f,moveY *deltaThrustForce* moveFactors.thrustFactor,
			moveZ *deltaThrustForce, ForceMode.Acceleration);
		
		if(rb.useGravity){
			if(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude < 300f){
				//Balancing
				//entering gravity, activate landing
				if (!landMode)
				{
					
					RPC_LandMode(true);
					
					landMode = true;

				}
				rb.AddForce(rb.mass * 9.81f * Mathf.Clamp(1f/Vector3.Dot(Vector3.up, transform.up), 0.1f, 1f)*transform.up * moveFactors.hoverForceFactor * Time.deltaTime);
				rb.AddTorque(Vector3.Cross(-transform.up,(transform.up - Vector3.up)*Vector3.Magnitude(transform.up - Vector3.up)*rb.mass*10f)*rb.mass/1000f);
			}
			else{
				rb.AddRelativeForce(rb.mass * 9.81f * Mathf.Clamp(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude * 0.002f,0, 1.5f) * moveFactors.hoverForceFactor * Time.deltaTime * Vector3.up);
			}
		}
		else
		{
			//exiting gravity, deactivate landing
			if (landMode)
			{
				
				RPC_LandMode(false);
				
				landMode = false;
			}
			rb.AddRelativeForce(0f, 0f, deltaThrustForce*0.4f);
		}

		engineSound.pitch = Mathf.Lerp(previousEnginePitch,Mathf.Clamp(Mathf.Abs(moveZ+moveX+moveY),0,0.1f) + (Time.frameCount % 5f)*0.003f  + 0.85f, 0.3f);
		previousEnginePitch = engineSound.pitch;
		if (MInput.useMouse)
		{
			rb.angularVelocity = transform.TransformDirection(MInput.GetMouseDelta("Mouse Y") * -moveFactors.pitchFactor * moveFactors.angularSpeed,
				MInput.GetMouseDelta("Mouse X") * moveFactors.yawFactor  * moveFactors.angularSpeed,
				Input.GetAxis("Move X") * moveFactors.rollFactor * -moveFactors.angularSpeed);
		}
		else
		{
			rb.angularVelocity = transform.TransformDirection(MInput.GetAxis("Rotate X") * moveFactors.angularSpeed,
				MInput.GetAxis("Rotate Y")  * moveFactors.angularSpeed,
				Input.GetAxis("Move X") * moveFactors.angularSpeed);
		}
	}
	public void RPC_LandMode(bool on)
	{
		if (on)
		{
			anim.SetBool("Land Mode", true);
			rb.angularDrag = 1f;
			rb.drag = 0.2f;
		}
		else
		{
			anim.SetBool("Land Mode", false);
			rb.angularDrag = 0.5f;
			rb.drag = 0.1f;
		}
	}


	protected void DisableThrusters()
	{
		for (int i = 0; i < bottomThrusters.Length; i++) {
			bottomThrusters [i].Stop();
		}

		for (int i = 0; i < rearThrusters.Length; i++) {
			rearThrusters [i].Stop ();
		}
	}

	protected void EnableThrusters()
	{
		try{
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].Play();
			}

			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].Play ();
			}

		}
		catch{
		}
	}

	[MRPC]
	public override void RPC_Exit()
	{
		base.RPC_Exit();
		DisableThrusters();
	}

	[MRPC]
	public override void RPC_Activate(Player_Controller _pilot)
	{
		base.RPC_Activate(_pilot);
		EnableThrusters();
	}

	protected void SimulateParticles(){
		if (!isLocalPlayer) {
			return;
		}
		if (moveY != 1f) {
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].emissionRate = 150f * (Mathf.Clamp (moveY, 0f, 1f) * 1f);
			}
		}
		if (moveZ != 1f) {
			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].emissionRate = 150f * (Mathf.Clamp (moveZ, 0.2f, 1f) * 1.3f);
			}
		}

	}

	[MRPC]
	void RPC_SimulateParticles(float _moveY, float _moveZ){


		if (_moveY != 1f) {
			for (int i = 0; i < bottomThrusters.Length; i++) {
				bottomThrusters [i].emissionRate = 150f * (Mathf.Clamp (_moveY, 0f, 1f) * 1f);
			}
		}
		if (_moveZ != 1f) {
			for (int i = 0; i < rearThrusters.Length; i++) {
				rearThrusters [i].emissionRate = 150f * (Mathf.Clamp (_moveZ, 0.2f, 1f) * 1.3f);
			}
		}
	}


	public override void DisableAI(){
		this.isAI = false;
		holdPosition = false;
		distanceNotified = true;
		this.target = null;
		Navigation.DeregisterTarget(this.transform);
	}

	void AI(){
		Vector3 nextPosition;
		if (route == null || route.Count <= 0) {
			if(GetComponent<Raycaster>().target != null){
				nextPosition = GetComponent<Raycaster> ().target;
			}
			else{
				return;
			}
		} else {
			Vector3 closest = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 middle = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 farthest = route [0];//new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);



			for (int i = 0; i < route.Count; i++) {
				if ((route [i] - this.transform.position).sqrMagnitude < (closest - this.transform.position).sqrMagnitude) {
					farthest = middle;
					middle = closest;
					closest = route [i];
				} else if ((route [i] - this.transform.position).sqrMagnitude < (middle - this.transform.position).sqrMagnitude) {
					farthest = middle;
					middle = route [i];
				} else if ((route [i] - this.transform.position).sqrMagnitude < (farthest - this.transform.position).sqrMagnitude) {
					farthest = route [i];
				}
			}


			if(route.Count > route.IndexOf (closest)+1){
				nextPosition = route [route.IndexOf (closest) + 1];
			}
			else{
				nextPosition = route [route.IndexOf (closest)];
			}
		}

		float angle = Vector2.SignedAngle (new Vector2 (transform.forward.x, transform.forward.z), new Vector2 ((nextPosition - transform.position).x, (nextPosition - transform.position).z));//Vector3.SignedAngle ((nextPosition-this.transform.position), transform.forward, Vector3.forward);


		float horizontal = Mathf.Clamp (angle / -100f, -1f, 1f) - Mathf.Clamp((rb.angularVelocity.y/ angle), -1f,1f);



		rb.angularVelocity = horizontal * 2f *  moveFactors.angularSpeed * Vector3.up;


		float anglePitch = Vector3.SignedAngle (nextPosition,this.transform.position, Vector3.right) + 90f;

		float gravityFactor = 0;
		if (rb.useGravity) {
			gravityFactor = 1f;
		}

		float vertical = Mathf.Clamp (angle / -100f, -1f, 1f);
		//rb.AddRelativeTorque(vertical * thrust * torqueFactor * Vector3.right);

		rb.AddForce (Mathf.Clamp((nextPosition - transform.position).y/90f, -1,1) * moveFactors.thrustToMassRatio * 2f * Vector3.up, ForceMode.Acceleration);


		float forward = 1f - (Mathf.Clamp(new Vector2(rb.velocity.x, rb.velocity.z).magnitude / (new Vector2((this.transform.position - nextPosition).x, (this.transform.position - nextPosition).z).magnitude), -1f,1f));

		//Slow down when approaching the target
		float slowFactor = 1f;
		if(Vector3.Distance(route[route.Count - 1],this.transform.position) < 15f){
			slowFactor = 0.3f;
			rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 0.05f);
			if(!distanceNotified){
				distanceNotified = true;
				WindowsVoice.Speak("Ten meters out.");
			}
		}
		rb.AddForce (slowFactor * forward * moveFactors.thrustToMassRatio*Time.deltaTime * 60f * (nextPosition - transform.position).normalized, ForceMode.Acceleration);
		rb.AddForce (slowFactor * gravityFactor * moveFactors.thrustToMassRatio * Vector3.up * Time.deltaTime * 45f, ForceMode.Acceleration);


		float upAngle = Vector2.SignedAngle (new Vector2 (transform.up.x, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (0f, 0f, upAngle*upAngle*Time.deltaTime * 60f*Mathf.Sign(upAngle)*0.1f);

		float rightAngle = Vector2.SignedAngle (new Vector2 (transform.up.z, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (rightAngle*rightAngle*Time.deltaTime * 60f*Mathf.Sign(rightAngle)*-0.1f,0f, 0f);

		

		rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 0.005f);

		/*if ((GetComponent<Raycaster>().target - this.transform.position).sqrMagnitude < 225f) {
			rb.velocity = Vector3.zero;
			isAI = false;
			this.enabled = false;
			Navigation.DeregisterTarget (this.transform);
		}*/


	}

	void FindDirection(){
		if (isAI == false) {
			return;
		}

		Raycaster caster = this.GetComponent<Raycaster> ();
		caster.origin = this.transform.position;
		if(Vector3.Distance(this.transform.position,target.position) <6f && !holdPosition){
			holdPosition = true;
			holdLocation = this.transform.position;
		}
		if(holdPosition){
			caster.target = holdLocation;
		}
		else{
			caster.target = target.position;
		}
		caster.resolution = Mathf.Ceil(Mathf.Max(new float[]{Mathf.Abs((caster.target - caster.origin).x),Mathf.Abs((caster.target - caster.origin).y),Mathf.Abs((caster.target - caster.origin).z)}) /4f);
		caster.Cast();

		//caster.Raycast (Vector3.zero, new Queue<Vector3>());
		caster.Visualize (caster.route.ToArray ());

		route = new List<Vector3>(caster.route); 

	}

	
	

	

	

}

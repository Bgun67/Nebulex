using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metwork_Object : MonoBehaviour {

	public Vector3 actualPosition;
	Quaternion actualRotation;
	Vector3 actualVelocity;
	Vector3 transV;

	[Tooltip("Does this object belong to this computer?")]
	public bool isLocal;
	[Tooltip("Used for linking and object on this computer with one on another computer")]
	public int netID;
	[Tooltip("Which player owns this object?")]
	public int owner;

	[HideInInspector]
	public Rigidbody rb;
	public MetworkView netView;
	//Metwork_Manager manager;

	//The time it takes to "snap" an object into the right position
	public float rotationLerp = 0.2f;
	public float snapLerp = 0.2f;
	public float lerpRate = 20f;
	public float snapDistance = 0f;
	public float velocitySnapDist = 0.5f;

	float forceFactor = 60f;
	public float rotForceFactor = 30f;

	public float velocityLerp = 0.2f;

	public float sendRate = 15f;
	bool isCollision = false;

	public bool isTwoPlane = false;


	public void Awake(){
		netView = GetComponent<MetworkView> ();
		//manager = GameObject.FindObjectOfType<Metwork_Manager> ();


		rb = this.GetComponent<Rigidbody> ();
		actualPosition = this.transform.position;
		InvokeRepeating("NetUpdate", 0f, Mathf.Clamp(1f / sendRate, 0, 20f));
		if ((Metwork.peerType == MetworkPeerType.Disconnected && this.owner == 1)){ // || (Metwork.peerType != MetworkPeerType.Disconnected && this.owner == manager.playerNumber)) {
			isLocal = true;
		} else
		{

			CheckLocal();
		}
		if (isLocal) {
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				netView.RPC ("RPC_SetLocation", MRPCMode.Others, new object[]{ this.transform.position, this.transform.rotation, rb.velocity});
			}
		}

		Invoke ("ResetPhysics", 0.5f);
	}
	public void CheckLocal()
	{
		
		print("Check" + this.transform.rotation.ToString(true));
		try
		{
			if (this.owner == Metwork.player.connectionID || (Metwork.isServer && this.owner == 0))
			{
				isLocal = true;
			}
			else
			{
				isLocal = false;
			}
		}
		catch
		{
			if (this.owner == 1 || this.owner == 0)
			{
				isLocal = true;
			}
			else
			{
				isLocal = false;
			}
		}
		if (isTwoPlane)
		{
			print("Local =" + isLocal);
		}
	}

	void OnCollisionEnter(Collision other){

		Metwork_Object otherNetObj = other.transform.root.GetComponent<Metwork_Object> ();
		if (otherNetObj != null) {
			if (otherNetObj.isLocal && !isTwoPlane) {
				//isCollision = true;
				Invoke ("ResetCollision",0.3f);
			}
		}
	}

	void ResetCollision(){
		isCollision = false;
	}

	void Update(){
		if(Time.frameCount % 10 == 0){
			CheckLocal();
		}
	}

	void FixedUpdate(){
		
		//if (manager == null && this.owner == 1) {
		//	isLocal = true;
		//	return;
		//} else if (manager == null) {
		//	return;
		//}

		//if (Metwork.peerType == MetworkPeerType.Disconnected) {
		//	return;
		//}
		//Physics.autoSimulation = true;

		

		if (!isLocal) {

			Vector3 positionDeviation = actualPosition - transform.position;
			float deviationSqrMagnitude = positionDeviation.sqrMagnitude;
			Vector3 transformVelocity = positionDeviation * lerpRate;


			//The object really should not be going this fast
			//If it is just teleport it
			if (deviationSqrMagnitude * lerpRate > 10000f) {
				if (!isTwoPlane) {
					//move the object
					rb.MovePosition(actualPosition);
					//clear the transform velocity
					transformVelocity = Vector3.zero;
				} else {
					//Physics.autoSimulation = false;
					//print ("Moving Carrier");
					//move the object
					rb.MovePosition(actualPosition);
					//clear the transform velocity
					transformVelocity = Vector3.zero;
					//Invoke ("ResetPhysics", 0.25f);
				}

			}


			if ( deviationSqrMagnitude >= snapDistance * snapDistance) {
				transV = transformVelocity;
				//rb.velocity = Vector3.ClampMagnitude (Vector3.Lerp (rb.velocity, actualVelocity + transformVelocity, velocityLerp), 1000f); //Vector3.Lerp (actualVelocity, actualVelocity + transformVelocity, 3f/deviationSqrMagnitude)
				rb.AddForce(Vector3.ClampMagnitude (transformVelocity, 1000f) * rb.mass * Time.fixedDeltaTime * forceFactor);
			}
			if (deviationSqrMagnitude < velocitySnapDist * velocitySnapDist) {

				rb.velocity = Vector3.Lerp(rb.velocity,actualVelocity, 1.0f);
			}


			if (isCollision) {
				//rb.AddForce (transformVelocity);

			} else {


				//"Snap" the object into place
				if (deviationSqrMagnitude < snapDistance * snapDistance && rb.velocity.sqrMagnitude < 1600f) {
				//	if (isTwoPlane) {
				//		print ("Snapping Carrier 2");
				//	}
					transform.position = Vector3.LerpUnclamped (transform.position, actualPosition, snapLerp);
					rb.velocity = actualVelocity;

				} else if (deviationSqrMagnitude < velocitySnapDist * velocitySnapDist) {
					rb.velocity = actualVelocity;
				}




			}

			Vector3 deltaRot = (Quaternion.Inverse(transform.rotation) * actualRotation).eulerAngles;


			if (deltaRot.x > 180f) {
				deltaRot.x = -360f + deltaRot.x;
			}
			if (deltaRot.y > 180f) {
				deltaRot.y = -360f + deltaRot.y;
			}
			if (deltaRot.z > 180f) {
				deltaRot.z = -360f + deltaRot.z;
			}


			if (!isTwoPlane) {
				rb.MoveRotation(Quaternion.LerpUnclamped (transform.rotation, actualRotation, rotationLerp));
			} else
			{
				//print("DeltaRot" + deltaRot);
				//print("Angular Velocity" + rb.angularVelocity);
				rb.MoveRotation(Quaternion.LerpUnclamped (transform.rotation, actualRotation, rotationLerp));

				//rb.angularVelocity = Vector3.up * Mathf.Clamp(deltaRot.y * rotationLerp, -10f, 10f);
			}

			actualPosition = actualPosition + actualVelocity * Time.fixedDeltaTime;
		}


	}

	[MRPC]
	public void RPC_SetLocation(Vector3 _position, Quaternion _rotation, Vector3 _velocity){
		actualPosition = _position;
		actualRotation = _rotation;
		actualVelocity = _velocity;

		rb.MovePosition(_position);
		rb.MoveRotation( _rotation);
		rb.velocity = _velocity;
		

		//	rb.angularVelocity = _angularVelocity;

	}

	void NetUpdate(){
		if (isLocal && Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("SendLocation", MRPCMode.Others, new object[] {
				this.transform.position,
				this.transform.rotation,
				this.rb.velocity,
				this.netID
			});
		}




	}

	void ResetPhysics(){
		Physics.autoSimulation = true;
	}

	//void OnServerInitialized(){
	//	Awake ();
	//}

	//public void OnConnectedToServer(){
	//	print ("Connected To Server");

	//	Awake ();

	//}

	[MRPC]
	public void SendLocation(Vector3 objectPosition, Quaternion objectRotation,Vector3 objectVelocity, int id){
		if (!isLocal)
		{
			actualPosition = objectPosition;
			actualRotation = objectRotation;
			actualVelocity = objectVelocity;
			
		}

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Network_Object : MonoBehaviour {
	/*
	public Vector3 actualPosition;
	Quaternion actualRotation;
	Vector3 actualVelocity;

	[Tooltip("Does this object belong to this computer?")]
	public bool isLocal;
	[Tooltip("Used for linking and object on this computer with one on another computer")]
	public int netID;
	[Tooltip("Which player owns this object?")]
	public int owner;

	[HideInInspector]
	public Rigidbody rb;
	public NetworkView netView;
	Network_Manager manager;

	//The time it takes to "snap" an object into the right position
	public float rotationLerp = 0.2f;
	public float snapLerp = 0.2f;
	public float lerpRate = 20f;
	public float snapDistance = 2f;
	public float velocitySnapDist = 2f;

	float forceFactor = 30f;
	public float rotForceFactor = 30f;

	public float velocityLerp = 0.2f;

	public float sendRate = 10f;
	bool isCollision = false;

	public bool isTwoPlane = false;


	void Awake(){
		netView = GetComponent<NetworkView> ();
		manager = GameObject.FindObjectOfType<Network_Manager> ();


		rb = this.GetComponent<Rigidbody> ();
		actualPosition = this.transform.position;
		InvokeRepeating("NetUpdate", 0f, Mathf.Clamp(1 / sendRate, 0, 20f));
		if ((Network.peerType == NetworkPeerType.Disconnected && this.owner == 1)  || (Network.peerType != NetworkPeerType.Disconnected && this.owner == manager.playerNumber)) {
			isLocal = true;
		} else {
			isLocal = false;
		}
		if (isLocal) {
			if (Network.peerType != NetworkPeerType.Disconnected) {
				netView.RPC ("RPC_SetLocation", RPCMode.Others, new object[]{ this.transform.position, this.transform.rotation, rb.velocity});
			}
		}

	}

	void OnCollisionEnter(Collision other){
		
		Network_Object otherNetObj = other.transform.root.GetComponent<Network_Object> ();
		if (otherNetObj != null) {
			if (otherNetObj.isLocal && !isTwoPlane) {
				isCollision = true;
				Invoke ("ResetCollision",0.3f);
			}
		}
	}

	void ResetCollision(){
		isCollision = false;
	}

	void Update(){
		if (manager == null && this.owner == 1) {
			isLocal = true;
			return;
		} else if (manager == null) {
			return;
		}

		if (this.owner == manager.playerNumber) {
			isLocal = true;
		} else {
			isLocal = false;
		}

		if (!isLocal) {
			
			Vector3 positionDeviation = actualPosition - transform.position;
			float deviationSqrMagnitude = positionDeviation.sqrMagnitude;
			Vector3 transformVelocity = positionDeviation * lerpRate;


			//The object really should not be going this fast
			//If it is just teleport it
			if (deviationSqrMagnitude * lerpRate > 100000f) {
				if (!isTwoPlane) {
					//move the object
					rb.transform.position = actualPosition;
					//clear the transform velocity
					transformVelocity = Vector3.zero;
				} else {
					Physics.autoSimulation = false;
					print ("Moving Carrier");
					//move the object
					rb.transform.position = actualPosition;
					//clear the transform velocity
					transformVelocity = Vector3.zero;
					Invoke ("ResetPhysics", 0.25f);
				}

			}

			if ( deviationSqrMagnitude >= snapDistance * snapDistance) {
				//rb.velocity = Vector3.ClampMagnitude (Vector3.Lerp (rb.velocity, actualVelocity + transformVelocity, velocityLerp), 1000f);
				rb.AddForce(Vector3.ClampMagnitude (Vector3.Lerp (rb.velocity, actualVelocity + transformVelocity, velocityLerp), 1000f) * rb.mass * Time.smoothDeltaTime * forceFactor);
			}
			else if (deviationSqrMagnitude < velocitySnapDist * velocitySnapDist) {
				
					rb.velocity = actualVelocity;
			}



			if (isCollision) {
				//rb.AddForce (transformVelocity);

			} else {
				

				//"Snap" the object into place
				if (deviationSqrMagnitude < snapDistance * snapDistance && rb.velocity.sqrMagnitude < 1600f) {
					if (isTwoPlane) {
						print ("Snapping Carrier 2");
					}
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
				transform.rotation = Quaternion.LerpUnclamped (transform.rotation, actualRotation, rotationLerp);
			} else {
				
				rb.AddTorque (Mathf.Clamp(deltaRot.y/2f,-6f,6f) * rotationLerp * rb.mass * Time.deltaTime * rotForceFactor * Vector3.up);
			}

			actualPosition = actualPosition + actualVelocity * Time.deltaTime;
		}


	}

	[RPC]
	public void RPC_SetLocation(Vector3 _position, Quaternion _rotation, Vector3 _velocity){
		actualPosition = _position;
		actualRotation = _rotation;
		actualVelocity = _velocity;

		rb.position = _position;
		rb.rotation = _rotation;
		rb.velocity = _velocity;
	}

	void NetUpdate(){

			if (isLocal && Network.peerType != NetworkPeerType.Disconnected) {
				
				netView.RPC ("SendLocation", RPCMode.Others, new object[] {
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

	void OnServerInitialized(){
		Awake ();
	}

	public void OnConnectedToServer(){
		print ("Connected To Server");

		Awake ();

	}

	[RPC]
	public void SendLocation(Vector3 objectPosition, Quaternion objectRotation,Vector3 objectVelocity, int id){
		if (!isLocal){
			actualPosition = objectPosition;
			actualRotation = objectRotation;
			actualVelocity = objectVelocity;
		}

	}
    */




}

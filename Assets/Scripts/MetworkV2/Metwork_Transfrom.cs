using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metwork_Transfrom : MonoBehaviour
{
    public Vector3 actualPosition;
	Quaternion actualRotation;
	
    float predictiveLerp;


	[Tooltip("Does this object belong to this computer?")]
	public bool isLocal;
	[Tooltip("Which player owns this object?")]
	public int owner;

	public MetworkView netView;
	//Metwork_Manager manager;

	//The time it takes to "snap" an object into the right position
	public float rotationLerp = 0.2f;
	public float snapLerp = 0.2f;
	public float lerpRate = 20f;

	public float sendRate = 15f;



	public void Awake(){
		netView = GetComponent<MetworkView> ();

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
				netView.RPC ("RPC_SetLocation", MRPCMode.Others, new object[]{ this.transform.position, this.transform.rotation});
			}
		}
	}
	public void CheckLocal()
	{
		
		
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
	}


	void Update(){
		if(Time.frameCount % 10 == 0){
			CheckLocal();
		}
	}

	void FixedUpdate(){

		if (!isLocal) {

			Vector3 positionDeviation = actualPosition - transform.position;
			float deviationSqrMagnitude = positionDeviation.sqrMagnitude;
			Vector3 transformVelocity = positionDeviation * lerpRate;


			//The object really should not be going this fast
			//If it is just teleport it
			if (deviationSqrMagnitude * lerpRate > 10000f) {
				//move the object
				this.transform.position = actualPosition;
				//clear the transform velocity
				transformVelocity = Vector3.zero;
				

			}


			transform.position = Vector3.LerpUnclamped (transform.position, actualPosition, snapLerp);
			




			

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


			
			transform.rotation = (Quaternion.LerpUnclamped (transform.rotation, actualRotation, rotationLerp));

            predictiveLerp += Time.fixedDeltaTime * sendRate * 0.7f;
			//actualPosition = actualPosition + actualVelocity * Time.fixedDeltaTime;
		}


	}

	[MRPC]
	public void RPC_SetLocation(Vector3 _position, Quaternion _rotation){
		actualPosition = _position;
		actualRotation = _rotation;
        predictiveLerp = snapLerp;

		this.transform.position = _position;
		this.transform.rotation = _rotation;

	}

	void NetUpdate(){
		if (isLocal && Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("SendLocation", MRPCMode.Others, new object[] {
				this.transform.position,
				this.transform.rotation
			});
		}
	}

	

	[MRPC]
	public void SendLocation(Vector3 objectPosition, Quaternion objectRotation){
		if (!isLocal)
		{
			actualPosition = objectPosition;
			actualRotation = objectRotation;
            predictiveLerp = snapLerp;
			
		}

	}
}

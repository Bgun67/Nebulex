using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {
	public WheelCollider[] wheels;
	public Transform FRWheelMesh;
	public Transform FLWheelMesh;
	public Rigidbody rb;
	public Transform centerOfMass;
	public float force  = 100f;
	public float maxSpeed =35f;
	public float speed;
	public ParticleSystem dust;
	public ParticleSystem.MainModule main;
	public Passenger_Enter driversSeat;
	public Metwork_Object netObj;
	public GameObject explosionEffect;
	public GameObject[] destroyedPrefabs;
	public AudioWrapper audioWrapper;
	public AudioSource idleSound;
	Animator anim;
	public float lastOccupiedTime = 0f;

	public GameObject underbodyLight;
	public GameObject headlight;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		netObj = GetComponent<Metwork_Object>();
		rb.centerOfMass = centerOfMass.localPosition;
		main = dust.main;
		anim = GetComponent<Animator>();
	}
	public void Activate(GameObject _player)
	{
		anim = GetComponent<Animator>();

		anim.SetBool("LightsOn", true);
		netObj = GetComponent<Metwork_Object>();
		headlight.SetActive(true);

		underbodyLight.SetActive(false);
		driversSeat.Activate(_player);
		netObj.owner = _player.GetComponent<Metwork_Object>().netID;
		this.GetComponent<Damage>().healthShown = true;
	}
	public void Exit()
	{
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			if (driversSeat.player != null) {
				netObj.netView.RPC ("RPC_Exit", MRPCMode.AllBuffered, new object[]{ driversSeat.player.GetComponent<Metwork_Object> ().netID });
			} else {
				netObj.netView.RPC ("RPC_Exit", MRPCMode.AllBuffered, new object[]{0});
			}
		}
		else{
			if (driversSeat.player != null) {
				RPC_Exit (driversSeat.player.GetComponent<Metwork_Object> ().netID);
			} else {
				RPC_Exit (0);
			}
		}
	}
	public void RPC_Exit(int id)
	{
		print("Exiting Shredder");
		anim.SetBool("LightsOn", false);
		headlight.SetActive(false);
		underbodyLight.SetActive(true);
	}
	// Update is called once per frame
	void Update () {
		//if(speed > 0.1f){
		
		//}
		/*wheels [0].motorTorque = force * (Input.GetAxis ("Move Z"));//+2f*Input.GetAxis("Move X"));
		wheels [1].motorTorque = force * (Input.GetAxis ("Move Z"));//-2f*Input.GetAxis("Move X"));
		print ("Wheel1: "+wheels [0].motorTorque);
		print ("Wheel2: "+wheels [1].motorTorque);

		wheels[2].motorTorque = force*Input.GetAxis("Move Z");
*/
		if(driversSeat.player != null){
			if(!idleSound.isPlaying){
				idleSound.Play();
				idleSound.loop = true;
			}
			lastOccupiedTime = Time.time;
			
		}
		else{
			this.GetComponent<Damage>().healthShown = false;
			if(idleSound.isPlaying){
				idleSound.loop = false;
			}
			
			if(Vector3.Distance(this.GetComponent<Damage>().initialPosition.position,this.transform.position) > 4f){
				if(Time.time - lastOccupiedTime > 10f && Time.frameCount % 10 == 0){
					this.GetComponent<Damage>().TakeDamage(10, 0, Vector3.zero, true);
				}
			}
			else{
				lastOccupiedTime = Time.time;
			}

		}
		if (driversSeat.player == null||(Metwork.peerType!=MetworkPeerType.Disconnected&&!netObj.isLocal))
		{
			return;
		}
		audioWrapper.PlayOneShot(0, Input.GetAxis("Move Z"));
		Vector3 relativeSpeed = transform.InverseTransformDirection(rb.velocity);
		speed = transform.InverseTransformDirection(rb.velocity).z;
		//rb.velocity = transform.TransformDirection(new Vector3(relativeSpeed.x, relativeSpeed.y,Mathf.Clamp(speed, -maxSpeed, maxSpeed)));

		foreach (WheelCollider wheel in wheels)
		{
			if (speed < maxSpeed)
			{
				wheel.motorTorque = force * Input.GetAxis("Move Z");
			}
			else
			{
				wheel.motorTorque = 0;
			}
		}

		float steerAngle = 30f*Input.GetAxis("Move X");
		wheels[0].steerAngle = steerAngle;
		wheels[1].steerAngle = steerAngle;

		Quaternion wheelRotation1;
		Quaternion wheelRotation2;
		Vector3 wheelPosition1;
		Vector3 wheelPosition2;
		wheels[0].GetWorldPose(out wheelPosition1, out wheelRotation1);
		wheels[1].GetWorldPose(out wheelPosition2, out wheelRotation2);

		FRWheelMesh.rotation = wheelRotation1;
		FLWheelMesh.rotation = wheelRotation2;
		FRWheelMesh.localPosition = Vector3.Lerp(FRWheelMesh.localPosition,transform.InverseTransformPoint(wheelPosition1),0.3f);
		FLWheelMesh.localPosition = Vector3.Lerp(FLWheelMesh.localPosition,transform.InverseTransformPoint(wheelPosition2),0.3f);

		rb.AddRelativeForce(-transform.up * speed*0.5f * rb.mass);
		WheelHit hit;
		if (wheels[2].GetGroundHit(out hit))
		{
			//main.startColor = hit.collider.GetComponent<MeshRenderer>().material.GetColor("_Color");
			//main.startColor = Color.red;
			//print (main.startColor);
		}
	}
	public void Die(){
		print("dying");
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			if (driversSeat.player != null) {
				netObj.netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{ driversSeat.player.GetComponent<Metwork_Object> ().netID });
			} else {
				netObj.netView.RPC ("RPC_Die", MRPCMode.AllBuffered, new object[]{0});
			}
		}
		else{
			if (driversSeat.player != null) {
				RPC_Die (driversSeat.player.GetComponent<Metwork_Object> ().netID);
			} else {
				RPC_Die (0);
			}
		}
		if (driversSeat.player != null)
		{
			driversSeat.player.GetComponent<Damage>().TakeDamage(1000, 0, transform.position, true);
		}
	}

	//This function should essentially make the ship "like new"
	[MRPC]
	public void RPC_Die(int id){
		Destroy(Instantiate (explosionEffect, this.transform.position, transform.rotation),5f);
		try{
			Destroy(Instantiate (destroyedPrefabs [Random.Range (0, destroyedPrefabs.Length)], this.transform.position, transform.rotation),5f);
		}
		catch{}
		underbodyLight.SetActive(true);
		headlight.SetActive(false);
		this.GetComponent<Damage> ().Reset();
		this.enabled = false;
	}
}

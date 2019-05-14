using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {
	public WheelCollider[] wheels;
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
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		netObj = GetComponent<Metwork_Object>();
		rb.centerOfMass = centerOfMass.localPosition;
		main = dust.main;
	}
	public void Activate(GameObject _player)
	{
		driversSeat.Activate(_player);
		netObj.owner = _player.GetComponent<Metwork_Object>().netID;
	}
	void Exit()
	{
		
	}
	// Update is called once per frame
	void Update () {
		/*wheels [0].motorTorque = force * (Input.GetAxis ("Move Z"));//+2f*Input.GetAxis("Move X"));
		wheels [1].motorTorque = force * (Input.GetAxis ("Move Z"));//-2f*Input.GetAxis("Move X"));
		print ("Wheel1: "+wheels [0].motorTorque);
		print ("Wheel2: "+wheels [1].motorTorque);

		wheels[2].motorTorque = force*Input.GetAxis("Move Z");
*/
		if (driversSeat.player == null||(Metwork.peerType!=MetworkPeerType.Disconnected&&!netObj.isLocal))
		{
			return;
		}
		Vector3 relativeSpeed = transform.InverseTransformDirection(rb.velocity);
		speed = transform.InverseTransformDirection(rb.velocity).z;
		//rb.velocity = transform.TransformDirection(new Vector3(relativeSpeed.x, relativeSpeed.y,Mathf.Clamp(speed, -maxSpeed, maxSpeed)));
		if (speed < maxSpeed)
		{
			foreach (WheelCollider wheel in wheels)
			{
				wheel.motorTorque = force * Input.GetAxis("Move Z");
			}
		}
		wheels[0].steerAngle = 20f*Input.GetAxis("Move X");
		wheels[1].steerAngle = 20f*Input.GetAxis("Move X");
		rb.AddRelativeForce(Vector3.down * speed*0.5f * rb.mass);
		WheelHit hit;
		if (wheels[2].GetGroundHit(out hit))
		{
			//main.startColor = hit.collider.GetComponent<MeshRenderer>().material.GetColor("_Color");
			//main.startColor = Color.red;
			//print (main.startColor);
		}
		//rb.AddRelativeTorque(0f,40f*rb.mass*Input.GetAxis("Move X"),0f);//-2.8f*rb.mass*Input.GetAxis("Move X")*(Mathf.Abs(speed)+1f));
		//transform.Rotate(0f,2f*Input.GetAxis("Move X"), 0f);


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
		Destroy(Instantiate (destroyedPrefabs [Random.Range (0, destroyedPrefabs.Length)], this.transform.position, transform.rotation),5f);
		this.GetComponent<Damage> ().Reset();
		this.enabled = false;
	}
}

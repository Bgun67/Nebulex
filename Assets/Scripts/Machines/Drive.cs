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
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.centerOfMass = centerOfMass.localPosition;
		main = dust.main;
	}
	void Activate()
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
		Vector3 relativeSpeed = transform.InverseTransformDirection(rb.velocity);
		speed = transform.InverseTransformDirection(rb.velocity).z;
		rb.velocity = transform.TransformDirection(new Vector3(relativeSpeed.x, relativeSpeed.y,Mathf.Clamp(speed, -maxSpeed, maxSpeed)));
		foreach (WheelCollider wheel in wheels) {
			wheel.motorTorque = force * Input.GetAxis("Move Z");
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {
	public WheelCollider[] wheels;
	public Rigidbody rb;
	public Transform centerOfMass;
	public float force  = 100f;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.centerOfMass = centerOfMass.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		/*wheels [0].motorTorque = force * (Input.GetAxis ("Move Z"));//+2f*Input.GetAxis("Move X"));
		wheels [1].motorTorque = force * (Input.GetAxis ("Move Z"));//-2f*Input.GetAxis("Move X"));
		print ("Wheel1: "+wheels [0].motorTorque);
		print ("Wheel2: "+wheels [1].motorTorque);

		wheels[2].motorTorque = force*Input.GetAxis("Move Z");
*/
		foreach (WheelCollider wheel in wheels) {
			wheel.motorTorque = force*Input.GetAxis("Move Z");
		}


		//rb.AddRelativeTorque(0, 0f,2000f*Input.GetAxis("Move X")+-2000f*Input.GetAxis("Move Z")*Input.GetAxis("Move X"));
		transform.Rotate(0f,2f*Input.GetAxis("Move X"), 0f);
		if (Input.GetKey ("g")) {

			Application.LoadLevel (Application.loadedLevel);
		}

	}
}

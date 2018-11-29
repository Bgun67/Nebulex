using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon_Gun : MonoBehaviour {
	public GameObject harpoon;
	public LineRenderer wire;
	public Transform shotSpawn;
	public bool fired;
	public bool hit;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (fired == true) {
			if (Vector3.SqrMagnitude (harpoon.transform.position - shotSpawn.transform.position) > 200000f) {
				BreakWire ();
			}
			if (hit) {

				wire.SetPosition (0, shotSpawn.position);
				wire.SetPosition (1, harpoon.transform.position);


			}
		} else {

			if (Input.GetButton ("Fire1")) {
				Fire ();
			}
		}
	}
	void Fire(){
		if (fired == false) {
			harpoon.GetComponent<Rigidbody> ().isKinematic = false;
			harpoon.transform.position = shotSpawn.transform.position;
			harpoon.GetComponent<Rigidbody> ().velocity = shotSpawn.forward * 75f;
			harpoon.GetComponent<Harpoon>().trailRenderer.time = 20f;
			harpoon.GetComponent<Harpoon> ().hit = false;

			wire.enabled = true;
			fired = true;
		}
	}
	void BreakWire(){
		fired = false;
		wire.enabled = false;
		harpoon.GetComponent<Rigidbody> ().isKinematic = true;
		harpoon.transform.parent = null;

		harpoon.GetComponent<Harpoon>().trailRenderer.time = 0f;
		harpoon.transform.position = shotSpawn.transform.position;
	}
	public void Join(Transform otherParent){
		this.GetComponent<ConfigurableJoint> ().connectedAnchor = otherParent.transform.localPosition;
		this.GetComponent<ConfigurableJoint> ().anchor = this.GetComponent<Rigidbody>().centerOfMass;
		this.GetComponent<ConfigurableJoint> ().connectedBody = otherParent.GetComponent<Rigidbody> ();
		//this.GetComponent<ConfigurableJoint> ().linearLimit.limit = Vector3.Magnitude(this.transform.position - otherParent.transform.position);

	}
}

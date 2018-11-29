using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droid_Controller : MonoBehaviour {
	public Rigidbody rb;
	public List<Vector3> route = new List<Vector3>();
	public float thrust;
	public float torqueFactor;
	public bool isAI = true;
	public Transform target;
	public Animator anim;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("FindDirection", 1f, 3f);
	}
	
	// Update is called once per frame
	void Update () {
		AI ();
		float rollSpeed = this.transform.InverseTransformVector (rb.velocity).z;
		anim.SetFloat ("Roll Speed", rollSpeed);
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
			Vector3 closest = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 middle = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 farthest = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);



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



			nextPosition = route [route.IndexOf (closest) + 1];
		}

		float angle = Vector2.SignedAngle (new Vector2 (transform.forward.x, transform.forward.z), new Vector2 ((nextPosition - transform.position).x, (nextPosition - transform.position).z));//Vector3.SignedAngle ((nextPosition-this.transform.position), transform.forward, Vector3.forward);


		float horizontal = Mathf.Clamp (angle / -100f, -1f, 1f) - Mathf.Clamp((rb.angularVelocity.y/ angle), -1f,1f);



		rb.AddRelativeTorque(horizontal * thrust * 2f *  torqueFactor * Vector3.up);



		float gravityFactor = 0;
		if (rb.useGravity) {
			gravityFactor = 1f;
		}

		//rb.AddRelativeTorque(vertical * thrust * torqueFactor * Vector3.right);

		rb.AddForce (Mathf.Clamp((nextPosition - transform.position).y/90f, -1,1) * thrust * 2f * Vector3.up);



		float forward = 1f - (Mathf.Clamp(new Vector2(rb.velocity.x, rb.velocity.z).magnitude / (new Vector2((this.transform.position - nextPosition).x, (this.transform.position - nextPosition).z).magnitude), -1f,1f));
		//print (forward);
		rb.AddForce (forward * thrust * (nextPosition - transform.position).normalized);
		rb.AddForce (gravityFactor * thrust * Vector3.up * Time.deltaTime * 45f);


		float upAngle = Vector2.SignedAngle (new Vector2 (transform.up.x, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (0f, 0f, upAngle*upAngle*Mathf.Sign(upAngle)*0.1f);

		float rightAngle = Vector2.SignedAngle (new Vector2 (transform.up.z, transform.up.y), Vector2.up);
		rb.AddRelativeTorque (rightAngle*rightAngle*Mathf.Sign(rightAngle)*-0.1f,0f, 0f);


		rb.velocity = Vector3.Lerp (rb.velocity, Vector3.zero, 0.005f);

		if ((GetComponent<Raycaster>().target - this.transform.position).sqrMagnitude < 25f) {
			rb.velocity = Vector3.zero;

		}



	}

	void FindDirection(){
		if (isAI == false) {
			return;
		}

		Raycaster caster = this.GetComponent<Raycaster> ();
		caster.origin = this.transform.position;
		caster.target = target.position;
		caster.resolution = Mathf.Ceil(Mathf.Max(new float[]{Mathf.Abs((caster.target - caster.origin).x),Mathf.Abs((caster.target - caster.origin).y),Mathf.Abs((caster.target - caster.origin).z)}) /4f);
		caster.Cast();

		//caster.Raycast (Vector3.zero, new Queue<Vector3>());
		//caster.Visualize (caster.route.ToArray ());

		route = new List<Vector3>(caster.route); 

	}
}

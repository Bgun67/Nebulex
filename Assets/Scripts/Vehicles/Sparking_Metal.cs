using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparking_Metal : MonoBehaviour {

	public GameObject sparksPrefab;

	GameObject tmpSparks;

	Collision flagCollision;
	static Queue<GameObject> sparks = new Queue<GameObject>(20);

	static int maxSparks = 20;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("CustomUpdate", 0f, 0.1f);
	}
	
	// Update is called once per frame
	void CustomUpdate () {
		if (flagCollision == null) {
			return;
		}

		for (int i = 0; i < flagCollision.contacts.Length && i <= 3; i++) {
			if (sparks.Count < maxSparks) {
				tmpSparks = Instantiate (this.sparksPrefab, flagCollision.contacts [i].point, Quaternion.identity);
				tmpSparks.hideFlags = HideFlags.HideInHierarchy;
				sparks.Enqueue (tmpSparks);
			} else {
				tmpSparks = sparks.Peek ();
				if (tmpSparks.GetComponent<ParticleSystem> ().isEmitting) {
					break;
				}

				tmpSparks = sparks.Dequeue();
				tmpSparks.SetActive(false);
				tmpSparks.transform.position = flagCollision.contacts [i].point;
				tmpSparks.SetActive (true);
				sparks.Enqueue (tmpSparks);

			}

		}
		flagCollision = null;

	}


	void OnCollisionEnter(Collision other){
		if (other.relativeVelocity.sqrMagnitude > 100f) {
			flagCollision = other;
		}
	}
}

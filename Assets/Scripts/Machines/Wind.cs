using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour {
	public float windSpeed;
	public float windForce;

	ParticleSystem windSystem;
	WindZone windZone;
	public float windIncrement;
	public float maxWindSpeed;
	// Use this for initialization
	void Start(){
		windZone = this.GetComponent<WindZone> ();
		windSystem = this.GetComponent<ParticleSystem> ();
	}
	public void Activate () {
		if (!windSystem.isPlaying) {
			windSystem.Play ();
		}
		if (windSpeed < maxWindSpeed) {
			windSpeed += windIncrement;
		} else {
			windSpeed = 0f;
		}
		StartCoroutine(UpdateWindSpeed ());

	}
	IEnumerator UpdateWindSpeed(){
		
		while (windForce != windSpeed) {
			if (windForce > windSpeed) {
				windForce -= 5f;
				windZone.windMain -= 5f;
			} else {
				windForce += 5f;
				windZone.windMain += 5f;

			}
			yield return new WaitForSeconds (0.1f);

		}

		if (windSpeed == 0f) {
			windSystem.Stop ();

		} 

	}
	
	void OnTriggerStay (Collider other) {

		if (other.transform.root.tag == "Player") {
			other.transform.root.GetComponent<Rigidbody> ().AddForce (transform.forward * windForce);
		}
	}

}

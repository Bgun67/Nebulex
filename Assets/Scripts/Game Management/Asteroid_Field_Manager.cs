using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Asteroid_Field_Manager : MonoBehaviour {
	

	public float worldSize;
	public float maxAsteroidSize;
	public float maxMass;
	public float maxAngularVelocity;
	public float maxVelocity;
	public float launchTime = 2.5f;


	//Move ui to GameController
	public GameObject warningUI;
	public Image warningPanel;
	public bool warningActive;
	public bool AIncreasing;
	public float exitTime;
	public Transform carrierOne;
	public Transform carrierTwo;
	public float boundingSphereThickness;


	// Use this for initialization
	void Start () {
		

	}

	// Update is called once per frame
	void Update () {

	}
	/*public void OnTriggerExit(Collider other){
		if (other.tag == "Asteroid") {
			Relaunch (other.gameObject);
		} else if (other.tag=="Player"){
			warningUI.SetActive (true);
			warningActive = true;
			StartCoroutine(FlashWarning ());
		}

	}
	public void OnTriggerEnter(Collider other){
		 if (other.tag=="Player"){
			warningUI.SetActive (false);
			warningActive = false;
			exitTime = 0;
		}

	}*/
/*	public void LaunchAsteroids(){
		if (asteroids <= maxInnerAsteroids) {
			GameObject prefab = asteroidPrefabs[Random.Range(0,asteroidPrefabs.Length-1)];
			Vector3 center = carrierOne.position + carrierTwo.position / 2f;
			Vector3 position = new Vector3(0f,0f,0f);
			float worldSize = Vector3.Distance (carrierOne.position, center) + boundingSphereThickness;

			for (int i = 0; i<5; i++) {
				position = center+Random.insideUnitSphere * worldSize;
				if (Physics.OverlapBox (position, Vector3.one * maxAsteroidSize / 2f).Length==0) {
					break;
				}

			}
			GameObject asteroid = GameObject.Instantiate (prefab, position, Quaternion.identity);
			asteroid.transform.localScale = Vector3.one*Random.Range (minAsteroidSize, maxAsteroidSize);
			Rigidbody rb = asteroid.GetComponent<Rigidbody> ();
			rb.angularVelocity =  Random.insideUnitSphere * maxAngularVelocity;
			rb.velocity =  Random.insideUnitSphere * maxVelocity;
			rb.mass = Random.Range (minMass, maxMass);
			Asteroid_Controller controller =asteroid.GetComponent<Asteroid_Controller> ();
			controller.boundingSphereThickness = boundingSphereThickness;
			controller.carrierOne = carrierOne;
			controller.carrierTwo = carrierTwo;


			asteroids++;

		}

	}
	[MRPC]
	public void RPC_InstantiateAsteroid(){

	}
	public IEnumerator FillOuterSurface(){
		
		while (outerAsteroids < maxOuterAsteroids) {
			GameObject prefab = asteroidPrefabs[Random.Range(0,asteroidPrefabs.Length-1)];
			Vector3 position;
			position = Random.onUnitSphere * worldSize;
			GameObject asteroid = GameObject.Instantiate (prefab, position, Quaternion.identity);
			asteroid.transform.localScale = Vector3.one*Random.Range (minAsteroidSize, maxAsteroidSize);
			Rigidbody rb = asteroid.GetComponent<Rigidbody> ();
			rb.angularVelocity =  Random.insideUnitSphere * maxAngularVelocity;
			rb.mass = Random.Range (minMass, maxMass);

			outerAsteroids++;
			yield return new WaitForSeconds (launchTime);

		}

	}*/
	public void Relaunch(GameObject asteroid){
		Vector3 center = carrierOne.position + carrierTwo.position / 2f;
		Vector3 position = new Vector3 (1000f, 1000f, 1000f);
		float worldSize = Vector3.Distance (carrierOne.position, center) + boundingSphereThickness;

		for (int i = 0; i < 5; i++) {
		position = center + Random.onUnitSphere * worldSize;
			position.y = 0f;
			if (Physics.CheckSphere (position, maxAsteroidSize / 2f)) {
				break;
			}

		}
		asteroid.transform.position = position;
		Rigidbody rb = asteroid.GetComponent<Rigidbody> ();
		float sizeSeed = Random.Range (0.1f, 1f);
		float size = sizeSeed*maxAsteroidSize;
		asteroid.transform.localScale = Vector3.one * size;
		
		float mass = sizeSeed*maxMass;
		rb.mass = mass;
	rb.AddForce (Random.insideUnitSphere * maxVelocity + (center - position)*maxVelocity);
		rb.AddRelativeTorque (Random.insideUnitSphere * maxAngularVelocity);
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			asteroid.GetComponent<MetworkView> ().RPC ("RPC_Relaunch", MRPCMode.Others, new object[]{ size,  mass });
		} else {

		}

	}
public void RelaunchJunk(GameObject piece){
		float carrierNum = Random.value;
		Vector3 center;
		if (carrierNum <= 0.5f) {
			center = carrierOne.transform.forward * boundingSphereThickness;
		} else {
			center = carrierTwo.transform.forward * boundingSphereThickness;

		}
		Vector3 position = new Vector3 (1000f, 1000f, 1000f);

	for (int i = 0; i < 5; i++) {
		position = center + (Random.insideUnitSphere * 100f);
			position.y = 0f;

		if (Physics.CheckSphere (position, maxAsteroidSize / 2f)) {
			break;
		}

	}
	piece.transform.position = position;
	Rigidbody rb = piece.GetComponent<Rigidbody> ();

	rb.AddForce (Random.insideUnitSphere  + (center - position));
	rb.AddRelativeTorque (Random.insideUnitSphere * maxAngularVelocity);
	if (Metwork.peerType != MetworkPeerType.Disconnected) {
		piece.GetComponent<MetworkView> ().RPC ("RPC_Relaunch", MRPCMode.Others, new object[]{piece.transform.localScale,   piece.GetComponent<Rigidbody>().mass });
	} else {

	}

}
	
	//move to game controller
	public IEnumerator FlashWarning(){
		while (warningActive == true) {
			float alpha = warningPanel.canvasRenderer.GetAlpha ();
			if (alpha <= 0f) {
				AIncreasing = true;
			} else if (alpha >= 1f) {
				AIncreasing = false;
			}
			if (AIncreasing == false) {
				warningPanel.canvasRenderer.SetAlpha( alpha- 0.05f);
			} else {
				warningPanel.canvasRenderer.SetAlpha( alpha+ 0.05f);
			}




			yield return new WaitForSecondsRealtime (0.01f);
			exitTime += Time.deltaTime;

		}
	}
}

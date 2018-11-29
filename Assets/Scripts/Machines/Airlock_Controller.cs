using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airlock_Controller : MonoBehaviour {

	public GameObject door1;
	public GameObject door2;
	Door_Controller door1Controller;
	Door_Controller door2Controller;
	public GameObject warningLight;
	Light bulb;
	public bool pressurizing;
	public bool turnUp = false;
	public float openTime;
	public AudioSource pressureSound;

	// Use this for initialization
	void Start () {
		door1Controller = door1.GetComponent<Door_Controller> ();
		door2Controller = door2.GetComponent<Door_Controller> ();
		pressureSound = this.GetComponent<AudioSource> ();
		bulb = warningLight.GetComponent<Light> ();
	}

	void Activate(){
		if (door1Controller.open) {
			door1Controller.Activate ();
		} else if (door2Controller.open) {
			door2Controller.Activate ();

		}

	}

	// Update is called once per frame
	void Update () {
		if (door1Controller.open) {
			door2Controller.locked = true;
			openTime += Time.deltaTime;
		}
		if (door2Controller.open) {
			door1Controller.locked = true;
			openTime += Time.deltaTime;


		}
		if (door1Controller.closing&&!pressurizing) {
			StartCoroutine (Pressurize(2));
		}
		else if (door2Controller.closing&&!pressurizing) {
			StartCoroutine (Pressurize(1));
		}
		if (openTime > 30f) {
			openTime = 0;


			print ("Working on it");
			door1Controller.locked = false;

			door2Controller.locked = false;
			door1Controller.open = true;
			door2Controller.open = true;
			door1Controller.StartCoroutine("Activate");
			door2Controller.StartCoroutine("Activate");
			print ("Activated both doors");
			StartCoroutine (Pressurize (0));

		}
	}

	IEnumerator Pressurize(int doorToOpen){
		print ("Running");
		pressurizing = true;
		yield return new WaitUntil (()=>(!door1Controller.open && !door2Controller.open));
		if (warningLight.GetComponent<Animator> () != null) {
			warningLight.GetComponent<Animator> ().SetBool ("Is Flashing", true);
		} else {
			Debug.LogWarning ("No animator attached to warning light: " + warningLight.transform.root.name + "/" + warningLight.name); 
		}
		pressureSound.Play ();
		print ("Doors Closed");
		door1Controller.locked = true;
		door2Controller.locked = true;
		yield return new WaitForSeconds (6f);
		if (doorToOpen == 1) {
			door1Controller.locked = false;

			door1Controller.StartCoroutine("Activate");
		} else if (doorToOpen == 2) {
			door2Controller.locked = false;
			door2Controller.StartCoroutine("Activate");
		} else {
			door1Controller.locked = false;

			door2Controller.locked = false;

		}
		warningLight.GetComponent<Animator> ().SetBool ("Is Flashing", false);
		pressureSound.Stop ();
		openTime = 0;

		pressurizing = false;


	}

}

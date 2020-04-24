using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Michael Gunther 22-01-2018
 * Charges and fires the plasma cannon
 * */
public class Plasma_Cannon : MonoBehaviour {

	public GameObject plasmaBallPrefab;
	public GameObject plasmaBall;

	//The largest size the ball gets(in meters)
	float maxSize = 6;

	public enum PlasmaCannonChargeState
	{
		Discharged,
		Discharging,
		Charging,
		Charged,
		Firing
	}

	//The seconds between two charge percentages
	public float chargeRate;
	public float dischargeRate;

	public Carrier_Controller ship;

	public float chargePercentage;

	//Whether the cannon has been requested to charge
	public bool charge = false;
	//If the cannon has been requested to fire
	public bool fire;
	public int damagePower = 50000;

	//The state of the plasma cannon
	[SerializeField]
	public PlasmaCannonChargeState chargeState = PlasmaCannonChargeState.Discharged;



	// Use this for initialization
	void Start () {
		ship = transform.root.GetComponent<Carrier_Controller> ();
	}
	
	// Update is called once per frame
	void Update () {
		//if(chargeState == PlasmaCannonChargeState.Charging){
		//	plasmaParticles;
		//}
		if (ship.hasPower && charge &&chargeState != PlasmaCannonChargeState.Charging) {
			StartCoroutine (ChargeCannon ());

		}
		if (!ship.hasPower && chargeState == PlasmaCannonChargeState.Charging) {
			StartCoroutine (DischargeCannon ());
		}
		if (chargeState == PlasmaCannonChargeState.Charged) {
			//Place in the right position
			plasmaBall.transform.position= this.transform.position;
		}

		if (fire && chargeState == PlasmaCannonChargeState.Charged) {
			StartCoroutine (Fire ());
		}


	}

	IEnumerator ChargeCannon(){
		//Indicate the cannon is charging
		chargeState = PlasmaCannonChargeState.Charging;

		plasmaBall = (GameObject)Instantiate (plasmaBallPrefab, this.transform.position, this.transform.rotation);
		plasmaBall.transform.SetParent (this.transform);

		//Spin the ball (for dramatic effect) at 3 radians per second on the z axis
		plasmaBall.GetComponent<Rigidbody>().angularVelocity = Vector3.left *3f;


		//While the cannon is connected to the power core and the cannon is not full charged
		while (ship.hasPower && chargePercentage < 100f) {
			//Increase the charge
			chargePercentage += 0.1f;

			//Resize the plasma ball
			plasmaBall.transform.localScale = Vector3.one * (chargePercentage * maxSize/ 100f);

			//Place in the right position
			plasmaBall.transform.position= this.transform.position;

			//If the cannon is fully charge
			if (chargePercentage >= 100f) {
				//indicate the cannon is charged
				chargeState = PlasmaCannonChargeState.Charged;

				charge = false;
			}

			//By ten because we are interpolating
			yield return new WaitForSeconds (chargeRate / 10f);

		}

	}

	IEnumerator DischargeCannon(){

		//Indicate the cannon is discharging
		chargeState = PlasmaCannonChargeState.Discharging;
		ship.pilotAR.ChargeCannon(false);


		//While the cannon is not connected to the power core and the cannon is not full discharged
		while (!ship.hasPower && chargePercentage > 0f) {
			//Increase the charge
			chargePercentage -= 0.1f;

			//Resize the plasma ball
			plasmaBall.transform.localScale = Vector3.one * (chargePercentage * maxSize/ 100f);

			//Place in the right position
			plasmaBall.transform.position= this.transform.position;

			//If the cannon is fully discharged
			if (chargePercentage <= 0f) {
				//indicate the cannon is charged
				chargeState = PlasmaCannonChargeState.Discharged;
				Destroy (plasmaBall);
			}

			//Same reason as above
			yield return new WaitForSeconds (dischargeRate / 10f);

		}

	}

	IEnumerator Fire(){
		fire = false;
		chargePercentage = 0;
		chargeState = PlasmaCannonChargeState.Discharged;
		ship.pilotAR.ChargeCannon( false);
		ship.pilotAR.FireCannon( false);

		yield return new WaitForSeconds (0f);

		plasmaBall.transform.parent = null;
		plasmaBall.GetComponent<SphereCollider> ().enabled = true;

		plasmaBall.GetComponent<Rigidbody> ().velocity += transform.forward * 400f;
		plasmaBall.GetComponent<Bullet_Controller> ().damagePower = damagePower;

		Destroy (plasmaBall, 40f);

		//Resize the plasma ball
		plasmaBall.transform.localScale = Vector3.one * (maxSize);

		plasmaBall = null;





	}


}

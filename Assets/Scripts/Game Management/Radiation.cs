using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radiation : MonoBehaviour {

	public int radiationPower = 10;
	int fromID = 0;
	public GameObject shipHolePrefab;
	public ParticleSystem stormSystem;
	public float initialTimeBetweenStorms = 40f;
	int stormNumber;
	public GameObject carrier;
	public LayerMask layerMask;
	public MetworkView netView;

	void Start(){
		this.transform.position = carrier.transform.position;
		this.transform.SetParent (carrier.transform);
		carrier.GetComponent<Rigidbody> ().isKinematic = true;
		stormSystem = GetComponent<ParticleSystem>();
		netView = this.GetComponent<MetworkView> ();
		StartCoroutine (StartStorm());

	}
	// Use this for initialization
	IEnumerator StartStorm () {
		print ("Starting Radiaition storm");
		yield return new WaitForSeconds (initialTimeBetweenStorms-stormNumber);
		ParticleSystem.MainModule main = stormSystem.main;
		main.duration = (10+20+stormNumber*0.5f);
		stormSystem.Play ();


		yield return new WaitForSeconds (10f);

		for (int i = 0; i < 40+stormNumber; i++) {
			Player_Controller[] allPlayers = FindObjectsOfType<Player_Controller> ();
			foreach (Player_Controller player in allPlayers) {
				if (Random.Range (0, 3) == 0) {
					if (Physics.Linecast (player.transform.position + Vector3.up * 200f, player.transform.position, layerMask,QueryTriggerInteraction.Ignore) == false) {
						Debug.DrawLine (player.transform.position + Vector3.up * 200f, player.transform.position,Color.blue,5f);
						player.damageScript.TakeDamage ((int)Random.Range(0.9f*radiationPower+i*0.5f, 1.1f*radiationPower+i*0.5f), 0);
					}

				} 

			}
			if (Metwork.isServer||Metwork.peerType == MetworkPeerType.Disconnected) {
				foreach (Ship_Hole hole in FindObjectsOfType<Ship_Hole>()) {
					hole.attachedCarrier.GetComponent<Damage> ().TakeDamage ((radiationPower+40), 0);
				}
				if (Random.Range (0, Mathf.Max(0,10 - stormNumber)) == 0) {
					RaycastHit hit;
					Vector2 randCircle = Random.insideUnitCircle * 300f;
					Vector3 randPosition = carrier.transform.position + new Vector3 (randCircle.x, 200f, randCircle.y);
					Debug.DrawLine (randPosition, randPosition+Vector3.down*100f, Color.red, 5f);
					if (Physics.Raycast (randPosition, Vector3.down, out hit, 400f,layerMask, QueryTriggerInteraction.Ignore)) {
						if (hit.transform.GetComponent<Carrier_Controller> () != null) {
							if (Metwork.peerType != MetworkPeerType.Disconnected) {
								int _viewID = Metwork.AllocateMetworkView (Metwork.player.connectionID);
								netView.RPC ("RPC_InstantiateHole", MRPCMode.AllBuffered, new object[]{ hit.point, hit.normal ,_viewID});

							} else {
								RPC_InstantiateHole (hit.point, hit.normal, -1);
							}

						}
					}
				}
			}
			yield return new WaitForSeconds (0.5f);
		}



		stormNumber++;
		radiationPower = (int)(radiationPower * 1.1f);
		StartCoroutine (StartStorm());
	}
	[MRPC]
	public void RPC_InstantiateHole(Vector3 position, Vector3 normal, int _ID){
		GameObject shipHole = Instantiate (shipHolePrefab, position, Quaternion.identity);
		shipHole.transform.rotation = Quaternion.LookRotation (normal);
		shipHole.transform.parent = carrier.transform;
		shipHole.GetComponentInChildren<Ship_Hole> ().attachedCarrier = carrier;
		shipHole.GetComponentInChildren<Panel_Controller> ().Activate (carrier);
		if (_ID == -1)
		{
			return;
		}
		shipHole.GetComponent<MetworkView>().viewID = _ID;
		if (!Metwork.metViews.ContainsKey (_ID)) {
			Metwork.metViews.Add (_ID, shipHole.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_ID] = shipHole.GetComponent<MetworkView> ();
		}

	}
	
	

		//if (other.transform.root.GetComponent<Carrier_Controller> () != null) {
			//GameObject shipHole = (GameObject)Instantiate (shipHolePrefab);
			//shipHole.GetComponent<Damage> ().forwardedDamage = other.transform.root.GetComponent<Damage> ();
		//}


}

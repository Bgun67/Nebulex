using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radiation : MonoBehaviour {

	public int radiationPower = 10;
	int fromID = 0;
	public GameObject shipHolePrefab;
	public GameObject asteroidPrefab; 
	public float initialTimeBetweenStorms = 40f;
	int stormNumber;
	public GameObject carrier;
	public LayerMask layerMask;
	public MetworkView netView;
	public int maxInitialAsteroids;

	void Start(){
		carrier.GetComponent<Rigidbody> ().isKinematic = true;
		netView = this.GetComponent<MetworkView> ();
		StartCoroutine (StartStorm());

	}
	// Use this for initialization
	IEnumerator StartStorm()
	{
		print("Starting Radiaition storm");
		GetComponent<AudioSource>().Play();
		yield return new WaitForSeconds(10f);

		for (int i = 0; i < 40 + stormNumber;)
		{

			if (!Metwork.isServer && Metwork.peerType != MetworkPeerType.Disconnected)
			{
				yield return null;
			}

			foreach (Ship_Hole hole in FindObjectsOfType<Ship_Hole>())
			{
<<<<<<< HEAD
				hole.attachedCarrier.GetComponent<Damage>().TakeDamage((radiationPower + 40), 0);
=======
				hole.attachedCarrier.GetComponent<Damage>().TakeDamage((radiationPower + 40), 0, hole.transform.position);
>>>>>>> Local-Git
			}
			Player_Controller[] allPlayers = FindObjectsOfType<Player_Controller>();
			foreach (Player_Controller player in allPlayers)
			{
				if (Random.Range(0, Mathf.Max(0, 10 - stormNumber)) != 0)
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.1f);

				if (Physics.Linecast(player.transform.position + Vector3.up * 100f, player.transform.position, layerMask, QueryTriggerInteraction.Ignore) == false)
				{
					InstantiateAsteroid(player.transform.position + Vector3.up * 100f);
					i++;
				}

			}
			int j = 0;
			while (j < stormNumber +7)
			{
				Vector2 randCircle = Random.insideUnitCircle * 300f;
				Vector3 randPosition = carrier.transform.position + new Vector3(randCircle.x, 200f, randCircle.y);

				if (Physics.Raycast(randPosition, Vector3.down, 200f, layerMask, QueryTriggerInteraction.Ignore))
				{
					InstantiateAsteroid(randPosition);
					i++;
				}
				j++;
			}

			yield return new WaitForSeconds(0.5f);
		}
		stormNumber++;
		maxInitialAsteroids += stormNumber * 3;
		radiationPower = (int)(radiationPower * 1.1f);
		yield return new WaitForSeconds(5f);

		GetComponent<AudioSource>().Stop();

		yield return new WaitForSeconds(initialTimeBetweenStorms - stormNumber);

		StartCoroutine(StartStorm());
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
		shipHole.GetComponentInChildren<MetworkView>().viewID = _ID;
		if (!Metwork.metViews.ContainsKey (_ID)) {
			Metwork.metViews.Add (_ID, shipHole.GetComponent<MetworkView> ());
		} else {
			Metwork.metViews[_ID] = shipHole.GetComponent<MetworkView> ();
		}

	}
	public void InstantiateHole(Vector3 _position, Vector3 _normal)
	{
		if (Random.Range(0, 5) == 0)
		{
			int _id;
			if (Metwork.peerType != MetworkPeerType.Disconnected)
			{
				if (Metwork.isServer)
				{
					_id = Metwork.AllocateMetworkView(Metwork.player.connectionID);
					netView.RPC("RPC_InstantiateHole", MRPCMode.AllBuffered, new object[] { _position, _normal, _id });

				}
			}
			else
			{
				_id = Metwork.AllocateMetworkView(1);
				RPC_InstantiateHole(_position, _normal, _id);
			}
		}
	}
	void InstantiateAsteroid(Vector3 _position)
	{
		Debug.DrawLine(_position, _position + Vector3.down * 200f);
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_InstantiateAsteroid", MRPCMode.AllBuffered, new object[] { _position});

		}
		else
		{
			RPC_InstantiateAsteroid(_position);
		}
	}
	[MRPC]
	public void RPC_InstantiateAsteroid(Vector3 position){
		GameObject _asteroid = Instantiate (asteroidPrefab, position, Quaternion.identity);

	}
	



}

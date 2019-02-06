using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mite_Controller : MonoBehaviour {
	
	public Player_Controller[] allPlayers;
	public Transform targetedPlayer;
	NavMeshAgent navAgent;
	public GameObject testPlayer;
	// Use this for initialization
	void Awake () {
		navAgent = this.GetComponent<NavMeshAgent>();
		allPlayers = GameObject.FindObjectsOfType<Player_Controller> ();
		FindPrimaryTarget();
		InvokeRepeating("TargetPlayer", Random.Range(0.1f,0.5f), 1f);
	}
	void FindPrimaryTarget()
	{
		targetedPlayer = allPlayers[Random.Range(0, allPlayers.Length)].transform;

	}
	// Update is called once per frame
	public void TargetPlayer()
	{
		RaycastHit playerHit;
		NavMeshHit _hit;
		print("checking players");
		foreach (Player_Controller player in allPlayers)
		{
			if (!NavMesh.SamplePosition(player.transform.position, out _hit, 2f, NavMesh.AllAreas))
			{
				continue;
			}
			if (Physics.Raycast(this.transform.position, player.transform.position - this.transform.position, out playerHit))
			{
				if (playerHit.collider.GetComponent<Player_Controller>())
				{
					targetedPlayer = player.transform;
					break;
				}
			}
			//
		}
		
		navAgent.destination = targetedPlayer.transform.position;
		print("Setting Destingation to player");

		
		
		Debug.DrawLine(transform.position, navAgent.destination, Color.red);
	}

	public void Mitosis()
	{
		navAgent.isStopped = true;
		Instantiate(this.gameObject, transform.position, transform.rotation);
		navAgent.isStopped = false;
	}
	//force at 1-distacne at 1 works
	//drag at 2 force at 2 is good
	void OnTriggerEnter(Collider other){
		if (other.tag == "Player") {
			other.GetComponent<Damage> ().TakeDamage (5,0);
		}
	}
	public void Die()
	{
		FindObjectOfType<Infestation>().mites.Push(this.gameObject);
		gameObject.SetActive(false);

	}
}

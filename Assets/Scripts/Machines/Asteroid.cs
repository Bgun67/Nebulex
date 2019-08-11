using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {
	Radiation radiation;
	public int damagePower = 100;
	public GameObject explosionPrefab;
	[Tooltip("Assigned From Radiation")]
	public LayerMask layerMask;
	// Use this for initialization
	void Start () {
		radiation = FindObjectOfType<Radiation>();
		this.layerMask = radiation.layerMask;
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.GetComponent<Gravity_Controller>() != null)
		{
			return;
		}
		RaycastHit hit;
		Physics.Raycast(transform.position+Vector3.up*10f, Vector3.down, out hit, 11f, layerMask, QueryTriggerInteraction.Ignore);
		if (other.GetComponent<Damage>()!=null)
		{
<<<<<<< HEAD
			other.GetComponent<Damage>().TakeDamage(damagePower,0);
=======
			other.GetComponent<Damage>().TakeDamage(damagePower,0, transform.position);
>>>>>>> Local-Git
		}
		if (other.transform.root.GetComponent<Carrier_Controller>() != null)
		{
			if (hit.collider != null&&hit.transform.GetComponent<Ship_Hole>()==null)
			{
				radiation.InstantiateHole(hit.point, hit.normal);
			}
		}
		if (explosionPrefab != null)
		{
			Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity),1f);
		}
		Destroy(this.gameObject);
	}


}

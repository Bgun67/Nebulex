using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poynting_Shield : MonoBehaviour {
	public GameObject explosionEffect;
	public MeshCollider collider;
	//public Rigidbody rb;
	void Reset()
	{
		collider = GetComponent<MeshCollider>();
	}

	/* 
		void OnTriggerEnter(Collider other){
			print ("Hit");

			Rigidbody otherRb = other.GetComponentInParent<Rigidbody> ();

			if (otherRb == null) {
				return;
			}

			if (Vector3.Dot (otherRb.velocity.normalized, this.transform.forward) <= 0) {
				otherRb.velocity = rb.velocity;
			} else {
				print("Unable");
			}
		}*/
	//repurposed script
	void OnTriggerEnter(Collider other){
		Transform root = other.transform.root;
		if (root.tag == "Bullet")
		{
			if (root.GetComponent<Bullet_Controller>().isExplosive)
			{
				Vector3 _velocity = root.GetComponent<Rigidbody>().velocity;
				RaycastHit _hit;
				Debug.DrawRay(root.position - _velocity.normalized*10f,  _velocity.normalized, Color.green, 5f);
				if (Physics.Raycast(root.position - _velocity.normalized*10f,  _velocity.normalized, out _hit))
				{
					if (_hit.collider.transform == this.transform)
					{
						root.GetComponent<Bullet_Controller>().DisableBullet();
						Destroy(Instantiate(explosionEffect, root.position, root.rotation), 4f);
					}
				}

			}
		}
	}
	public IEnumerator ShutdownShield()
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		Material mat = renderer.material;

		while (mat.color.a>0f)
		{
			mat.color =new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a-0.01f);
			renderer.material = mat;
			yield return new WaitForSeconds(0.01f);
		}
		renderer.enabled = false;
		collider.enabled = false;

	}
	public IEnumerator ReactivateShield()
	{
		MeshRenderer renderer = GetComponent<MeshRenderer>();
		Material mat = renderer.material;
		renderer.enabled = true;

		while (mat.color.a<1f)
		{
			mat.color =new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a+0.01f);
			renderer.material = mat;
			yield return new WaitForSeconds(0.01f);
		}
		collider.enabled = true;
	}
}

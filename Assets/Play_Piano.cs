using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Piano : MonoBehaviour
{
	Animator anim;
	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
	}

    // Update is called once per frame
    void OnCollisionEnter( Collision other)
    {

		if (other.transform.tag == "Bullet")
		{
			anim.SetTrigger("Play");
			print(-Vector3.SignedAngle(transform.forward, -other.relativeVelocity, transform.up) );
			anim.SetFloat("Blend",-Vector3.SignedAngle(transform.forward, -other.relativeVelocity, transform.up));
		}
	}
}

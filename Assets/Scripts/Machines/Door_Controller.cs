using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Controller : MonoBehaviour {
	public Animator anim;
	[Tooltip ("Does the door open with a speed or does it simply run the animation")]
	public bool automatic = true;
	[Tooltip ("speed at which manual door anims should run")]
	public float doorSpeed;
	[Tooltip ("is the door open?")]
	public bool open = false;
	public bool doNotMoveCollider;
	public bool opening;
	public bool closing;
	public bool locked;
	public BoxCollider boxCollider;
	public int doorNumber = -1;
	public bool outerDoor = false;
	// Use this for initialization
	void Start(){
		
	}
	public IEnumerator Activate () {

		if (!locked)
		{
			yield break;
		}

		if (!open) {
				open = true;

				anim.SetBool ("Opening", true);
				if (!automatic) {
					anim.SetFloat ("Door Speed", doorSpeed);
				}
				if (doNotMoveCollider) {
					boxCollider.isTrigger = true;
				}
				opening = true;
				

			} else
		{
			open = false;
			anim.SetBool("Opening", false);
			if (!automatic)
			{
				anim.SetFloat("Door Speed", -doorSpeed);
			}

			if (doNotMoveCollider)
			{
				boxCollider.isTrigger = false;
			}
			if (outerDoor)
			{
				locked = true;
			}
			closing = true;
		}

		yield return new WaitForSeconds (2f);
		closing = false;
		opening = false;

	}

	public void Unlock()
	{
		locked = false;
	}
	public void TerminateMovement(){
		anim.SetFloat ("Door Speed", 0f);

	}





}

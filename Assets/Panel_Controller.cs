using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Panel_Controller : MonoBehaviour {
	public Animator anim;
	public bool repairing;
	public float totalRepairTime = 10f;
	public float currentRepairTime;
	public float autoTime = 10f;
	public float currentAutoTime;
	public GameObject player;
	public ParticleSystem sparkEffect;
	public UnityEvent repairFunction;
	public UnityEvent disableFunction;
	public Light redLight;
	public Light greenLight;



	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	public void Activate (GameObject _player) {
		if (anim.GetBool ("Open")) {
			
			repairing = true;
			anim.SetBool ("Repair",true);

		} else {
			anim.SetBool ("Open", true);
			Disable ();
			currentAutoTime = 0f;
		}
		player = _player;
	}
	void Update(){
		if (repairing) {
			if (Input.GetButton ("Use Item")) {
				if (currentRepairTime >= totalRepairTime) {
					currentRepairTime = totalRepairTime;
					repairing = false;

					Repair ();

				}
				greenLight.enabled = true;
				redLight.enabled = true;
				currentRepairTime += Time.deltaTime;
				if (!sparkEffect.isPlaying) {
					sparkEffect.Play ();
				}
				player.GetComponent<Rigidbody> ().velocity = this.transform.root.GetComponent<Rigidbody> ().velocity;
			} else {
				repairing = false;
				greenLight.enabled = false;
				currentRepairTime = 0f;
				sparkEffect.Stop ();
				anim.SetBool ("Repair",false);


			}
		} 
		if (currentAutoTime < autoTime) {
			currentAutoTime += Time.deltaTime;
		} else if(currentAutoTime>autoTime) {
			Repair ();
			currentAutoTime = autoTime;
		}
	}
	void Disable(){
		currentRepairTime = 0;
		disableFunction.Invoke ();
		greenLight.enabled = false;
		redLight.enabled = true;

	}

	void Repair()
	{
		redLight.enabled = false;

		anim.SetBool("Open", false);
		try
		{
			anim.SetBool("Repair", false);
		}
		catch
		{
		}

		repairFunction.Invoke();
		greenLight.enabled = true;
		currentAutoTime = autoTime;

	}
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Damage : MonoBehaviour {
	public int originalHealth;
	public int currentHealth;
	public UnityEvent dieFunction;
	public float damageThreshold1;

	public Text UI_healthText;
	public RectTransform UI_healthBar;
	public RectTransform UI_healthBox;

	public Transform initialPosition;
	[Tooltip ("Make Longer than the carcass destroy time")]
	public float resetTime;
	int[] assistArray = new int[33];
	public Game_Controller gameController;

	public float damageThreshold2;
	public bool regen;
	 float regenWait;
	[Tooltip("Time after taking damage when the health starts to regenerate")]
	public float regenTime;
	public bool healthShown;
	public bool isDead;
	[Tooltip("Use this to send the damage to a different damage script specified below")]
	public bool forwarder;
	[Tooltip("Leave empty if above not checked")]
	public Damage forwardedDamage;
	[Tooltip("Ignore if forwarder not checked")]
	public float forwardedScale = 1f;


	public GameObject impactPrefab;

	[Tooltip("Set this to -1 to disable impact damage")]
	public float impactDamageFactor = -1f;

	public Metwork_Object netObj;

	// Use this for initialization
	void Start () {
		if (forwarder) {
			if (forwardedDamage == null) {
				forwardedDamage = transform.root.GetComponent<Damage> ();
			}
			return;
		}
		currentHealth = originalHealth;
		Reactivate ();
		gameController = FindObjectOfType<Game_Controller> ();
		if(healthShown){
			UpdateUI();
		}

		if (regen) {
			InvokeRepeating ("RegenHealth", 1f, 0.5f);
		}
		if (netObj == null) {
			netObj = this.GetComponent<Metwork_Object> ();
		}
		if (netObj == null) {
			netObj = this.GetComponentInParent<Metwork_Object> ();
		}

	}
	public void Reset(){
		print ("Respawned");

		this.gameObject.SetActive (false);

		Invoke ("Reactivate", resetTime);
			

	}
	public void Reactivate(){
		if (initialPosition != null) {
			transform.position = initialPosition.position;
			transform.rotation = initialPosition.rotation;
		} else {
			initialPosition = this.transform;
		}
		currentHealth = originalHealth;
		this.gameObject.SetActive (true);
		isDead = false;


	}


	public void TakeDamage(int damageAmount, int fromID){
		if (forwarder) {
			print ("sendingDamage");

			forwardedDamage.TakeDamage ((int)(damageAmount*forwardedScale), fromID);
			return;
		}
		if (damageAmount >= damageThreshold1) {
			if (!netObj.isLocal || isDead) {
				return;
			}
			currentHealth -= damageAmount;

			if (regen) {
				regenWait = Time.time + regenTime;
			}
			if (currentHealth <= 0f) {
				if (fromID != 0) {
					if (this.tag == "Player") {
						gameController.AddKill (fromID);
						gameController.AddDeath (netObj.owner);


					}

				}
				for (int i = 1; i < assistArray.Length; i++) {
					if (i == fromID) {
						continue;
					}
					int assistScore = assistArray [i];
					assistScore = (int)Mathf.Clamp((assistScore / originalHealth)*100,0,100);
					if (assistScore >= 85) {
						gameController.AddKill (i);

					}
					else if(assistScore>33){
						gameController.AddAssist (i,assistScore);

					}

				}
				for (int i = 0; i < assistArray.Length; i++) {
					assistArray [i] = 0;
				}
				if (dieFunction.GetPersistentEventCount () > 0) {
					isDead = true;
					dieFunction.Invoke ();

				} else {
					Destroy (this.gameObject);
				}
				int j = 0;
				foreach (Damage damageScript in this.GetComponentsInChildren<Damage>()) {
					if (j > 9) {
						break;
					}
					print (damageScript.gameObject.name);
					damageScript.TakeDamage (damageScript.originalHealth + 1, fromID);

				}
					


				
			} else {
				if (fromID != 0) {
					assistArray [fromID] += damageAmount;
				}
			}
			if (healthShown) {
				UpdateUI ();
			}
		}



	}
	public void RegenHealth(){
		if (currentHealth < originalHealth) {
			if (Time.time >= regenWait) {
				currentHealth++;
				UpdateUI ();
			}
		} else {
			for (int i = 0; i < assistArray.Length; i++) {
				assistArray [i] = 0;
			}
		}
	}

	public void OnCollisionEnter(Collision collision){
		if (forwarder) {
			return;
		}
		if (impactPrefab != null && collision.transform.root.tag == "Bullet") {
			GameObject impact = (GameObject)Instantiate (impactPrefab, collision.contacts[0].point, Quaternion.identity);
			impact.transform.forward = -collision.contacts [0].normal;
			impact.transform.SetParent (this.transform);
			Destroy (impact, 20f);
			return;
		}

		if (!netObj.isLocal||collision.transform.root.tag == "Bullet") {
			return;
		}

		if (impactDamageFactor > 0f) {
			
			float damage = collision.impulse.sqrMagnitude * impactDamageFactor * 1E-6f;
			if (damage > 1f) {
				TakeDamage ((int)damage, 0);
			}
		}
	}
	public void UpdateUI(){
		if (healthShown == true) {
			UI_healthBox.gameObject.SetActive (true);
			UI_healthText.text = "+" + currentHealth;
			UI_healthBar.offsetMin = new Vector2(128f - (float)currentHealth/(float)originalHealth * 128f,-16f);
			UI_healthBar.offsetMax = new Vector2(256f - (float)currentHealth/(float)originalHealth * 128f,0f);
		} else {
			//UI_healthText.text = "";
			if (UI_healthBox != null) {
				UI_healthBox.gameObject.SetActive (false);
			}

		}
	}
}

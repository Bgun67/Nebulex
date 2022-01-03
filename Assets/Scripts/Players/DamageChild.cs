using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DamageChild :MonoBehaviour, IDamage  {
	[Tooltip("Leave blank to use transform root. Damage script to send the health reduction to.")]
	public Damage forwardedDamage;
	[Tooltip("Ignore if forwarder not checked")]
	public float forwardedScale = 1f;

	// Use this for initialization
	void Awake () {
		if (forwardedDamage == null) {
			forwardedDamage = transform.root.GetComponent<Damage> ();
		}
	}
	public void TakeDamage(int damageAmount, int fromID, Vector3 _hitDirection, bool overrideTeam = false)
	{
		forwardedDamage.TakeDamage((int)(damageAmount * forwardedScale), fromID, _hitDirection, overrideTeam);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Mirror;

public interface IDamage{
	public void TakeDamage(int damageAmount, int fromID, Vector3 _hitDirection, bool overrideTeam = false);
}

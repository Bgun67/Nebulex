using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class Activater : NetworkBehaviour
{
	public MonoBehaviour[] scriptsToActivate;
	public int maxPassengers;
	public int passengers;
	public string text = "";
	GameObject player;
	public float maxDistance = Mathf.Infinity;
	public bool raycast = false;
	public float cooldown = 0;
	[HideInInspector][SyncVar] public float nextAvailableTime = 0;
	[SerializeField] Transform center;


	public Vector3 Position
	{
		get
		{
			if (center)
			{
				return center.position;
			}
			else
			{
				return transform.position;
			}
		}
	}

	public void ActivateScript(GameObject player)
	{
		if (Time.time < nextAvailableTime)
		{
			return;
		}
		nextAvailableTime = Time.time + cooldown;
		foreach (MonoBehaviour scriptToActivate in scriptsToActivate)
		{
			print("Activating" + scriptToActivate.name);
			scriptToActivate.SendMessage("Activate", player);
		}
	}


	public void DeactivateScript(GameObject player)
	{
		CmdDeactivateScript();
	}

	[Command]
	public void CmdDeactivateScript()
	{
		foreach (MonoBehaviour scriptToActivate in scriptsToActivate)
		{
			scriptToActivate.SendMessage("Deactivate", player);
		}
		return;
	}

	//[MRPC]
	public void AddPassenger()
	{
		passengers++;
	}
	//[MRPC]
	public void RemovePassenger()
	{
		passengers--;
	}

}

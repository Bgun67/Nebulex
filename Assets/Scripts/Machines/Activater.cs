using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activater : MonoBehaviour
{
	public MonoBehaviour[] scriptsToActivate;
	public int maxPassengers;
	public int passengers;
	public MetworkView netView;
<<<<<<< HEAD
	
	// Use this for initialization
	void Start () {
		netView = GetComponent<MetworkView> ();
=======
	public string text = "";
	GameObject textObj;
	GameObject player;

	// Use this for initialization
	void Start()
	{
		Invoke("Setup", 0.1f);
>>>>>>> Local-Git
	}
	void Setup(){
		netView = GetComponent<MetworkView>();
		textObj = Instantiate((GameObject)Resources.Load("Info Text"));
		Vector3 _textPosition = transform.position;
		if (GetComponent<Collider>())
		{
			_textPosition = GetComponent<Collider>().bounds.center;
		}
		else if(GetComponentInChildren<Collider>())
		{
			_textPosition = GetComponentInChildren<Collider>().bounds.center;

<<<<<<< HEAD
	public void ActivateScript(GameObject player)
	{
=======
		}
		textObj.transform.position = _textPosition;
		textObj.transform.parent = transform;
		SetText();
		player = FindObjectOfType<Game_Controller>().localPlayer;

		StartCoroutine(CheckShowText());

	}
>>>>>>> Local-Git

	public void ActivateScript(GameObject player)
	{

<<<<<<< HEAD
=======

>>>>>>> Local-Git
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_ActivateScript", MRPCMode.AllBuffered, new object[] { player.GetComponent<Metwork_Object>().netID });
		}
		else
		{
			RPC_ActivateScript(player.GetComponent<Metwork_Object>().netID);
		}

	}

	[MRPC]
	public void RPC_ActivateScript(int _netID)
	{
		GameObject player = FindObjectOfType<Game_Controller>().GetPlayerFromNetID(_netID);

		foreach (MonoBehaviour scriptToActivate in scriptsToActivate)
		{
			print("Activating" + scriptToActivate.name);
			scriptToActivate.enabled = true;
			scriptToActivate.SendMessage("Activate", player);

		}
		return;
	}

	public void DeactivateScript(GameObject player)
	{
		if (maxPassengers == 0)
		{
			netView.RPC("RPC_DeactivateScript", MRPCMode.AllBuffered, new object[] { });

		}
		if (passengers < 1)
		{
			foreach (MonoBehaviour scriptToActivate in scriptsToActivate)
			{

				scriptToActivate.enabled = false;
				//scriptToActivate.GetComponent<Metwork_Object> ().owner = 0;
			}
		}


	}

	[MRPC]
	public void RPC_DeactivateScript()
	{
		foreach (MonoBehaviour scriptToActivate in scriptsToActivate)
		{

			scriptToActivate.enabled = false;
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

	public IEnumerator CheckShowText()
	{
		yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			Vector3 _displacement = player.transform.position - transform.position;
			if (_displacement.sqrMagnitude > 40f)
			{
				textObj.SetActive(false);
			}
			else
			{
				textObj.SetActive(true);
				float _angle = Vector3.SignedAngle(Vector3.forward, -_displacement, Vector3.up);
				textObj.transform.rotation = Quaternion.Euler(0f,Mathf.RoundToInt( _angle/90f)*90f, 0f);
			}

		}
	}
	void SetText()
	{
		string useKey = "";
			if (MInput.useMouse)
			{
				useKey = "G";
			}
			else
			{
				useKey = "□";
			}
		if (text != "")
		{
			text.Replace("useKey", useKey);
			textObj.GetComponent<TextMesh>().text = text;
		}
		else
		{
			textObj.GetComponent<TextMesh>().text = "Press "+useKey+" To Use";
		}
	}
	
}

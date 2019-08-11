using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_Match_Pad : MonoBehaviour {
	public GameObject text;
	public ParticleSystem glow;
	public void Start()
	{
		if (text == null)
		{
			text = GameObject.Find("Start Match Text");
			glow = GetComponentInChildren<ParticleSystem>();

		}
	}
	public void Activate()
	{
		try
		{
			var main = glow.main;
			main.startColor = Color.red;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		catch { }
		FindObjectOfType<Network_Manager>().PrematureStart();
	}
	
	// Update is called once per frame
	void Update () {
		text.transform.Rotate(0f, 1f, 0f);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fracture : Map_Manager
{

	protected override void DelayedSetup()
	{
		foreach (Player_Controller player in players)
		{
			player.suffocationTime = 120f;
		}
	}
	// Update is called once per frame
	void Update()
    {
        
    }
}

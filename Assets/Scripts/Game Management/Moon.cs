using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : Map_Manager
{
	
	protected override void MapSetup()
    {
		Physics.gravity = Vector3.up * -1.625f;
         
	}
	protected override void DelayedSetup()
	{
		
	}

	protected override void SlowUpdate()
    {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Controller : Ship_Controller
{
    [MRPC]
	public override void RPC_Exit()
	{
		base.RPC_Exit();
		if (anim != null)
		{
			anim.SetBool("Should Close", false);
		}
	}

	[MRPC]
	public override void RPC_Activate(int _pilot)
	{
		base.RPC_Activate(_pilot);
		
		if (anim != null) {
			anim.SetBool ("Should Close", true);
		} 
	}

	protected override void ShipUpdate()
	{
		base.ShipUpdate();
		if (Input.GetButtonDown("Jump"))
		{
			
			EnableNightVision();
			
		}
	}

	void EnableNightVision()
	{
		Night_Vision_Effects nightVision = mainCamera.GetComponent<Night_Vision_Effects>();
		if (nightVision != null)
		{
			nightVision.enabled = !nightVision.enabled;

		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport_Controller : Ship_Controller
{
    // Start is called before the first frame update
    protected override void ShipUpdate()
    {
		base.ShipUpdate();
		if (Input.GetButtonDown("Jump"))
		{
			LowerRamp();
		}
    }
	void LowerRamp()
	{
		transform.GetComponentInChildren<Door_Controller>().GetComponent<Activater>().ActivateScript(player.gameObject);
	}
	

    
}

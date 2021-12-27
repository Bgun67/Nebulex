using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class System_Sector : MonoBehaviour
{
	public Door_Controller [] doors;
	bool pressurized = true;
	Warning_Light[] warningLights;
	// Start is called before the first frame update
	void Awake()
    {
		doors = GetComponentsInChildren<Door_Controller>();
		warningLights = GetComponentsInChildren<Warning_Light>();
	}
	void Start()
	{
		Pressurize();
	}

	public void Depressurize()
	{
		foreach (Warning_Light wlight in warningLights)
		{
			wlight.Flash(true);
			pressurized = false;
		}
	}

	public void Pressurize()
	{
		foreach (Warning_Light wlight in warningLights)
		{
			wlight.Flash(false);
			pressurized = true;
		}
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}

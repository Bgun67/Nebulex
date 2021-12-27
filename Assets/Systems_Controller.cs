using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Systems_Controller : MonoBehaviour
{
	int currentDoor = 0;
	public Door_Controller[] doors;
	public System_Sector[] sectors;
	// Start is called before the first frame update
	void Reset()
    {
		doors = FindObjectsOfType<Door_Controller>();
		sectors = FindObjectsOfType<System_Sector>();
		for (int i = 0; i < doors.Length; i++)
		{
			doors[i].doorNumber = i;
		}

	}

	void Update()
	{
		if (Time.frameCount % 60 == 0)
		{
			ToggleDoor(currentDoor);
			currentDoor = (currentDoor+1)% doors.Length;
			sectors[0].Depressurize();
		}
	}

	// Update is called once per frame
	void ToggleDoor(int doorNumber)
    {
        StartCoroutine(doors[doorNumber].Activate());
	}
	
}

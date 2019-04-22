using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : Map_Manager
{
	public GameObject[] seaCameraObjs;
	public Crest.OceanRenderer ocean;
	protected override void MapSetup()
    {
		ocean = FindObjectOfType<Crest.OceanRenderer>();
	}
	protected override void DelayedSetup()
	{
		foreach (Player_Controller player in players)
		{
			GameObject.Instantiate(seaCameraObjs[0], player.mainCamObj.transform);
			GameObject.Instantiate(seaCameraObjs[1], player.mainCamObj.transform);
		}
		foreach (Ship_Controller ship in FindObjectsOfType<Ship_Controller>())
		{
			print("Instantiating");
			GameObject.Instantiate(seaCameraObjs[0], ship.mainCamera.transform);
			GameObject.Instantiate(seaCameraObjs[1], ship.mainCamera.transform);
		}
	}

	protected override void SlowUpdate()
    {
		Camera[] _cams = FindObjectsOfType<Camera>();
		foreach (Camera _cam in _cams)
		{
			Transform _camTransform = _cam.transform;
			if (_camTransform != null && _camTransform.root!= ocean.transform)
			{
				ocean.Viewpoint = _camTransform;
				break;
			}
		}
	}
}

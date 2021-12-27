using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warning_Light : MonoBehaviour
{
	float intensity = 5f;
	float frequency = 2f;
	bool isFlashing;
	Light bulb;
	// Start is called before the first frame update
	void Awake()
    {
		bulb = GetComponentInChildren<Light>();
	}

	void Update()
	{
		if (!isFlashing)
		{
			bulb.enabled = false;
		}
		else
		{
			bulb.enabled = true;
			bulb.intensity = (0.5f+0.5f*Mathf.Sin(Time.time*frequency)) * intensity;
		}
	}

	// Update is called once per frame
	public void Flash(bool _isFlashing)
    {
		isFlashing = _isFlashing;
	}
}

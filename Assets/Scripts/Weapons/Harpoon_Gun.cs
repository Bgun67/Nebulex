using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon_Gun : MonoBehaviour {
	public Harpoon harpoon1;
	public Harpoon harpoon2;
	public LayerMask mask;
	public LineRenderer wire1;
	public LineRenderer wire2;
	public Transform shotSpawn;
	public bool fired;
	public float originalLength1;
	public float originalLength2;

	// Use this for initialization
	void Reset () {
		harpoon1 = transform.Find("Harpoon 1").GetComponent<Harpoon>();
		harpoon2 = transform.Find("Harpoon 2").GetComponent<Harpoon>();
		wire1 = harpoon1.GetComponent<LineRenderer>();
		wire2 = harpoon2.GetComponent<LineRenderer>();
		shotSpawn = transform.Find("Shot Spawn");

	}

	// Update is called once per frame
	void Update()
	{
		if (fired == true)
		{
			if (Vector3.SqrMagnitude(harpoon1.transform.position - shotSpawn.transform.position) > 200000f)
			{
				//BreakWire();
			}
			ApplyForce();
		}
		if (!harpoon1.gameObject.activeInHierarchy || !harpoon2.gameObject.activeInHierarchy)
		{
			BreakWire();
		}
		ShowWire();

		if (Input.GetButtonDown("Fire1"))
		{
			Fire();
		}

	}
	void Fire(){
		if (fired == false) {
			if (LaunchHarpoon(1))
			{
				LaunchHarpoon(2);
			}
		}
		else
		{
			BreakWire();
		}
	}
	bool LaunchHarpoon(int harpoonNumber)
	{
		Harpoon _harpoon;
		if (harpoonNumber == 1)
		{
			_harpoon = harpoon1;
		}
		else
		{
			_harpoon = harpoon2;
		}
		RaycastHit _hit;
		if (Physics.Raycast(shotSpawn.position, shotSpawn.forward * -1 * Mathf.Pow(-1, harpoonNumber), out _hit, 1000, mask, QueryTriggerInteraction.Ignore))
		{
			Transform _root = _hit.transform.root;
			print("Harpoon:"+harpoonNumber+" "+_root);
			if (_root.GetComponent<Rigidbody>() && (_root != transform.root))
			{
				if (_root.GetComponent<Carrier_Controller>())
				{
					_harpoon.carrierConnected = true;
				}
				_harpoon.isConnected = true;
				_harpoon.connectedBody = _hit.transform.GetComponent<Rigidbody>();
				_harpoon.transform.parent = _hit.transform;
				_harpoon.transform.position = _hit.point;
				if (harpoonNumber == 1)
				{
					originalLength1 = _hit.distance;
					fired = true;
				}
				else
				{
					originalLength2 = _hit.distance;
				}
				return true;

			}
		}
		return false;
	}
	void BreakWire(){
		fired = false;
		harpoon1.transform.parent = this.transform;
		harpoon1.transform.position = shotSpawn.transform.position;
		harpoon1.carrierConnected = false;
		harpoon1.isConnected = false;
		harpoon1.connectedBody = null;

		harpoon2.transform.parent = this.transform;
		harpoon2.transform.position = shotSpawn.transform.position;
		harpoon2.carrierConnected = false;
		harpoon2.isConnected = false;
		harpoon2.connectedBody = null;

	}
	public void ApplyForce()
	{
		if (harpoon1.isConnected)
		{
			Vector3 _displacement = (harpoon1.transform.position - shotSpawn.transform.position);
			Vector3 wireDirection = _displacement.normalized;
			float _distanceDelta = Mathf.Clamp(_displacement.magnitude - originalLength1, 0f, 100f);
			if (_displacement.magnitude < 5f)
			{
				BreakWire();
				return;
			}
			if (!harpoon1.carrierConnected)
			{
				harpoon1.connectedBody.AddForce(wireDirection * -10000f * _distanceDelta);
			}
			if (Input.GetButton("Sprint"))
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 100000f * Time.deltaTime);
				//prevent backwards movment
				originalLength1 = Mathf.Abs(_displacement.magnitude);
				originalLength2 = Mathf.Abs(Vector3.Distance(harpoon2.transform.position, harpoon1.transform.position) - _displacement.magnitude);
			}
			else
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 10000 * _distanceDelta);
			}
		}
		if (harpoon2.isConnected)
		{
			Vector3 _displacement = (harpoon2.transform.position - shotSpawn.transform.position);
			Vector3 wireDirection = _displacement.normalized;
			float _distanceDelta = Mathf.Clamp(_displacement.magnitude - originalLength2, 0f, 100f);
			
			if (!harpoon2.carrierConnected)
			{
				harpoon2.connectedBody.AddForce(wireDirection * -10000f * _distanceDelta);
			}
			
			else
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 10000 * _distanceDelta);
			}
		}
	}
	void ShowWire()
	{

		wire1.SetPosition(0, shotSpawn.transform.position);
		wire1.SetPosition(1, harpoon1.transform.position);


		wire2.SetPosition(0, shotSpawn.transform.position);
		wire2.SetPosition(1, harpoon2.transform.position);

	}
	void OnDisable()
	{
		BreakWire();
	}
}

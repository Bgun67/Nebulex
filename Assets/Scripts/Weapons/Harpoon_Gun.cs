using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Harpoon
{
	public Rigidbody connectedBody;
	public bool isConnected;
	public bool carrierConnected;
	public bool hit;
	public Transform _transform;
	public LineRenderer _wire;
	public Transform _shotSpawn;
	public float _originalLength;
}
public class Harpoon_Gun : MonoBehaviour
{
	public Harpoon[] harpoons = new Harpoon[2];
	public LayerMask mask;
	public Metwork_Object netObj;
	public bool fired;
	public Animator anim;


	// Use this for initialization
	void Reset () {
		harpoons[0] = FindHarpoonData(0);
		harpoons[1] = FindHarpoonData(1);
	}
	Harpoon FindHarpoonData(int harpoonNumber)
	{
		Harpoon data = new Harpoon();
		data._transform = transform.Find("Harpoon " + (harpoonNumber));
		data._wire = data._transform.GetComponent<LineRenderer>();
		data._shotSpawn = transform.Find("Shot Spawn "+harpoonNumber);
		return data;
	}
	void Start()
	{
		anim = GetComponent<Animator>();
		netObj = transform.root.GetComponent<Metwork_Object>();
	}

	// Update is called once per frame
	void Update()
	{
		if (!anim.GetBool("Enabled"))
		{
			return;
		}
		if (fired == true)
		{
			if (Vector3.SqrMagnitude(harpoons[0]._transform.position - harpoons[0]._shotSpawn.position) > 200000f)
			{
				//BreakWire();
			}
			ApplyForce();
		}
		if (!harpoons[0]._transform.gameObject.activeInHierarchy || !harpoons[1]._transform.gameObject.activeInHierarchy)
		{
			BreakWire();

		}
		ShowWire();
		if (!netObj.isLocal)
		{
			return;
		}
		if (Input.GetButtonDown("Fire1"))
		{
			if (fired == false)
			{
				Fire();
			}
			else
			{
				if (Metwork.peerType != MetworkPeerType.Disconnected)
				{
					netObj.netView.RPC("RPC_BreakWire", MRPCMode.AllBuffered, new object[] { });
				}
				else
				{
					netObj.SendMessage("RPC_BreakWire");
				}
			}
		}

	}
	void Fire(){
		if (LaunchHarpoon(0))
		{
			LaunchHarpoon(1);
		}
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			int _H0ID = harpoons[0]._transform.parent.GetComponent<Metwork_Object>().netID;
			int _H1ID = harpoons[1]._transform.parent.GetComponent<Metwork_Object>().netID;
			Vector3 _H0Pos = harpoons[0]._transform.localPosition;
			Vector3 _H1Pos = harpoons[1]._transform.localPosition;

			//you can find this in player controller
			netObj.netView.RPC("RPC_FireGrapple", MRPCMode.OthersBuffered, new object[] {_H0ID,_H1ID,_H0Pos,_H1Pos});
		}
	}
	public void ConnectGrapple(Transform parent1, Transform parent2, Vector3 localPos1, Vector3 localPos2)
	{
		//connect to transform
		harpoons[0]._transform.parent = parent1;
		harpoons[0]._transform.localPosition = localPos1;
		harpoons[1]._transform.parent = parent2;
		harpoons[1]._transform.localPosition = localPos1;


		//check if connected to somthing other than the gun
		if (parent1 != this.gameObject)
		{
			harpoons[0].isConnected = true;
			fired = true;
		}
		if (parent1.GetComponent<Carrier_Controller>()!= null)
		{
			harpoons[0].carrierConnected = true;
		}
		//check if connected to somthing other than the gun
		if (parent2 != this.gameObject)
		{
			harpoons[1].isConnected = true;
		}
		if (parent2.GetComponent<Carrier_Controller>()!= null)
		{
			harpoons[1].carrierConnected = true;
		}
	}
	bool LaunchHarpoon(int harpoonNumber)
	{
		
		Harpoon _harpoon = harpoons[harpoonNumber];
		
		RaycastHit _hit;
		if (Physics.Raycast(_harpoon._shotSpawn.position, _harpoon._shotSpawn.forward, out _hit, 1000, mask, QueryTriggerInteraction.Ignore))
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
				_harpoon._transform.parent = _hit.transform;
				_harpoon._transform.position = _hit.point;
				_harpoon._originalLength = _hit.distance;
				fired = true;
				return true;

			}
		}
		return false;
	}
	
	public void BreakWire(){
		fired = false;
		foreach (Harpoon _harpoon in harpoons)
		{
			_harpoon._transform.parent = this.transform;
			_harpoon._transform.position = _harpoon._shotSpawn.position;
			_harpoon.carrierConnected = false;
			_harpoon.isConnected = false;
			_harpoon.connectedBody = null;
		}

	}
	public void ApplyForce()
	{
		if (harpoons[0].isConnected)
		{
			Vector3 _displacement = (harpoons[0]._transform.position - harpoons[0]._shotSpawn.position);
			Vector3 wireDirection = _displacement.normalized;
			float _distanceDelta = Mathf.Clamp(_displacement.magnitude - harpoons[0]._originalLength, 0f, 100f);
			if (_displacement.magnitude < 5f)
			{
				BreakWire();
				return;
			}
			if (!harpoons[0].carrierConnected)
			{
				harpoons[0].connectedBody.AddForce(wireDirection * -10000f * _distanceDelta);
			}
			if (Input.GetButton("Sprint"))
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 100000f * Time.deltaTime);
				//prevent backwards movment
				harpoons[0]._originalLength = Mathf.Abs(_displacement.magnitude);
				harpoons[1]._originalLength = Mathf.Abs(Vector3.Distance(harpoons[1]._transform.position, harpoons[0]._transform.position) - _displacement.magnitude);
			}
			else
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 10000 * _distanceDelta);
			}
		}
		if (harpoons[1].isConnected)
		{
			Vector3 _displacement = (harpoons[1]._transform.position - harpoons[1]._shotSpawn.position);
			Vector3 wireDirection = _displacement.normalized;
			float _distanceDelta = Mathf.Clamp(_displacement.magnitude - harpoons[1]._originalLength, 0f, 100f);
			
			if (!harpoons[1].carrierConnected)
			{
				harpoons[1].connectedBody.AddForce(wireDirection * -10000f * _distanceDelta);
			}
			
			else
			{
				transform.root.GetComponent<Rigidbody>().AddForce(wireDirection * 10000 * _distanceDelta);
			}
		}
	}
	void ShowWire()
	{
		foreach (Harpoon harpoon in harpoons)
		{
			harpoon._wire.SetPosition(0, harpoon._shotSpawn.position);
			harpoon._wire.SetPosition(1, harpoon._transform.position);
		}

	}
	void OnDisable()
	{
		BreakWire();
	}
}

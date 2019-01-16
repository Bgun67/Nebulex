using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Infestation : MonoBehaviour {

	public Stack<GameObject> mites = new Stack<GameObject>();
	public GameObject mitePrefab;
	public int maxMites = 40;
	int startMetID = 10000;
	public Text evolutionText;
	public GameObject nest;
	public MetworkView netView;
	// Use this for initialization
	void Start () {
		foreach (Carrier_Controller carrier in FindObjectsOfType<Carrier_Controller>())
		{
			carrier.GetComponent <Rigidbody>().isKinematic = true;
		}
		FillStack();
		InvokeRepeating("SpawnMite", 1f, 10f);
	}
	void FillStack()
	{
		for (int i = 0; i < maxMites; i++)
		{
			GameObject _mite = Instantiate(mitePrefab, nest.transform.position, nest.transform.rotation);
			_mite.SetActive(false);
			_mite.GetComponent<MetworkView>().viewID = startMetID + i;
			_mite.transform.parent = this.transform;
			mites.Push(_mite);
			
		}
	}

	// Update is called once per frame
	void SpawnMite () {
		if (mites.Count > 0)
		{
			if (Metwork.peerType != MetworkPeerType.Disconnected||Metwork.isServer)
			{
				netView.RPC("RPC_SpawnMite", MRPCMode.AllBuffered, new object[] { });
			}
			else
			{
				RPC_SpawnMite();
			}
		}
	}
	[MRPC]
	public void RPC_SpawnMite()
	{
		GameObject _mite = mites.Pop();
		_mite.SetActive(true);
		_mite.transform.position = nest.transform.position;
	}
	public void Evolve()
	{

	}
}

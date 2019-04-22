using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_Manager : MonoBehaviour
{
	[Range(0.01f,5f)]
	[Tooltip("The number of seconds between each slow update")]
	public float slowUpdateTime = 1f;
	protected Player_Controller[] players;
	/// <summary>Do not override</summary>Start is called before the first frame update
	void Start()
    {
		players = FindObjectsOfType<Player_Controller>();
		MapSetup();
		Invoke("DelayedSetup",1f);
		StartCoroutine("CheckSlowUpdate");

	}
	///<summary>Use instead of the start function</summary>
	protected virtual void MapSetup()
	{

	}
	///<summary>Use when you need to execute tasks that do not need to happen exactly at the start</summary>
	protected virtual void DelayedSetup()
	{

	}

	// Update is called once per frame
	void Update()
    {
       
    }
	private IEnumerator CheckSlowUpdate()
	{
		while (true)
		{
			yield return new WaitForSeconds(slowUpdateTime);
			SlowUpdate();
		}
	}
	//a slower version of update
	protected virtual void SlowUpdate(){
       
    }
}

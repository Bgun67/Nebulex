using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Splash_Scene_Controller : MonoBehaviour {
	public Text startText;

	// Use this for initialization
	void Start () {

		StartCoroutine (DisplayStartText());
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			
				SceneManager.LoadScene ("Start Scene");
		}
	}
	IEnumerator DisplayStartText(){
		
		yield return new WaitForSeconds(0.1f);
		while (true){
			yield return new WaitForSeconds(0.5f);
			startText.enabled = !startText.enabled;
		}
	}
}

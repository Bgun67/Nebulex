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
		//FindObjectOfType<ParticleSystem> ().Play ();
		//FindObjectOfType<Animator> ().SetTrigger("Play");

		InvokeRepeating ("DisplayStartText", 3f, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			
				SceneManager.LoadScene ("Start Scene");
		}
	}
	void DisplayStartText(){
		
		startText.enabled = !startText.enabled;
	}
}

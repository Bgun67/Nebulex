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
		Invoke("NextScene", 6f);
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown) {
			if(SceneManager.GetActiveScene().name == "DiPolar Scene"){
				SceneManager.LoadScene ("Splash Scene");
			}
			else{
				SceneManager.LoadScene ("Start Scene");
			}
		}
	}

	void NextScene(){
		if(SceneManager.GetActiveScene().name == "DiPolar Scene"){
				SceneManager.LoadScene ("Splash Scene");
		}
	}
	
}

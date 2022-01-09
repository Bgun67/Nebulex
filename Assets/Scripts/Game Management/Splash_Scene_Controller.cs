using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;


public class Splash_Scene_Controller : MonoBehaviour {
	public Text startText;
	public Image shadow;
	float initialTime = 0;

	// Use this for initialization
	void Start () {
		if(SceneManager.GetActiveScene().name == "DiPolar Scene"){
				Invoke("NextScene", 6f);
		}
		
		initialTime = Time.time;
		
		
	}
	
	// Update is called once per frame
	void Update () {
		//if(SceneManager.GetActiveScene().name == "DiPolar Scene"){
			
		//	if (Input.anyKeyDown) {
			
		//		SceneManager.LoadScene ("Splash Scene");
		//	}
			
		//}
		if(SceneManager.GetActiveScene().name == "Start Scene"){
			shadow.color = new Color(0,0,0, Mathf.Clamp01(1.0f - (Time.time - initialTime) * 0.333f));
		}

	}

	public void NextScene(){
		if(SceneManager.GetActiveScene().name == "DiPolar Scene"){
				SceneManager.LoadScene ("Splash Scene");
		}
		else{
			SceneManager.LoadScene ("Start Scene");
		}
	}
	
	
}

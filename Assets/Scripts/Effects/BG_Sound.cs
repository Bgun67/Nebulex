using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BG_Sound : MonoBehaviour
{
    float volume = 0f;
    void Start () {
		if(FindObjectsOfType<BG_Sound>().Length > 1){
			Destroy(this.gameObject);
		}
		else{
			volume = this.GetComponent<AudioSource>().volume;
			DontDestroyOnLoad (this.gameObject);
		}
	}
	void Update()
	{
		if(Time.time > 8f && !GetComponent<AudioSource>().isPlaying){
			GetComponent<AudioSource>().Play();
		}
		this.GetComponent<AudioSource>().volume = volume;
		if (FindObjectOfType<Game_Controller>() != null || SceneManager.GetActiveScene().name == "LobbyScene" || SceneManager.GetActiveScene().name == "TransistionScene")
		{
			volume = Mathf.Lerp(volume, 0f, 0.05f);
		}
		
		if(volume < 0.05f){
			Destroy(this.gameObject);
		}
	}
}

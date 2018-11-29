using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warning : MonoBehaviour {
	public bool turnUp;
	public bool warningActive;
	public float intensity;
	public GameObject[] lightObjList;
	public Light[] lightList;

	// Use this for initialization
	void Start () {
		lightObjList = GameObject.FindGameObjectsWithTag ("Warning Light");
		lightList = new Light[lightObjList.Length];
		int i = 0;
		foreach(GameObject light in lightObjList){
			lightList [i] = light.GetComponent<Light> ();
			i++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (warningActive == true) {
			StartCoroutine (FlashWarningLight ());
		}
	}
	IEnumerator FlashWarningLight(){
		//while (warningActive == true) {
			if (intensity <= 0f) {
				turnUp = true;
			} else if (intensity >= 1f) {
				turnUp = false;
			}
			if (turnUp == false) {
				intensity -= 0.05f;
			} else {
				intensity += 0.05f;
			}

			foreach (Light light in lightList) {
				light.intensity = intensity;
			}


			yield return new WaitForSeconds (1f);

		//}
	}
}

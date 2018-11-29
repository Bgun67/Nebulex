using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holodesk_Controller : MonoBehaviour {
	public bool on;
	public GameObject shipOneModel;
	public GameObject shipTwoModel;
	public Transform shipOneTransform;
	public Transform shipTwoTransform;
	public GameObject enemyShips;
	public Ship_Controller[] friendlyShips;
	public GameObject[] friendlyShipModels;
	public float sizeFactor;
	public LineRenderer[] projectors;
	[Tooltip("The maximum global distance the object is shown away from the middle ship")]
	public float maxDistance;
	// Use this for initialization
	void Start () {
		friendlyShips = GameObject.FindObjectsOfType<Ship_Controller> ();
		projectors = Transform.FindObjectsOfType<LineRenderer> ();

	}
	
	// Update is called once per frame
	void Update () {
		if (on) {
			if(Time.frameCount%2 == 0){

				shipTwoModel.transform.localPosition = (shipTwoTransform.position - shipOneTransform.position) * sizeFactor;
				for(int i = 0; i<friendlyShips.Length; i++) {
					friendlyShipModels[i].transform.localPosition = (friendlyShips[i].transform.position - shipOneTransform.position) * sizeFactor;

				}
				foreach (LineRenderer projector in projectors) {
					projector.SetPosition (0, projector.transform.position);
					projector.SetPosition (1, this.transform.position);
					if (Vector3.Magnitude (projector.transform.position - shipOneModel.transform.position) > maxDistance) {
						projector.gameObject.SetActive (false);
					} else {
						projector.gameObject.SetActive (true);

					}
					//shipOne.transform.position = this.transform.position;
				}
			}

		} else {
			shipOneModel.SetActive (false);
			shipTwoModel.SetActive (false);
		}
	}

}

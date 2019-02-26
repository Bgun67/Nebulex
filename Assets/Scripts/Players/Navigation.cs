using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class Navigation : MonoBehaviour {

	[System.Serializable]
	public struct Target
	{
		public Transform _transform;
		public string _name;
		public float _distance;
		public Color _colour;
		public RectTransform _image;
	}

	public static List<Target> targets = new List<Target>();
	public Sprite targetImage;
	public Sprite arrowImage;
	Camera cam;

	[SerializeField]
	public static RectTransform baseImage;

	//Doubles will be accounted for
	[Tooltip ("These need only go on one instance and will be registered to all the instances")]
	public List<Target> targetsToRegister = new List<Target>();

	public static void RegisterTarget(Transform _transform,string _name, float _distance, Color _colour){
		Target _target = new Target ();
		_target._transform = _transform;
		_target._name = _name;
		_target._colour = _colour;
		_target._distance = _distance;
		_target._image = Instantiate (baseImage.gameObject, baseImage.transform.parent).GetComponent<RectTransform>();
		_target._image.gameObject.SetActive (true);
		_target._colour.a = 0.5f;
		_target._image.GetComponent<Image> ().color = _target._colour;
		_target._image.GetComponentInChildren<Text> ().text = _target._name;
		_target._image.GetComponentInChildren<Text> ().color = _target._colour;

		targets.Add (_target);
	}

	public static void DeregisterTarget(Transform _transform){
		for (int i = 0; i < targets.Count; i++) {
			if (targets [i]._transform == _transform) {
				Destroy(targets[i]._image.gameObject);
				targets.RemoveAt (i);
				break;
			}
		}
	}

	void Start(){
		foreach (Target target in targetsToRegister) {
			if(!targets.Contains(target)){
				targets.Add (target);
				baseImage = targets[0]._image;
				target._image.GetComponent<Image> ().color = target._colour;
				target._image.GetComponentInChildren<Text> ().text = target._name;
				target._image.GetComponentInChildren<Text> ().color = target._colour;

			}
		}

	}
	
	// Update is called once per frame
	void OnPreRender () {
		if (cam == null) {
			cam = this.GetComponent<Camera> ();
		}
		//This is there almost exclusively for the scene camera
		if (!this.cam.enabled) {
			return;
		}




		//Here we are defining a frustum of four planes
		//By taking the dot product of the normal of each plane with the distance vector of the point
		//We can see if the point is in front of the plane

		//Get frustum vectors
		Vector3 topL = cam.ViewportPointToRay (new Vector3 (0, 1, 0)).direction;
		Vector3 topR = cam.ViewportPointToRay (new Vector3 (1, 1, 0)).direction;
		Vector3 botL = cam.ViewportPointToRay (new Vector3 (0, 0, 0)).direction;
		Vector3 botR = cam.ViewportPointToRay (new Vector3 (1, 0, 0)).direction;


		//Define planes normals
		Vector3 normalUp = -Vector3.Cross(topL, topR);
		Vector3 normalDown = -Vector3.Cross(botR, botL);
		Vector3 normalLeft = -Vector3.Cross(botL, topL);
		Vector3 normalRight = -Vector3.Cross(topR, botR);


		//Define point distance vector
		Vector3 distVect;

		for (int i = 0; i < targets.Count; i++) {
			//Check if we are far enough away from the target
			distVect = targets [i]._transform.position - cam.transform.position;
			if (distVect.sqrMagnitude < targets[i]._distance * targets[i]._distance) {
				targets [i]._image.gameObject.SetActive (false);
				continue;
			}

			bool isVisible = true;
			//Check visibility
			if (Vector3.Dot (normalUp, distVect) < 0 ||
			    Vector3.Dot (normalDown, distVect) < 0 ||
			    Vector3.Dot (normalLeft, distVect) < 0 ||
			    Vector3.Dot (normalRight, distVect) < 0) {
				isVisible = false;
			}

			//If the target is visible show a image over the target

			targets [i]._image.gameObject.SetActive (true);


			//first you need the RectTransform component of your canvas
			RectTransform CanvasRect = targets [i]._image.GetComponentInParent<Canvas> ().GetComponent<RectTransform> ();
		
			float _flipNegative = 1f;
			//dot the forward vector of the camera with the target to see if it needs to be flipped
			if(Vector3.Dot(cam.transform.forward, targets[i]._transform.position-cam.transform.position)<0f && !isVisible){
				_flipNegative = -1f;
			}
			//then you calculate the position of the UI element
			//0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
			Vector2 ViewportPosition = cam.WorldToViewportPoint (targets [i]._transform.position);

			



			if (isVisible) {
				targets [i]._image.GetComponent<Image> ().sprite = targetImage;

			} else {
				targets [i]._image.GetComponent<Image> ().sprite = arrowImage;
				//ViewportPosition = ViewportPosition.normalized;
				if(_flipNegative < 0){
				//	ViewportPosition = new Vector2(ViewportPosition.y,ViewportPosition.x);
				}

				//float max = Mathf.Max (Mathf.Abs(ViewportPosition.x), Mathf.Abs(ViewportPosition.y));
				//ViewportPosition.x = Mathf.Clamp01(ViewportPosition.x / max);
				//ViewportPosition.y = Mathf.Clamp01(ViewportPosition.y/max);
				
				if(Vector3.Dot (normalUp, distVect) < 0){
					ViewportPosition.y = 1f;
				}
				if(Vector3.Dot (normalDown, distVect) < 0){
					ViewportPosition.y = 0f;
				}
				if(Vector3.Dot (normalLeft, distVect) < 0){
					ViewportPosition.x = 0f;
				}
				if(Vector3.Dot (normalRight, distVect) < 0){
					ViewportPosition.x = 1f;
				}
			}

			targets [i]._image.anchorMin = ViewportPosition;
			targets [i]._image.anchorMax = ViewportPosition;
			

		}


	}
}

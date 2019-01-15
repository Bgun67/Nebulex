using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RemoveCapsule : MonoBehaviour {

	public bool start = false;
	public GameObject[] objectsToStrip;
	public Mesh mesh;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnGUI(){
		if (start) {
			start = false;
			//OffsetMetViews (-200);
			//FixNetViews ();
			//foreach(MeshFilter meshF in GameObject.FindObjectsOfType<MeshFilter>()){
			//	if(meshF.GetInstanceID() == 227858){
			//		print (meshF.gameObject.transform.parent.name +"/"+ meshF.name);
			//	}
			//}
			//print ("Complete");
			//ApproximateCylinder(this.transform, 2.1f, 1f, 16f, 16, this.gameObject);
		}
	}

	void OffsetMetViews(int offset){
		foreach (GameObject objectToStrip in objectsToStrip) {
			foreach (MetworkView _view in objectToStrip.GetComponentsInChildren<MetworkView>()) {
				_view.viewID += offset;
			}
		}
	}

	void FixNetViews(){
		/*foreach (NetworkView view in GameObject.FindObjectsOfType<NetworkView>()) {
			
			if (view.observed != null || view.stateSynchronization != 0) {
				print ("Ammending: " + view.transform.root.name + "/" + view.name);
				view.observed = null;
				view.stateSynchronization = 0;
			}
			if (view.GetComponents<NetworkView> ().Length > 1) {
				print ("More than one collider on: " + view.transform.root.name + "/" + view.name);
			}

		}*/
	}

	void ApproximateCylinder(Transform center, float radius,float width, float height, int numBoxes,GameObject obj){
		BoxCollider[] boxes = new BoxCollider[numBoxes];
		Vector3[] positions = new Vector3[numBoxes];

		//Follow z-forward, y-up, x-right convention
		float yPosBottom = center.position.y - height/2f;
		float yPosTop = center.position.y + height / 2f;

		for (int i = 0; i < numBoxes; i++) {
			positions[i].x = radius * Mathf.Cos(i * 2f*Mathf.PI / (float)numBoxes);

			positions[i].z = radius * Mathf.Sin(i * 2f*Mathf.PI / (float)numBoxes);
			print (positions [i].y);
			positions [i].y = 0;//center.position.y;
		}

		for (int i = 0; i < numBoxes; i++) {
			GameObject newObj = (GameObject)GameObject.Instantiate (new GameObject ("Collider"));
			newObj.transform.position = obj.transform.position;
			newObj.transform.rotation = obj.transform.rotation;

			newObj.transform.parent = obj.transform;


			BoxCollider collider = newObj.AddComponent<BoxCollider> ();
			newObj.transform.position += positions [i];
			collider.size = new Vector3 ( 0.1f,width, height);
			newObj.transform.Rotate (new Vector3 (0,0, -(180f/Mathf.PI) * i * 2f*Mathf.PI / (float)numBoxes));

		}

	}
}

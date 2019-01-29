using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Michael G: 2018-01-23
 * Creates colliders from a given mesh to create a mesh collider that can be used on non-kinematic rigidbodies
 * Notes: Don't put on a sphere, Meshes should probably be triangulated.
 * This code is covered under copyright laws
 * Improvements/Fixes: 
 * */
[ExecuteInEditMode]
public class Collider_Creator : MonoBehaviour {

	//The mesh to be turned into a mesh collider
	[Tooltip("If no mesh specified, the default mesh will be used")]
	public Mesh mesh;
	//The gameobject to add the mesh to
	public GameObject output;

	float threshold = 0.1f;


	public bool bake = false;
	[Tooltip("Use caution, this will clear ALL meshcolliders from output")]
	public bool clear = false;

	void OnEnable(){
		if (output == null) {
			output = this.gameObject;
		}
		if (clear) {
			print ("Clearing " + output.GetComponents<MeshCollider> ().Length + " mesh colliders");
			//Remove the previous colliders
			foreach (MeshCollider collider in output.GetComponents<MeshCollider>()) {
				DestroyImmediate (collider);

			}
			clear = false;
		}
	}

	// Use this for initialization
	void OnGUI () {

		if (output == null) {
			output = this.gameObject;
		}
		if (clear) {
			print ("Clearing " + output.GetComponents<MeshCollider> ().Length + " mesh colliders");
			//Remove the previous colliders
			foreach (MeshCollider collider in output.GetComponents<MeshCollider>()) {
				DestroyImmediate (collider);

			}
			clear = false;
		}

		if (!bake) {
			return;
		}

		bake = false;

		output = this.gameObject;

		//Remove the previous colliders
		foreach (MeshCollider collider in output.GetComponents<MeshCollider>()) {
			//if (collider.hideFlags == HideFlags.HideInInspector) {
				DestroyImmediate (collider);
			//}
		}





		//Temporary variable: holds one face at a time
		Mesh tmpMesh;

		Vector3 vertexA;
		Vector3 vertexB;
		Vector3 vertexC;




		if (mesh == null) {
			//If no mesh given, default to mesh filters
			mesh = this.GetComponent<MeshFilter> ().sharedMesh;
		}

		if (mesh == null) {
			Debug.LogError ("Gameobject: " + this.gameObject + " has no mesh");
			return;
		}


		int[] meshTriangles = mesh.triangles;
		int meshTrianglesLength = mesh.triangles.Length;
		Vector3[] meshVertices = mesh.vertices;
		int[] trianglesOrder = new int[]{0,1,2,3,4,5};

		//There are 3 vertex indices for each triangle
		for (int i = 0; i < meshTrianglesLength - 2; i+= 3) {

			tmpMesh = new Mesh ();
			//For bookkeeping and error locating reasons
			tmpMesh.name = gameObject.name + "_" + i.ToString ();



			vertexA = meshVertices [meshTriangles [i]];
			vertexB = meshVertices [meshTriangles [i + 1]];
			vertexC = meshVertices [meshTriangles [i + 2]];

			//if (Vector3.Distance (vertexA, vertexB) < 0.01f) {
			//	return;
			//}
			//if (Vector3.Distance (vertexB, vertexC) < 0.01f) {
			//	return;
			//}
			//if (Vector3.Distance (vertexC, vertexA) < 0.01f) {
			//	return;
			//}



			if (i <= 87) {
				Debug.DrawLine (output.transform.TransformPoint (vertexA), output.transform.TransformPoint (vertexB), new Color(255f,0,255f), 10f);
				Debug.DrawLine (output.transform.TransformPoint (vertexB), output.transform.TransformPoint (vertexC), new Color(255f,0,255f), 10f);
				Debug.DrawLine (output.transform.TransformPoint (vertexC), output.transform.TransformPoint (vertexA), new Color(255f,0,255f), 10f);
			}

			//Cross both edges to find a perpendicular edge
			Vector3 crossProduct = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
			Vector3 unitCrossProduct = crossProduct.normalized;

			float crossProductSize = 0.002f;

			//Create four vertices, three from the face and one that is slightly off the third vertex. 
			//This vertex can be improved by putting it at an orthogonal to the other two edges 
			tmpMesh.vertices = new Vector3[]{vertexA,vertexB,vertexC, vertexA + unitCrossProduct * crossProductSize, vertexB + unitCrossProduct * crossProductSize,vertexC + unitCrossProduct * crossProductSize
			};

			//Two triangles formed from this face, triangle with vertices at indexes 0,1,2 in vertices and 0,1,3
			tmpMesh.triangles = trianglesOrder;



			MeshCollider meshCollider = output.AddComponent<MeshCollider> ();
			meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;

			meshCollider.sharedMesh = tmpMesh;


			meshCollider.convex = true;


			//Hide the component so it doesn't bog down the inspector
			meshCollider.hideFlags = HideFlags.HideInInspector;

			
		}
		int _numMeshColliders = 0;
		foreach(MeshCollider _mc in output.GetComponents<MeshCollider>()){
			_numMeshColliders ++;
		}
		print(_numMeshColliders);






	}
	

}

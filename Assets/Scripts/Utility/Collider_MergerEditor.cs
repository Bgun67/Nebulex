#if UnityEditor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


public class Collider_MergerEditor : EditorWindow {
	public LayerMask layerMask;
	public List<MeshCollider> meshColliders = new List<MeshCollider>();

	[MenuItem("Example/Collider Merger")]
    static void Init()
    {
        Collider_MergerEditor window = (Collider_MergerEditor)EditorWindow.GetWindow(typeof(Collider_MergerEditor));
        window.Show();
    }

	void OnEnable(){
		SceneView.onSceneGUIDelegate += SceneGUI;
	}
	// Update is called once per frame
	void SceneGUI (SceneView sceneView) {
		

		if(Event.current.type == EventType.MouseDown){
			
			Ray ray = Camera.current.ScreenPointToRay(Event.current.mousePosition);
			RaycastHit hit = new RaycastHit();
         	if (Physics.Raycast(ray, out hit, 1000.0f, layerMask, QueryTriggerInteraction.Ignore)) {
				 Debug.DrawLine(Camera.current.transform.position, hit.point, Color.blue, 1f);
				 Debug.Log(hit.collider.gameObject);
				 if(!meshColliders.Contains((MeshCollider)hit.collider)){
				 	meshColliders.Add((MeshCollider)hit.collider);
				 }
         	}
		}


		foreach(MeshCollider _meshCollider in meshColliders){
			
			Graphics.DrawMesh(_meshCollider.sharedMesh, Matrix4x4.TRS(_meshCollider.transform.position, _meshCollider.transform.rotation, _meshCollider.transform.lossyScale*1.05f), new Material(Shader.Find("Diffuse")), _meshCollider.gameObject.layer, Camera.current);
		}
	
	}
	void OnGUI(){
		LayerMask tempMask = EditorGUILayout.MaskField( InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
		layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
		EditorGUILayout.IntField(meshColliders.Count);
		//foreach(MeshCollider _meshCollider in meshColliders){
		//	EditorGUILayout.ObjectField(_meshCollider);
		//}

		
	}

}
#endif

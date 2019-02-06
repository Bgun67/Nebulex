#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

#if UNITY_EDITOR||UNITY_EDITOR_64
[ExecuteInEditMode]
public class Gun_Creator : ScriptableWizard {
	public string name;
	public GameObject gunModel;
	GameObject gunPrefab;
	public List<string> scopeNames = new List<string>();
	public bool[] isScopeActive;
	Loadout_Controller sceneLoadout;
	GameObject[] rootObjects;

	[MenuItem("GameObject/Gun Creator")]
	// Use this for initialization
	static void CreateWizard(){
		ScriptableWizard.DisplayWizard<Gun_Creator> ("Create Gun", "Create");
	

	
	}
	void Awake(){
		scopeNames.Clear ();
		 rootObjects = SceneManager.GetSceneByName ("Loadout Scene").GetRootGameObjects ();
		 sceneLoadout = new Loadout_Controller ();
		foreach (GameObject GO in rootObjects) {
			if (GO.GetComponent<Loadout_Controller> () != null) {
				sceneLoadout = GO.GetComponent<Loadout_Controller> ();
			}
		}
		isScopeActive = new bool[sceneLoadout.primaryScopesObjs.Length];
		
		//bool scopesEnabled = EditorGUILayout.BeginToggleGroup ("Scopes", true);
		for (int i = 0; i < sceneLoadout.primaryScopesObjs.Length; i++) {
			GameObject scope = sceneLoadout.primaryScopesObjs [i];
			//isScopeActive [i] = EditorGUILayout.Toggle (scope.name, isScopeActive [i]);
			scopeNames.Add (scope.name);

		}
		//EditorGUILayout.EndToggleGroup ();


	}

	void OnWizardCreate () {
		


		gunPrefab = Instantiate (gunModel);
		gunPrefab.name = name;
		Fire fireScript = gunPrefab.AddComponent<Fire> ();



		fireScript.unavailableScopes.Clear ();
		for (int i = 0; i < isScopeActive.Length; i++) {
			if (isScopeActive[i]) {
			} else {
				fireScript.unavailableScopes.Add (scopeNames[i]);
			}
		}


		GameObject scopePosition = new GameObject ();
		scopePosition.name = "Scope";
		scopePosition.transform.parent = gunPrefab.transform;
		scopePosition.transform.localPosition = gunPrefab.transform.up * 1f;;

		GameObject shotSpawn = new GameObject ();
		shotSpawn.name = "Shot Spawn";
		Vector3 extents = Vector3.zero;
		try{
		 extents = gunPrefab.GetComponent<MeshRenderer> ().bounds.extents;
		}
		catch{
			extents = gunPrefab.GetComponentInChildren<MeshRenderer> ().bounds.extents;

		}
		Vector3 position = Vector3.zero;
		if (extents.x > extents.y) {
			if (extents.x > extents.z) {
				position = new Vector3 (extents.x, 0f, 0f);
			} else {
				position = new Vector3 (0f, 0f, extents.z);

			}
		} else {
			if (extents.y > extents.z) {
				position = new Vector3 ( 0f,extents.y, 0f);
			} else {
				position = new Vector3 (0f, 0f, extents.z);

			}
		}

		shotSpawn.transform.position = gunPrefab.transform.position+ position;
		shotSpawn.transform.parent = gunPrefab.transform;

		gunPrefab.AddComponent<AudioSource> ();

		MetworkView netView = gunPrefab.AddComponent<MetworkView> ();
		//TODO: Update
		//netView.stateSynchronization = 0;
		//netView.observed = null;

		Loadout_Controller.gunLocalPositions.Add (name, Vector3.zero);
		Loadout_Controller.gunLocalRotations.Add (name, Vector3.zero);



		fireScript.bulletPrefab = (GameObject)Resources.Load("Weapons/Ammunition/Bullet");



	}
	

}
#endif

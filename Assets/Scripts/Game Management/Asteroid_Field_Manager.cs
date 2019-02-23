#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading;

#if UNITY_EDITOR || UNITY_EDITOR_64
[ExecuteInEditMode]
public class Asteroid_Field_Manager : ScriptableWizard {
	

	public float worldSize = 10000f;
	public float minYDistance = 50f;
	public float maxYDistance = 100f;
	public float minAsteroidSize = 2f;
	public float maxAsteroidSize=12f;
	float asteroids;
	[Range(0,900)]
	public float maxAsteroids = 900;
	[Range(0,10)]
	public int asteroidToJunkRatio = 0;
	public GameObject[] asteroidPrefabs;
	public GameObject[] junkPrefabs;
	public bool destroyField = false;
	public bool showFlags = false;
	public bool hideFlags = false;

	public float progress = 0f;
	

	[MenuItem("GameObject/Asteroid Field Manager")]
	// Use this for initialization
	static void CreateWizard(){
		ScriptableWizard.DisplayWizard<Asteroid_Field_Manager> ("Generate Field", "Create");
	
	}
		void OnWizardCreate () {
			if(destroyField){
				DestroyField();
			}
			else if(showFlags){
				foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
				{
					if(go.name == "Space Junk"){
						go.hideFlags = HideFlags.None;
					}
				}
			}
			else if(hideFlags){
				foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
				{
					if(go.name == "Space Junk"){
					go.hideFlags = HideFlags.HideInHierarchy;
					}
				}
			}
			else{
				GenerateAsteroids();
			}
		}
		void ShowFlags(){

		}
	public void GenerateAsteroids(){
		GameObject sector1 = new GameObject();
		GameObject prefab = new GameObject();

		sector1.name = "Asteroid Sector";
		sector1.SetActive(false);

		while (asteroids <= maxAsteroids) {
			
			bool _isAsteroid = Random.Range(0f,10f)>asteroidToJunkRatio;
			if(_isAsteroid){
			 prefab = asteroidPrefabs[Random.Range(0,junkPrefabs.Length-1)];
			}
			else{
			prefab = junkPrefabs[Random.Range(0,asteroidPrefabs.Length-1)];
			}
			Vector3 center = Vector3.zero;
			Vector3 position = new Vector3(0f,0f,0f);

			float _scale = 1;
			if(_isAsteroid){
				_scale = Random.Range (minAsteroidSize, maxAsteroidSize);
			}

			if(asteroids%2 == 0){
					center+=Vector3.up *Random.Range(minYDistance+_scale/2f, maxYDistance);
				}
				else{
					center+=Vector3.up *-Random.Range(minYDistance+_scale/2f, maxYDistance);

				}

			for (int i = 0; i<5; i++) {
				position = center+new Vector3(Random.Range(-worldSize,worldSize),0f, Random.Range(-worldSize, worldSize));
				if (!Physics.CheckBox (position, prefab.transform.lossyScale * _scale / 2f)) {
			
					GameObject asteroid = GameObject.Instantiate (prefab, position, Quaternion.identity);
			asteroid.transform.localScale = Vector3.one*_scale;
			asteroid.transform.rotation = Random.rotation;
			asteroid.name = "Space Junk";
			asteroid.isStatic = true;
			asteroid.hideFlags = HideFlags.HideInHierarchy;
			asteroid.transform.parent = sector1.transform;
			break;
				}

			}
			asteroids++;
			if(asteroids%50 == 0){
				progress = asteroids/maxAsteroids;
			}

			
		}
				sector1.SetActive(true);
	}
	void DestroyField(){
		DestroyImmediate(GameObject.Find("Asteroid Sector"));
	}
}
#endif



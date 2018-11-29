using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Loadout_Controller : MonoBehaviour {
	public static Dictionary<string, Vector3> gunLocalPositions = new Dictionary<string,Vector3>()
	{
		{"SRR-3", new Vector3(-0.08f,0.08f,-0.33f)},
		{"Oynix-93", new Vector3(-0.207f,0.222f,0.417f)},
		{"Ratpak", new Vector3(-0.15f,0f,0.25f)},
		{"NTO-MSQ0", new Vector3(-0.13f,0.4f,0.16f)},
		{"Biron", new Vector3(-0.13f,0.4f,0.16f)},


		{"FN-227", new Vector3(-0.12f,-0.23f,-0.05f)},
		{"Q-338", new Vector3(0f,0.11f,-0.09f)},

		{"Thunderstroke", new Vector3(-0.24f,-0.27f,1.36f)},
		{"ATX-Heavy", new Vector3(-0.26f,0.08f,0.94f)},


		{"SIT", new Vector3(-0.164f,0.0962f,0.3289f)}


	};
	public static Dictionary<string, Vector3> gunLocalRotations = new Dictionary<string,Vector3>()
	{
		{"SRR-3", new Vector3(168.05f,262.4f,198.69f)},
		{"Oynix-93", new Vector3(20.133f,-13.94f,-7.824f)},
		{"Ratpak", new Vector3(6.212f,81.495f,16.55f)},
		{"NTO-MSQ0", new Vector3(18.69f,-12.46f,-8.8f)},
		{"Biron", new Vector3(18.69f,-12.46f,-8.8f)},



		{"FN-227", new Vector3(14.13f,-13.1f,-8.2f)},
		{"Q-338", new Vector3(9.92f,77.481f,59.078f)},

		{"Thunderstroke", new Vector3(-67.67f,12.501f,-19.18f)},
		{"ATX-Heavy", new Vector3(21.191f,-8.791f,-7.971f)},



		{"SIT", new Vector3(11.55f,-9.26f,-7.1f)}
	};



	[Header("Primary")]

	public GameObject[] primaryGunObjs;
	public Transform primaryParent;
	public GameObject primaryButton;


	[Header("Secondary")]
	public GameObject[] secondaryGunObjs;
	public Transform secondaryParent;
	public GameObject secondaryButton;

	[Header("Scopes")]

	public GameObject[] primaryScopesObjs;
	public Transform primaryScopesParent;
	public GameObject scopesButton;

	public GameObject[] secondaryScopesObjs;
	public Transform secondaryScopesParent;

	[Header("Other")]
	public GameObject[] otherObjs;
	public Transform otherParent;
	public GameObject otherButton;

	public int selectedGun = 0;
	public Vector2 initialPosition;
	public Vector2 previousPosition;
	public GameObject buttonPrefab;
	public string[] loadoutData;
	public int skillLevel;
	public Transform finger;
	GameObject weapon;
	public string[] originalData;
	public Text weaponDataText;

	// Use this for initialization
	void Start () {
		LoadData ();
		try{
			skillLevel = int.Parse(Util.LushWatermelon(System.IO.File.ReadAllLines (Application.streamingAssetsPath+"/Player Data.txt"))[1]);
		}
		catch{ Debug.Log("Encountered error in"+this.name);
			skillLevel = int.Parse(Profile.RestoreDataFile ()[1]);
		}
		previousPosition = initialPosition;
		InstantiateButtons(primaryGunObjs, Gun_Button_Controller.ButtonTypes.Primary, primaryParent);
		InstantiateButtons(secondaryGunObjs, Gun_Button_Controller.ButtonTypes.Secondary, secondaryParent);
		InstantiateButtons(primaryScopesObjs, Gun_Button_Controller.ButtonTypes.Scope, primaryScopesParent);
		InstantiateButtons(secondaryScopesObjs, Gun_Button_Controller.ButtonTypes.Scope, secondaryScopesParent);
		SceneManager.SetActiveScene (SceneManager.GetSceneByName ("Loadout Scene"));

		//BackClicked ();
				
	}
	void Update(){
		transform.Rotate (0f, 1f, 0f);
	}
	void LoadData(){
		try{
		loadoutData = Util.LushWatermelon(System.IO.File.ReadAllLines ( Application.streamingAssetsPath+"/Loadout Settings.txt"));
		}
		catch{ Debug.Log("Encountered error in"+this.name);
			loadoutData = Profile.RestoreLoadoutFile ();
		}
		originalData = new string[loadoutData.Length];
		for(int i = 0; i<loadoutData.Length; i++) {
			string line = loadoutData [i];
			originalData [i] = line;
		}
	}
	public void ShowWeapon(){
		try{	
			if (weapon != null) {
				Destroy (weapon);
				weapon = null;
			}
			if (selectedGun == 0) {
				GameObject primaryWeaponPrefab = (GameObject)Resources.Load ("Weapons/" + loadoutData [0]);
				Vector3 primaryLocalPosition = gunLocalPositions [loadoutData [0]];
				Vector3 primaryLocalRotation = gunLocalRotations [loadoutData [0]];
				//add the primary weapon to the finger
				weapon = (GameObject)Instantiate (primaryWeaponPrefab, finger);
				weapon.transform.localPosition = primaryLocalPosition;
				weapon.transform.localRotation = Quaternion.Euler (primaryLocalRotation);
				GameObject primaryScope = (GameObject)Resources.Load ("Weapons/Scopes/" + loadoutData [2]);
				Instantiate (primaryScope, weapon.GetComponent<Fire> ().scopePosition);


			} else {
				GameObject secondaryWeaponPrefab = (GameObject)Resources.Load ("Weapons/" + loadoutData [1]);
				Vector3 secondaryLocalPosition = gunLocalPositions [loadoutData [1]];
				Vector3 secondaryLocalRotation = gunLocalRotations [loadoutData [1]];
				//add the primary weapon to the finger
				weapon = (GameObject)Instantiate (secondaryWeaponPrefab, finger);
				weapon.transform.localPosition = secondaryLocalPosition;
				weapon.transform.localRotation = Quaternion.Euler (secondaryLocalRotation);
				GameObject secondaryScope = (GameObject)Resources.Load ("Weapons/Scopes/" + loadoutData [3]);
				Instantiate (secondaryScope, weapon.GetComponent<Fire> ().scopePosition);
			}
			DisplayGunInfo();
		} catch {
			print ("Sorry. Braden.exe has encountered an error");
			loadoutData = Profile.RestoreLoadoutFile ();
		}
	}
	void DisplayGunInfo(){
		Fire tmpFire = weapon.GetComponent<Fire> ();
		weaponDataText.text = 
			("<b><size=16>" + weapon.name.Replace ("(Clone)", "") + "</size></b>\n" +
		"Fire Rate: " + tmpFire.fireRate + "\n" +
		"Reload Time: " + tmpFire.reloadTime + "\n" +
		"Damage: " + tmpFire.damagePower + "\n" +
		"Bullet Velocity: " + tmpFire.bulletVelocity + "\n" +
		"Fire Type: " + tmpFire.fireType.ToString () + "\n" +
		"Mag Size: " + tmpFire.magSize+"\n");
		if (tmpFire.bulletPrefab.GetComponent<Bullet_Controller> ().isExplosive) {
			weaponDataText.text += "Explosive Rounds\n";
		}



	}
	public void MouseOver(string text, Gun_Button_Controller.ButtonTypes type ){
		
		if (type == Gun_Button_Controller.ButtonTypes.Primary) {
			loadoutData [0] = text;
		} else if (type == Gun_Button_Controller.ButtonTypes.Secondary) {
			loadoutData [1] = text;
		}
		else if (type == Gun_Button_Controller.ButtonTypes.Scope) {
			if (selectedGun == 0) {
				loadoutData [2] = text;
			} else {
				loadoutData [3] = text;
			}
		}
		ShowWeapon ();

	}
	public void MouseLeave(){
		for(int i = 0; i<loadoutData.Length; i++) {
			string line = originalData [i];
			loadoutData [i] = line;
		}
		ShowWeapon ();
	}
	// Update is called once per frame
	public void PrimaryButtonClicked (string buttonText) {
		loadoutData [0] = buttonText;
		SaveData ();
		ShowAttachmentButtons ();
	}
	public void SecondaryButtonClicked (string buttonText) {
		loadoutData [1] = buttonText;
		SaveData ();
		ShowAttachmentButtons ();
	}
	public void ScopesButtonClicked (string buttonText) {
		if (selectedGun == 0) {
			loadoutData [2] = buttonText;
		} else {
			loadoutData [3] = buttonText;
		}
		SaveData ();
		ShowWeapon ();
	}
	public void PrimaryClicked(){
		LoadData ();
		selectedGun = 0;
		HideAllButtons ();
		primaryParent.gameObject.SetActive (true);
		ShowWeapon ();
	}
	public void SecondaryClicked(){
		LoadData ();
		selectedGun = 1;
		HideAllButtons ();
		secondaryParent.gameObject.SetActive (true);
		ShowWeapon ();

	}
	public void ScopesClicked(){
		LoadData ();

		HideAllButtons ();
		foreach (Gun_Button_Controller button in primaryScopesParent.GetComponentsInChildren<Gun_Button_Controller>()) {
			button.GetComponent<Button> ().interactable = true;
			if (weapon.GetComponent<Fire> ().unavailableScopes.Contains (button.buttonText.text)) {
				button.GetComponent<Button> ().interactable = false;
			}
		}
		primaryScopesParent.gameObject.SetActive (true);
		ShowWeapon ();
		
	}
	public void BackClicked(){
		if (primaryButton.gameObject.activeInHierarchy) {
			if (SceneManager.sceneCount > 1) {
				SceneManager.UnloadSceneAsync ("Loadout Scene");
			} else {
				SceneManager.LoadScene ("Start Scene");
			}
		}
		HideAllButtons ();

		primaryButton.SetActive (true);
		secondaryButton.SetActive (true);
		ShowWeapon ();


	}
	public void ShowAttachmentButtons(){
		HideAllButtons ();
		scopesButton.SetActive (true);
		otherButton.SetActive (true);
	}
	public void HideAllButtons(){
		primaryButton.SetActive (false);
		secondaryButton.SetActive (false);
		scopesButton.SetActive (false);
		otherButton.SetActive (false);

		primaryParent.gameObject.SetActive (false);
		secondaryParent.gameObject.SetActive (false);
		primaryScopesParent.gameObject.SetActive (false);
		secondaryScopesParent.gameObject.SetActive (false);
	}
	public void InstantiateButtons(GameObject[] objectList, Gun_Button_Controller.ButtonTypes type, Transform parent){
		previousPosition = initialPosition;
		for (int i = 0; i < objectList.Length; i++) {
			
			GameObject button = (GameObject)Instantiate (buttonPrefab, parent);
			button.GetComponent<RectTransform>().anchoredPosition = previousPosition;
			previousPosition = previousPosition+Vector2.down*30f;
			button.GetComponentInChildren<Text> ().text = objectList [i].name;
			button.GetComponent<Gun_Button_Controller> ().buttonType = type;
			if (type == Gun_Button_Controller.ButtonTypes.Primary || type == Gun_Button_Controller.ButtonTypes.Secondary) {
				if (objectList [i].GetComponent<Fire> ().skillLevel > skillLevel) {
					button.GetComponent<Button> ().interactable = (false);
				}
			}
			else if (type == Gun_Button_Controller.ButtonTypes.Scope) {
				
			}
		}
	}
	public void SaveData(){
		for(int i = 0; i<loadoutData.Length; i++) {
			string line = loadoutData [i];
			originalData [i] = line;
		}
		System.IO.File.WriteAllLines (Application.streamingAssetsPath+"/Loadout Settings.txt", Util.ThiccWatermelon(loadoutData));
		ShowWeapon ();
	}
}

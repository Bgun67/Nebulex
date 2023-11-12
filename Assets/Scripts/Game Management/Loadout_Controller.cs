using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Loadout_Controller : MonoBehaviour {
	

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
			skillLevel = int.Parse(Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath+"/Player Data.txt"))[1]);
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
		loadoutData = Util.LushWatermelon(System.IO.File.ReadAllLines ( Application.persistentDataPath+"/Loadout Settings.txt"));
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
        //	
        if (weapon != null)
        {
            Destroy(weapon);
            weapon = null;
        }
        GameObject weaponPrefab = null;
        GameObject scopePrefab = null;
        try
        {
            if (selectedGun == 0)
            {

                weaponPrefab = (GameObject)Resources.Load("Weapons/" + loadoutData[0]);
                scopePrefab = (GameObject)Resources.Load("Weapons/Scopes/" + loadoutData[2]);


            }
            else
            {
                weaponPrefab = (GameObject)Resources.Load("Weapons/" + loadoutData[1]);
                scopePrefab = (GameObject)Resources.Load("Weapons/Scopes/" + loadoutData[3]);
            }
        }
        catch (System.Exception e)
        {
            print("Sorry. Braden.exe has encountered an error" + e);
            loadoutData = Profile.RestoreLoadoutFile();
        }

        //add the primary weapon to the finger
        weapon = (GameObject)Instantiate(weaponPrefab, finger);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        Instantiate(scopePrefab, weapon.GetComponent<Fire>().scopePosition);

        DisplayGunInfo();
		
	}
	void DisplayGunInfo(){
		Fire tmpFire = weapon.GetComponent<Fire> ();
		weaponDataText.text =
			("<b><size=16>" + weapon.name.Replace("(Clone)", "") + "</size></b>\n" +
		"Fire Rate: " + tmpFire.fireRate + "\n" +
		"Reload Time: " + tmpFire.reloadTime + "\n" +
		"Damage: " + tmpFire.damagePower + "\n" +
		"Bullet Velocity: " + tmpFire.bulletVelocity + "\n" +
		"Fire Type: " + tmpFire.fireType.ToString() + "\n" +
		"Mag Size: " + tmpFire.magSize + "\n");
		if (skillLevel < tmpFire.skillLevel)
		{
			weaponDataText.text+="<color=red>SKill Level: " +tmpFire.skillLevel+"</color>\n";
		}
		else
		{
					weaponDataText.text+="SKill Level: " +tmpFire.skillLevel+"\n";

		}
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
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Loadout Settings.txt", Util.ThiccWatermelon(loadoutData));
		ShowWeapon ();
	}
}

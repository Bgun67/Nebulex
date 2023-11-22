using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;
using System.Diagnostics.Tracing;
using UnityEngine.EventSystems;
using System;

public class Loadout_Controller : MonoBehaviour {
	public enum WeaponType{
		Primary,
		Secondary,
		Tertiary,
	}
	public LoadoutProperties loadoutProperties;

	public Button confirmButton;
	public Button backButton;

	public GameObject quickSelectScreen;
	public Button primaryButton;
	public Button secondaryButton;
	public Button tertiaryButton;

	public GameObject weaponScreen;
	public Transform weaponListParent;
	public Transform attachmentsListParent;
	public GameObject weaponButtonPrefab;

	public WeaponType selectedWeaponType = 0;
	public string[] loadoutData;
	public int skillLevel;
	public string[] originalData;
	public TextMeshProUGUI weaponDataText;
	WeaponProperties selectedWeapon;

	// Use this for initialization
	void Start () {
		LoadData ();
		try{
			skillLevel = int.Parse(Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath+"/Player Data.txt"))[1]);
		}
		catch{ Debug.Log("Encountered error in"+this.name);
			skillLevel = int.Parse(Profile.RestoreDataFile ()[1]);
		}

		primaryButton.onClick.AddListener(()=>{PrimaryButtonClicked();});
		secondaryButton.onClick.AddListener(()=>{SecondaryButtonClicked();});
		tertiaryButton.onClick.AddListener(()=>{TertiaryButtonClicked();});

		backButton.onClick.AddListener(()=>{BackClicked();});

		WeaponProperties primary = loadoutProperties.PrimaryWeapons.Find((a)=>{return a.name==loadoutData[0];});
		WeaponProperties secondary = loadoutProperties.SecondaryWeapons.Find((a)=>{return a.name==loadoutData[1];});
		//WeaponProperties tertiary = loadoutProperties.TertiaryWeapons.Find((a)=>{return a.name==loadoutData[4];});
		if (primary.PreviewImage)
		{
			primaryButton.transform.Find("Image").GetComponent<Image>().sprite = primary.PreviewImage;
		}
		primaryButton.GetComponentInChildren<TextMeshProUGUI>().text = primary.FriendlyName!=""?primary.FriendlyName:primary.name;
		if (secondary.PreviewImage)
		{
			secondaryButton.transform.Find("Image").GetComponent<Image>().sprite = secondary.PreviewImage;
		}
		secondaryButton.GetComponentInChildren<TextMeshProUGUI>().text = secondary.FriendlyName!=""?secondary.FriendlyName:secondary.name;
		/*if (tertiary.PreviewImage)
		{
			tertiaryButton.transform.Find("Image").GetComponent<Image>().sprite = tertiary.PreviewImage;
		}
		tertiaryButton.GetComponentInChildren<TextMeshProUGUI>().text = tertiary.FriendlyName!=""?tertiary.FriendlyName:tertiary.name;
		*/
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

	void PrimaryButtonClicked(){
		quickSelectScreen.SetActive(false);
		weaponScreen.SetActive(true);
		selectedWeaponType = WeaponType.Primary;
		PopulateWeapons(loadoutProperties.PrimaryWeapons);
	}

	void SecondaryButtonClicked(){
		quickSelectScreen.SetActive(false);
		weaponScreen.SetActive(true);
		selectedWeaponType = WeaponType.Secondary;
		PopulateWeapons(loadoutProperties.SecondaryWeapons);

	}

	void TertiaryButtonClicked(){
		quickSelectScreen.SetActive(false);
		weaponScreen.SetActive(true);
		selectedWeaponType = WeaponType.Tertiary;
		PopulateWeapons(loadoutProperties.TertiaryWeapons);
	}


	void DisplayGunInfo(WeaponProperties weaponProperties){
		if(!weaponProperties){
			return;
		}
		weaponDataText.text =
		"<b><size=16>" + weaponProperties.name.Replace("(Clone)", "") + "</size></b>\n" +
		"Fire Rate: " + weaponProperties.FireRate + "\n" +
		"Reload Time: " + weaponProperties.ReloadTime + "\n" +
		"Damage: " + weaponProperties.DamagePower + "\n" +
		"Bullet Velocity: " + weaponProperties.BulletVelocity + "\n" +
		"Fire Type: " + weaponProperties.FireType.ToString() + "\n" +
		"Mag Size: " + weaponProperties.MagSize + "\n";
		if (skillLevel < weaponProperties.SkillLevel)
		{
			weaponDataText.text += "<color=red>Skill Level: " + weaponProperties.SkillLevel + "</color>\n";
		}
		else
		{
			weaponDataText.text += "Skill Level: " + weaponProperties.SkillLevel + "\n";

		}



	}
	public void MouseOver(WeaponProperties weapon){
		
		
		DisplayGunInfo (weapon);

	}
	public void MouseLeave(){
		
		DisplayGunInfo (selectedWeapon);
	}
	// Update is called once per frame
	public void SelectWeapon (WeaponProperties weapon, WeaponType type) {
		switch (type){
			case WeaponType.Primary:
				loadoutData [0] = weapon.name;
				if (weapon.PreviewImage)
				{
					primaryButton.transform.Find("Image").GetComponent<Image>().sprite = weapon.PreviewImage;
				}
				primaryButton.GetComponentInChildren<TextMeshProUGUI>().text = weapon.FriendlyName!=""?weapon.FriendlyName:weapon.name;
				break;
			case WeaponType.Secondary:
				loadoutData [1] = weapon.name;
				if (weapon.PreviewImage)
				{
					secondaryButton.transform.Find("Image").GetComponent<Image>().sprite = weapon.PreviewImage;
				}
				secondaryButton.GetComponentInChildren<TextMeshProUGUI>().text = weapon.FriendlyName!=""?weapon.FriendlyName:weapon.name;
				break;
			case WeaponType.Tertiary:
				loadoutData [4] = weapon.name;
				if (weapon.PreviewImage)
				{
					tertiaryButton.transform.Find("Image").GetComponent<Image>().sprite = weapon.PreviewImage;
				}
				tertiaryButton.GetComponentInChildren<TextMeshProUGUI>().text = weapon.FriendlyName!=""?weapon.FriendlyName:weapon.name;
				break;
		}
		selectedWeapon = weapon;
		SaveData ();
		DisplayGunInfo (weapon);
	}

	private void SelectScope(string scope, WeaponType type)
    {
        switch (type){
			case WeaponType.Primary:
				loadoutData [2] = scope;
				break;
			case WeaponType.Secondary:
				loadoutData [3] = scope;
				break;
		}

		SaveData ();
		DisplayGunInfo (selectedWeapon);
    }
	
	public void ConfirmClicked(){
		this.gameObject.SetActive(false);
	}

	public void BackClicked(){
		quickSelectScreen.SetActive(true);
		weaponScreen.SetActive(false);
	}
	

	public void PopulateWeapons(List<WeaponProperties> weaponList){
		
		for(int i = 0; i<weaponListParent.childCount; i++){
			Destroy(weaponListParent.GetChild(i).gameObject);
		}
		foreach (WeaponProperties weapon in weaponList) {
			
			GameObject buttonGO = (GameObject)Instantiate (weaponButtonPrefab, weaponListParent);
			Button button = buttonGO.GetComponent<Button>();
			Gun_Button_Controller trigger = buttonGO.GetComponent<Gun_Button_Controller>();
			button.GetComponentInChildren<TextMeshProUGUI>().text = weapon.FriendlyName!=""?weapon.FriendlyName:weapon.name;
			

			button.onClick.AddListener(() =>
			{
				List<string> availableScopes = loadoutProperties.Scopes;
				foreach (string unavailableScope in weapon.UnavailableScopes)
				{
					availableScopes.Remove(unavailableScope);
				}
				SelectWeapon(weapon, selectedWeaponType);
				PopulateAttachments(availableScopes);
			});
			if (weapon.PreviewImage)
			{
				trigger.image.sprite = weapon.PreviewImage;
			}
			trigger.onPointerEnter += () => { MouseOver(weapon); };
			trigger.onPointerExit += () => { MouseLeave(); };

		}
	}

	public void PopulateAttachments(List<string> scopesList){
		attachmentsListParent.gameObject.SetActive(true);
		for(int i = 0; i<attachmentsListParent.childCount; i++){
			Destroy(attachmentsListParent.GetChild(i).gameObject);
		}
		foreach (string scope in scopesList) {
			print(scope);
			GameObject buttonGO = (GameObject)Instantiate (weaponButtonPrefab, attachmentsListParent);
			Button button = buttonGO.GetComponent<Button>();
			button.GetComponentInChildren<TextMeshProUGUI>().text = scope;
			button.onClick.AddListener(()=>{SelectScope(scope, selectedWeaponType);});
			Gun_Button_Controller trigger = buttonGO.GetComponent<Gun_Button_Controller>();
			
			trigger.image.enabled = false;
			//TODO Add Full mouse over capability
			//trigger.onPointerEnter += () => { MouseOver(weapon); };
			//trigger.onPointerExit += () => { MouseLeave(); };

		}
	}

    

    public void SaveData(){
		for(int i = 0; i<loadoutData.Length; i++) {
			string line = loadoutData [i];
			originalData [i] = line;
		}
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Loadout Settings.txt", Util.ThiccWatermelon(loadoutData));
	}
}

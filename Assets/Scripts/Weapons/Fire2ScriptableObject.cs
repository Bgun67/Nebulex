#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR||UNITY_EDITOR_64
[ExecuteInEditMode]
public class Fire2ScriptableObject : ScriptableWizard {
	public Fire[] weapons;

	[MenuItem("GameObject/Fire2ScriptableObject")]
	// Use this for initialization
	static void CreateWizard(){
		ScriptableWizard.DisplayWizard<Fire2ScriptableObject> ("Create Gun", "Create");
    }
    void Awake(){
        weapons = Resources.FindObjectsOfTypeAll<Fire>();
	
	}
	

	void OnWizardCreate () {

        foreach (Fire weapon in weapons)
        {
            WeaponProperties properties = (WeaponProperties)WeaponProperties.CreateInstance("WeaponProperties");
            PropertyInfo[] weaponProperties = typeof(WeaponProperties).GetProperties();
            foreach (PropertyInfo pI in weaponProperties)
            {
                string cameledName = pI.Name[0].ToString().ToLower() + pI.Name.Substring(1);
                Debug.Log(cameledName);
                try{
                     SerializedObject serializedObject = new UnityEditor.SerializedObject(weapon);

                    SerializedProperty serializedPropertyMyInt = serializedObject.FindProperty(cameledName);

                    Debug.Log(serializedPropertyMyInt.objectReferenceValue);
                    pI.SetValue(properties, serializedPropertyMyInt.objectReferenceValue);
                }
                catch{
                    Debug.Log("Could not Find"+cameledName);
                }
                //This returns the value cast (or boxed) to object so you will need to cast it to some other type to find its value.
            }
            AssetDatabase.CreateAsset(properties, "Assets/Scripts/Weapons/Weapon Properties/"+weapon.name+".asset");
        }

        AssetDatabase.SaveAssets();

    }
	

}
#endif

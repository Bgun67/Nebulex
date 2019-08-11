using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR || UNITY_EDITOR_64
using UnityEditor;

[ExecuteInEditMode]
public class FixMetViews : ScriptableWizard
{
    public static List<MetworkViewConflict> conflicts;
	[MenuItem("GameObject/FixMetViews")]
	// Use this for initialization
	static void CreateWizard(){
		ScriptableWizard.DisplayWizard<Gun_Creator> ("Fix All Selected Conflicts", "Other");
		conflicts = FindObjectOfType<MetID_Manager>().conflictingViews;
	}
	void OnWizardCreate()
	{

	}
}
#endif
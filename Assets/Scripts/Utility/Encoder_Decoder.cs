using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class Encoder_Decoder : ScriptableWizard {
	public string[] inputText;
	public bool decode;
	public string[] outputText;
	[MenuItem("Assets/Encoder_Decoder")]
	// Use this for initialization
	static void CreateWizard(){
		#if UNITY_EDITOR
		ScriptableWizard.DisplayWizard<Encoder_Decoder> ("Create Gun", "Exit", "Translate");
		#endif


	}
	void Awake(){
		


	}
	void OnWizardCreate(){

	}
	void OnWizardOtherButton () {
		if (decode) {
			outputText = Util.LushWatermelon (inputText);

		} else {
			outputText = Util.ThiccWatermelon (inputText);
		}
			
	}
}
#endif
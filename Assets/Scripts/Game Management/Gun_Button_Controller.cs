using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun_Button_Controller : MonoBehaviour {
	public enum ButtonTypes
	{
		Primary,
		Secondary,
		Scope,
		Other
	}
	public Text buttonText;
	public ButtonTypes buttonType;
	public Loadout_Controller loadoutScript;
	// Use this for initialization
	void Start () {
		loadoutScript = FindObjectOfType<Loadout_Controller> ();
	}
	
	// Update is called once per frame
	public void OnClick(){
		if (buttonType == ButtonTypes.Primary) {
			loadoutScript.PrimaryButtonClicked (buttonText.text);
		}
		else if(buttonType == ButtonTypes.Secondary) {
			loadoutScript.SecondaryButtonClicked (buttonText.text);
		}
		else if(buttonType == ButtonTypes.Scope) {
			loadoutScript.ScopesButtonClicked (buttonText.text);
		}
	}
	public void OnMouseEnter(){
		loadoutScript.MouseOver (buttonText.text, buttonType);

	}
	public void OnMouseExit(){
		loadoutScript.MouseLeave ();

	}
}

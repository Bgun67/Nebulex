using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_Controller : MonoBehaviour {

	public Button button;
	public Text text;
	public Color originalTextColour;
	public Color selectedTextColour;
	public Button_Controller[] associates;

	bool isClicked = false;

	public Sprite normalSprite;
	public Sprite higlightedSprite;


	public void OnSelect(){
		text.color = selectedTextColour;
	}
	public void OnDeselect(){
		if (!isClicked) {
			text.color = originalTextColour;
		}
	}
	public void OnClick(){
		button.image.sprite = higlightedSprite;
		text.color = selectedTextColour;
		isClicked = true;

		foreach (Button_Controller _button in associates) {
			_button.button.image.sprite = normalSprite;
			_button.text.color = originalTextColour;
			_button.isClicked = false;
		}
	}
}

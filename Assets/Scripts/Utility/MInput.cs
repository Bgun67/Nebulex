using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows;

public class MInput : MonoBehaviour {

	public enum InputLock{
		LockTranslation,
		LockRotation,
		LockAll,
		None
	}

	static float previousMouseX = 0;
	static float previousMouseY = 0;

	static float previousDeltaX = 0;
	static float previousDeltaY = 0;

	public static InputLock inputLock;
	public static bool useMouse = true;
	public static float sensitivity = 1f;

	public static string[,] buttonMappings = new string[,]{
		//Sort name(script name) || Controller Button
		{"Fire1", "Fire1"},
		{"Sprint", "Fire1"},
		{"Jump", "Jump"}
	};

	public static float GetAxis(string _axisName){
		if (inputLock == InputLock.LockAll)
		{
			return 0f;
		}
		switch (_axisName) {
		case "Move Z":
			if (inputLock == InputLock.LockTranslation) {
				return 0f;
			}
			return Input.GetAxis (_axisName);
			break;

		case "Rotate Y":
		case "Rotate X":
			if (inputLock == InputLock.LockRotation) {
				return 0f;
			}
			if (useMouse) {
				float _smoothRot = 0f;
				if (_axisName == "Rotate Y") {
					_smoothRot = Mathf.Lerp (previousMouseX, (Input.mousePosition.x / (float)Screen.width)-0.5f, 0.5f);
					previousMouseX = _smoothRot;

				} else if (_axisName == "Rotate X") {
					_smoothRot = Mathf.Lerp (previousMouseY, -((Input.mousePosition.y / (float)Screen.height)-0.5f), 0.5f); 
					previousMouseY = _smoothRot;

				}

				return _smoothRot * 5f * _smoothRot * Mathf.Sign (_smoothRot)*sensitivity;
			} else {
				return Input.GetAxis (_axisName)*sensitivity;
			}
			break;
		

		
		default:
			Debug.LogError ("MInput: " + _axisName + " does not exist!");
			return 0;
		}

	}
	public static float GetMouseDelta(string _axisName)
	{
		if (inputLock == InputLock.LockAll)
		{
			return 0f;
		}
		if (inputLock == InputLock.LockRotation)
		{
			return 0f;
		}
		float _smoothRot = 0f;
		if (_axisName == "Mouse X")
		{

			float _delta = Input.GetAxis("Mouse X");
			_smoothRot = Mathf.Lerp(previousDeltaX,_delta,0.4f);
			previousDeltaX = _delta;

		}
		else if (_axisName == "Mouse Y")
		{
			float _delta = Input.GetAxis("Mouse Y");
			_smoothRot = Mathf.Lerp(previousDeltaY,_delta,0.6f);
			previousDeltaY = _delta;

		}

		return _smoothRot * 5f * _smoothRot * Mathf.Sign(_smoothRot)*sensitivity;

		Debug.LogError("MInput: " + _axisName + " does not exist!");
	}

	public static bool GetButton(string _axisName){
		switch (_axisName) {

		case "Left Trigger":
			if (useMouse) {
				return (Input.GetButton(_axisName) || Input.GetMouseButton(1));	
			} else {
				return Input.GetButton (_axisName);
			}
			break;
		


		default:
			Debug.LogError ("MInput: " + _axisName + " does not exist!");
			return Input.GetButton(_axisName);
		}

	}

	public static bool GetButtonDown(string _axisName){
		switch (_axisName) {

		case "Switch Weapons":
			if (useMouse) {
				return (Input.GetButtonDown (_axisName) || Input.mouseScrollDelta.magnitude > 0.5f);	
			} else {
				return Input.GetButtonDown (_axisName);
			}
			break;
		case "Fire2":
			if (useMouse) {
				return (Input.GetButtonDown (_axisName));	
			} else {
				return Input.GetButtonDown (_axisName);
			}
			break;


		default:
			Debug.LogError ("MInput: " + _axisName + " does not exist!");
			return Input.GetButtonDown(_axisName);
		}

	}
	static string GetMappedButton(string _buttonName)
	{
		for (int i = 0; i < buttonMappings.GetLength(0); i++)
		{
			if (buttonMappings[i, 0] == _buttonName)
			{
				return buttonMappings[i, 1];
			}
		}
		Debug.LogWarning("Button or Axis "+_buttonName+"could not be found");
		return _buttonName;
	}
	static void SetMappedButton(string _scriptButtonName, string _controllerName)
	{
		for (int i = 0; i < buttonMappings.GetLength(0); i++)
		{
			if (buttonMappings[i, 0] == _scriptButtonName)
			{
				buttonMappings[i, 1] = _controllerName;
				return;
			}
		}
		Debug.LogError("Could not Find either " + _scriptButtonName+" or " + _controllerName);
	}
	public static IEnumerator SwitchMappedButton(string _buttonToSwap)
	{
		bool _buttonPressed = false;
		print("SwitchMap");
		while (!_buttonPressed)
		{
			//find pushedbutton from list of possible names
			for (int i = 0; i < buttonMappings.GetLength(0); i++)
			{
				if (Input.GetButton(buttonMappings[i, 0]))
				{
					//find the index of the button we are swapping out
					for (int k = 0; k < buttonMappings.GetLength(0); k++)
					{
						if (buttonMappings[k,0] == _buttonToSwap)
						{
							//reassign button with pushed button
							buttonMappings[k, 1] = buttonMappings[i,0];
						}
					}
					_buttonPressed = true;
				}
			}
			yield return new WaitForSeconds(0.01f);
		}
		FindObjectOfType<Controls_Manager>().ShowMappings();
	}



}

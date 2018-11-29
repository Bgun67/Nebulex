using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows;

public class MInput : MonoBehaviour {

	public enum InputLock{
		LockTranslation,
		LockRotation,
		LockAll
	}

	static float previousMouseX = 0;
	static float previousMouseY = 0;

	public static InputLock inputLock;
	public static bool useMouse = true;

	public static float GetAxis(string _axisName){
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
					_smoothRot = Mathf.Lerp (previousMouseY, (Input.mousePosition.x / (float)Screen.width)-0.5f, 0.5f);
					previousMouseY = _smoothRot;

				} else if (_axisName == "Rotate X") {
					_smoothRot = Mathf.Lerp (previousMouseX, -((Input.mousePosition.y / (float)Screen.height)-0.5f), 0.5f); 
					previousMouseX = _smoothRot;

				}

				return _smoothRot * 5f * _smoothRot * Mathf.Sign (_smoothRot);
			} else {
				return Input.GetAxis (_axisName);
			}
			break;
		

		
		default:
			Debug.LogError ("MInput: " + _axisName + " does not exist!");
			return 0;
		}

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
			return false;
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
			return false;
		}

	}


}

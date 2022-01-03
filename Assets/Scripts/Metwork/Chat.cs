using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class Chat : NetworkBehaviour {


	public Text txtChatLog;
	public InputField txtField;

	public Game_Controller gameController;

	bool fieldActive;
	public bool showLog;

	void Start(){
		gameController = Game_Controller.Instance;
	}

	void FixedUpdate(){
		//Runs before update so I can get that juice
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (!fieldActive) {
				fieldActive = true;
				EventSystem.current.SetSelectedGameObject (txtField.gameObject, null);
				txtField.OnPointerClick (new PointerEventData (EventSystem.current));
				showLog = true;
				
			} else {
				fieldActive = false;
				showLog = false;
				Input.ResetInputAxes();
				OnSubmit();

				
			}
		}
		if(fieldActive){
			Input.ResetInputAxes();
		}
	}
	
	public void OnSubmit(){
		if (txtField.text.Length < 1) {
			return;
		}

		
		Cmd_SendMessage (gameController.localPlayer.name + ": " + txtField.text + "\n", gameController.localTeam);
		
		txtField.text = "";
		EventSystem.current.SetSelectedGameObject (null, null);
	}
	[Server]
	public static void LogToChat(string _message){
		FindObjectOfType<Chat>().Rpc_SendMessage (_message, 2);
	}
	[Command]
	void Cmd_SendMessage(string _message, int team){
		Rpc_SendMessage(_message, team);
	}

	///<summary>
	///Team 2 is everyone
	///Team 0 and Team 1 are only those teams
	///<summary>
	[ClientRpc]
	void Rpc_SendMessage(string message, int team){
		switch(team){
			case 2:
			txtChatLog.text += "[All] " + message;
			break;
			case 0:
			txtChatLog.text += "<color=#BBFFBB>[Green] " + message + "</color>";
			break;
			case 1:
			txtChatLog.text += "<color=#FFBBBB>[Red] " + message + "</color>";
			break;
			case 3:
			txtChatLog.text += message;
			break;
			default:
			break;

		}

	}

	
}

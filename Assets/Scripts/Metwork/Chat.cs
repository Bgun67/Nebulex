using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Chat : MonoBehaviour {


	public MetworkView netView;

	public Text txtChatLog;
	public InputField txtField;

	public Game_Controller gameController;

	bool fieldActive;
	public bool showLog;

	void Start(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
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
	/*void Update(){
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (!fieldActive) {
				fieldActive = true;
				EventSystem.current.SetSelectedGameObject (txtField.gameObject, null);
				txtField.OnPointerClick (new PointerEventData (EventSystem.current));
				showLog = true;
				
			} else {
				fieldActive = false;
				showLog = false;
				OnSubmit();
				
			}
		}
		if(fieldActive){
			Input.ResetInputAxes();
		}
	}*/


	public void OnSubmit(){
		if (txtField.text.Length < 1) {
			return;
		}

		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_SendMessage", MRPCMode.AllBuffered, new object[]{ gameController.localPlayer.GetComponent<Player_Controller>().playerName + ": " + txtField.text + "\n" , gameController.GetLocalTeam()});
		} else {
			RPC_SendMessage (gameController.localPlayer.name + ": " + txtField.text + "\n", gameController.GetLocalTeam());
		}
		txtField.text = "";
		EventSystem.current.SetSelectedGameObject (null, null);
		//txtChatLog.OnPointerClick (new PointerEventData (EventSystem.current));
	}
	public static void LogToChat(string _message){
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			FindObjectOfType<Chat>().netView.RPC ("RPC_SendMessage", MRPCMode.AllBuffered, new object[]{ _message, 2});
		} else {
			FindObjectOfType<Chat>().RPC_SendMessage (_message, 2);
		}
	}

	///<summary>
	///Team 2 is everyone
	///Team 0 and Team 1 are only those teams
	///<summary>
	[MRPC]
	public void RPC_SendMessage(string message, int team){
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

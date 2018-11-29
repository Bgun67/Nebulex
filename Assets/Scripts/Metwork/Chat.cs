using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Chat : MonoBehaviour {

	public MetworkView netView;

	public InputField txtChatLog;
	public InputField txtField;

	public Game_Controller gameController;

	bool fieldActive;
	public bool showLog;

	void Start(){
		gameController = GameObject.FindObjectOfType<Game_Controller> ();
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (!fieldActive) {
				fieldActive = true;
				EventSystem.current.SetSelectedGameObject (txtField.gameObject, null);
				txtField.OnPointerClick (new PointerEventData (EventSystem.current));
				showLog = true;
				StartCoroutine(ShowLog ());
			} else {
				fieldActive = false;
				showLog = false;
				StartCoroutine(HideLog ());
			}
		}
	}

	IEnumerator ShowLog(){
		Color _chatColor = txtChatLog.image.color;
		Color _textColor = txtChatLog.textComponent.color;
		while (txtChatLog.image.color.a < 1f && showLog) {
			_chatColor.a += 0.1f;
			txtChatLog.image.color = _chatColor;
			_textColor.a += 0.1f;
			txtChatLog.textComponent.color = _textColor;
			yield return new WaitForSeconds (0.001f);
		}
		while(EventSystem.current.currentSelectedGameObject == txtField.gameObject){
			yield return new WaitForSeconds (0.5f);
		}

		StartCoroutine (HideLog ());
	}
	IEnumerator HideLog(){
		showLog = false;
		yield return new WaitForSeconds (3f);
		Color _chatColor = txtChatLog.image.color;
		Color _textColor = txtChatLog.textComponent.color;
		while (txtChatLog.image.color.a > 0f && !showLog) {
			_chatColor.a -= 0.1f;
			txtChatLog.image.color = _chatColor;
			_textColor.a -= 0.1f;
			txtChatLog.textComponent.color = _textColor;
			yield return new WaitForSeconds (0.001f);
		}
	}

	public void OnSubmit(){
		if (txtField.text.Length < 1) {
			return;
		}

		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_SendMessage", MRPCMode.AllBuffered, new object[]{ gameController.localPlayer.GetComponent<Player_Controller>().playerName + ": " + txtField.text + "\n" });
		} else {
			RPC_SendMessage (gameController.localPlayer.name + ": " + txtField.text + "\n");
		}
		txtField.text = "";
		EventSystem.current.SetSelectedGameObject (null, null);
		//txtChatLog.OnPointerClick (new PointerEventData (EventSystem.current));
	}

	[MRPC]
	public void RPC_SendMessage(string message){
		
		txtChatLog.text += message;
		
		//if(txtChatLog.text.Length > 20000){
		//	txtChatLog.text.Substring(10);
		//}

		showLog = true;
		StartCoroutine (ShowLog ());
		StartCoroutine (HideLog ());

	}

	
}

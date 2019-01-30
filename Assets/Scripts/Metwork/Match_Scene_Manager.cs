using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;

public class Match_Scene_Manager : MonoBehaviour {
	//public Network netView;
	public InputField gameNameInput;
	public GameObject loadingPanel;
	/*public class HostData
	{
		public int connectedPlayers;
		public string gameName;
		public string gameType;
		public string guid;

	}*/

	public bool testing;
	public string portString;
	public string localIP;
	public float pollingTime = 10f;
	public Button[] hostButtons;
	MHostData[] hostData;
	public PHPMasterServerConnect connection;
	public bool isError = false;

	// Use this for initialization
	void Awake () {
		foreach(Button hostButton in hostButtons){
			hostButton.gameObject.SetActive (false);
		}
		if(testing){
			localIP = File.ReadAllLines(Application.streamingAssetsPath + "/Player Data.txt")[3];
		}
		connection = this.GetComponent<PHPMasterServerConnect> ();
		StartCoroutine (GetMatches ());

	}
		

	public IEnumerator GetMatches(){
		while (this.enabled) {

			connection.QueryPHPMasterServer ();
			print ("Querying");
			
			yield return new WaitForSecondsRealtime (pollingTime);
		}

	}

	public void DisplayMatches(){
		hostData = GetComponent<PHPMasterServerConnect> ().PollHostList ();
		foreach(Button hostButton in hostButtons){
			hostButton.gameObject.SetActive (false);
		}
		if (hostData != null && hostData.Length > 0) {
			for (int i = 0; i<hostData.Length; i++) {
				hostButtons [i].GetComponentInChildren<Text>().text = hostData [i].gameName +" " + hostData[i].gameType + " " + hostData[i].connectedPlayers + "/" + hostData[i].playerLimit;
				hostButtons [i].gameObject.SetActive (true);
			}

		} else {
			print("No games currently available");
			for (int i = 0; i<hostButtons.Length; i++) {
				hostButtons [i].gameObject.SetActive (false);
			}
		}
	}

	public void ChangeMatchType(string _matchType){
		System.IO.File.WriteAllLines (Application.streamingAssetsPath + "/Match Settings.txt",Util.ThiccWatermelon(new string[] {
			"1200",
			_matchType
		}));
		connection.gameType = _matchType;

		//Hide the buttons from the old match type
		for (int i = 0; i < hostButtons.Length; i++) {
			hostButtons [i].gameObject.SetActive (false);
		}
		connection.QueryPHPMasterServer ();


	}

	public void JoinServer(int index){
		
		if (testing) {
			//Network.Connect (localIP, 12345);//hostData [index].port);
		} else {
			//Network.Connect (hostData [index].guid);
			Metwork.Connect (hostData [index].gameName);

		}

		Metwork.onConnectedToServer += OnConnectedToMetServer;
		Metwork.onPlayerConnected += OnMetPlayerConnected;
		System.IO.File.WriteAllLines (Application.streamingAssetsPath + "/Match Settings.txt",Util.ThiccWatermelon(new string[] {
			"1200",
			hostData[index].gameType
		}));
		connection.gameType = hostData [index].gameType;
		connection.gameName = hostData [index].gameName;
		//unsecure, but should not fail
		try { loadingPanel.SetActive(true);

		}catch { }

		Invoke("DeactivateLoadPanel", 8f);



	}
	void DeactivateLoadPanel()
	{
		//unsecure, but should not fail
		loadingPanel.SetActive(false);
	}
	public void OnMetPlayerConnected(MetworkPlayer _player){
		//if (Metwork.player == null || _player.connectionID == Metwork.player.connectionID && (SceneManager.GetActiveScene().name == "MatchScene")) {
		//	SceneManager.LoadScene ("LobbyScene");
		//	Destroy (this);
		//}
	}

	public void OnConnectedToMetServer(){
		if (SceneManager.GetActiveScene ().name == "MatchScene") {
			//SceneManager.LoadScene ("LobbyScene");
		}

		Destroy (this);
	}

	public void QuitToMainMenu(){
		Destroy (this.gameObject);
		SceneManager.LoadScene ("Start Scene");
	}

	public void OnValueChanged(){

		if (gameNameInput.text.Contains ("\n")) {
			gameNameInput.text = gameNameInput.text.Substring (0, gameNameInput.text.Length - 1);
			StartServer ();
		}

	}

	public void StartServer(){
		if (isError) {
			return;
		}

		if (gameNameInput.text != "")
		{
			connection.gameName = gameNameInput.text;
			if (testing)
			{
				//Network.InitializeServer (32, int.Parse (portString), false);
			}
			else
			{
				//Network.InitializeServer (32, int.Parse (portString),
				//	!Network.HavePublicAddress ());

				Metwork.InitializeServer(gameNameInput.text);
			}
			Metwork.onServerInitialized += OnMetServerInitialized;

			//This line is only necessary for local builds
			//MasterServer.RegisterHost (connection.gameType, gameNameInput.text, "");
			//unsecure, but should not fail
			try { loadingPanel.SetActive(true); } catch { }
			Invoke("DeactivateLoadPanel", 5f);
			//Destroy the script, it no longer belongs
			print("Starting Server");
			//Destroy (this);
		}
		else
		{
			print("Please input a game name");
		}
	}

	public void OnMetServerInitialized(){
		SceneManager.LoadScene ("LobbyScene");
		StartCoroutine (GameObject.FindObjectOfType<PHPMasterServerConnect> ().OnServerInitialized());
		Destroy (this);
	}

	public void OnQueryMasterServerFailed(){
		print ("Master Server Query Failed");
		//isError = true;
	}


}

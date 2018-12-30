using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Network_Manager : MonoBehaviour {

	[Tooltip("The minimum players required to leave the lobby and begin the match, make sure to changein game controller")]
	public int minStartingPlayers = 8;
	public int maxPlayers;

	//public int netID;
	public float sendRate;

	public int connections = 1;
	public int playerNumber = 1;

	bool isMigratingHost = false;

	public MetworkView netView;
	public PHPMasterServerConnect conn;

	public string firstAlternateHost;


	public enum SceneMode
	{
		MainMenu,
		Lobby,
		Game
	}
	public SceneMode sceneMode = SceneMode.MainMenu;

	// Use this for initialization
	void Start () {
		netView = this.GetComponent<MetworkView> ();
		conn = GetComponent<PHPMasterServerConnect> ();
		Metwork.onPlayerConnected += this.OnMetPlayerConnected;
		Metwork.onPlayerDisconnected += this.OnMetPlayerDisconnected;
		SceneManager.sceneLoaded += OnSceneLoaded;

	}
	void Update(){
		if (Input.GetKeyDown (".")) {
			PrematureStart();
		}
	}
	public void PrematureStart()
	{
		minStartingPlayers = 1;
		//We have sufficient players, move to the game. Checking if we are connected is unnecessary as we
		//must be connected anyway
		print("We Starting");
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_LoadScene", MRPCMode.AllBuffered, new object[] { "TransistionScene" });
		}
		else
		{
			RPC_LoadScene("TransistionScene");
		}
		sceneMode = SceneMode.Game;
		print("Invoked RPC");
	}

	void OnSceneLoaded(Scene _scene, LoadSceneMode _mode){
		if (_scene.name == "LobbyScene") {
			sceneMode = SceneMode.Lobby;
			print ("Is Server: " + Metwork.isServer);
			print ("# of players: " + Metwork.players.Count);
			//The server is not counted in connections so a +1 is required to include them
			if (Metwork.players.Count >= minStartingPlayers && Metwork.isServer) {
				//We have sufficient players, move to the game. Checking if we are connected is unnecessary as we
				//must be connected anyway
				print ("We Starting");
				netView.RPC ("RPC_LoadScene", MRPCMode.AllBuffered, new object[]{ "TransistionScene" });
				sceneMode = SceneMode.Game;
				print ("Invoked RPC");
			}


		} else if (_scene.name == "Space") {
			sceneMode = SceneMode.Game;
			//call to server to sync the scores on all clients
			if (Metwork.peerType != MetworkPeerType.Disconnected)
			{
				netView.RPC("RPC_UpdateMatchInfo", MRPCMode.Server, new object[] { });
			}
		}
	}

	void OnMetPlayerConnected(MetworkPlayer player){

		if (Metwork.isServer) {

			//TODO: Update
			//netView.RPC ("SetPlayerNumber", player, new object[]{Metwork.players.IndexOf(player) + 2});
			//netView.RPC ("AddPlayer", RPCMode.AllBuffered, new object[]{ connections + 1 });
			//netView.RPC ("MakeHostAlternate", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});
			//netView.RPC ("SetHostAlternates", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});

			//The server is not counted in connections so a +1 is required to include them
			//This has changed
			if (sceneMode == SceneMode.Lobby && Metwork.players.Count >= minStartingPlayers) {
				//We have sufficient players, move to the game. Checking if we are connected is unnecessary as we
				//must be connected anyway
				print ("We Starting");
				netView.RPC ("RPC_LoadScene", MRPCMode.AllBuffered, new object[]{ "TransistionScene" });
				sceneMode = SceneMode.Game;

			} else if(sceneMode == SceneMode.Lobby ){
				//Load the player into our lobby
				netView.RPC ("RPC_LoadScene", player, new object[]{ "LobbyScene" });
				print ("Loading Player into Lobby Scene");
			}else {
				//Load the player into our game
				netView.RPC ("RPC_LoadScene", player, new object[]{ "TransistionScene" });
				print ("Loading Player into Transistion Scene");
			}

			foreach (Metwork_Object _netObj in GameObject.FindObjectsOfType<Metwork_Object>()) {
				_netObj.netView.RPC ("RPC_SetLocation", player, new object[]{_netObj.transform.position, _netObj.transform.rotation, _netObj.rb.velocity});
			}
		}



	}
	//Deprecated
	//void OnPlayerConnected(NetworkPlayer player){
		/*
		if (Network.isServer) {
			//byte bite;
			//int connectionID = NetworkTransport.Connect(0, player.externalIP,player.externalPort,0,out bite);
			//WWWForm form = new WWWForm();


			netView.RPC ("SetPlayerNumber", player, new object[]{(new List<NetworkPlayer>(Network.connections)).IndexOf(player) + 2});
			netView.RPC ("AddPlayer", RPCMode.AllBuffered, new object[]{ connections + 1 });
			//netView.RPC ("MakeHostAlternate", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});
			netView.RPC ("SetHostAlternates", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});

			//The server is not counted in connections so a +1 is required to include them
			if (sceneMode == SceneMode.Lobby && Network.connections.Length + 1>= minStartingPlayers) {
				//We have sufficient players, move to the game. Checking if we are connected is unnecessary as we
				//must be connected anyway
				print("We Starting");
				netView.RPC("RPC_LoadScene", RPCMode.AllBuffered, new object[]{"TransistionScene"});
				sceneMode = SceneMode.Game;

			}

			foreach (Network_Object _netObj in GameObject.FindObjectsOfType<Network_Object>()) {
				_netObj.netView.RPC ("RPC_SetLocation", player, new object[]{_netObj.transform.position, _netObj.transform.rotation, _netObj.rb.velocity});
			}
		}
		*/


	//}

	[MRPC]
	void RPC_LoadScene(string _sceneName){
		print("Loading scene " + _sceneName);
		if(SceneManager.GetActiveScene().name == "Space" && _sceneName == "TransistionScene"){
			print("Cancelling Load");
			return;
		}
		SceneManager.LoadScene (_sceneName);

	}
	void OnMetPlayerDisconnected(MetworkPlayer player){
		if (Metwork.isServer) {
			netView.RPC ("AddPlayer", MRPCMode.AllBuffered, new object[]{ connections - 1 });
			//TODO: Update
			//if (player.guid == firstAlternateHost) {
			//	netView.RPC ("SetHostAlternates", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});
			//}
		}

	}
	//Deprecated
	//void OnPlayerDisconnected(NetworkPlayer player){
		/*if (Network.isServer) {
			netView.RPC ("AddPlayer", RPCMode.AllBuffered, new object[]{ connections - 1 });
			if (player.guid == firstAlternateHost) {
				netView.RPC ("SetHostAlternates", RPCMode.AllBuffered, new object[]{Network.connections[0].guid});
			}
		}*/

	//}


	[MRPC]
	void SetHostAlternates(string _firstAlternate){
		firstAlternateHost = _firstAlternate;
	}

	[MRPC]
	void AddPlayer(int players){
		connections = players;
	}
	
	[MRPC] 
	void SetPlayerNumber(int number){
		if (isMigratingHost) {
			isMigratingHost = false;
			return;
		}
		playerNumber = number;

	}

	//TODO: Upgrade
	[MRPC]
	void BreakHost(string _firstAlternate, string playerID){
		/*print ("Breaking Host. Migrating to host: " + _firstAlternate);
		if (Network.isServer) {
			return;
		}
		//Open the new server
		if (_firstAlternate == Network.player.guid && playerID == Network.player.ToString()) {

			Network.InitializeServer (32, Network.player.port, !Network.HavePublicAddress ());
			

			MasterServer.RegisterHost (conn.gameType, conn.name, "Demo");

			//Change all the server stuff to this as owner
			//I really hope this doesn't screw everything up
			foreach (Network_Object obj in GameObject.FindObjectsOfType<Network_Object>()) {
				if (obj.tag == "Player" && obj.owner == 1 ) {
					obj.owner = this.playerNumber;
					continue;
				}
				if (obj.tag == "Player" && obj.owner == this.playerNumber ) {
					obj.owner = 1;

				}
			}
			this.playerNumber = 1;
		}
		//Connect to the new host
		else {
			StartCoroutine(LateConnect (_firstAlternate));
		}*/
	}

	//TODO: Upgrade for new metworking
	IEnumerator LateConnect(string _firstAlternate){
		//Leave time for the host to set up
		yield return new WaitForSecondsRealtime(0.5f);
		//Network.Connect (_firstAlternate);
		//Ignore the next set player number command
		isMigratingHost = true;

	}

	void OnServerInitialized(){
		/*NetworkTransport.Init ();
		ConnectionConfig config = new ConnectionConfig ();
		int reliableChannel = config.AddChannel (QosType.Reliable);

		HostTopology topology = new HostTopology (config, 32);

		int hostID = NetworkTransport.AddHost (topology, Network.player.port, Network.player.ipAddress);
		*/
	}





	//public void StartServer(){
	//	Network.InitializeServer (maxPlayers, 25001);
	//}
	//public void JoinServer(string IP){
		//Connect to the Network
	//	Network.Connect(IP, 25001);
	//}
		
	//On windows store apps this will NOT run
	void OnApplicationQuit(){
		try{
			//TODO: Upgrade
			//netView.RPC("BreakHost", MRPCMode.OthersBuffered, new object[]{Network.connections [0].guid, Network.connections[0].ToString()});
		}
		catch{
		}
		print ("Breaking the host");
		conn.UnregisterHost ();
		//Network.Disconnect ();
		Metwork.Disconnect();
		print ("Completed break");

	}







}

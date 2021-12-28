/*MetworkV2
 * Designed as a replacement for the Unity Raknet Metworking System (Legacy)
 * Capable of operating on PC, Android and WebGL, as well as surviving upgrade to 2018.2 (I think)
 * Michael Gunther
 * 2018-07-19
*/

using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Byn.Net;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Byn.Common;
using System.IO;

[AttributeUsage(AttributeTargets.Method)]
public class MRPC :Attribute{}

public enum MRPCMode{
	All,
	AllBuffered,
	Others,
	OthersBuffered,
	Server,
	Player
}

public enum MetworkPeerType
{
	Disconnected,
	Connected
}
public enum DisconnectReason{
	Unexpected,
	ClientQuit,
	ServerQuit
}
public class MetworkPlayer{
	
	int pConnectionID = -1;
	bool pIsServer = false;
	int pNetConnectionID = -1;

	/// <summary>
	/// Initializes a new instance of the <see cref="MetworkPlayer"/> class.
	/// </summary>
	/// <param name="_connID">Conn ID, zero is the server</param>
	/// <param name="_isServer">If set to <c>true</c> is server.</param>
	public MetworkPlayer(int _connID, bool _isServer){
		pConnectionID = _connID;
		pIsServer = _isServer;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="MetworkPlayer"/> class.
	/// </summary>
	/// <param name="_connID">Conn ID, zero is the server</param>
	/// <param name="_isServer">If set to <c>true</c> is server.</param>
	/// <param name="_netConnectionID">The connectionID used by WebRTC.</param>
	public MetworkPlayer(int _connID, bool _isServer, int _netConnectionID){
		pConnectionID = _connID;
		pIsServer = _isServer;
		pNetConnectionID = _netConnectionID;
	}

	/// <summary>
	/// Gets the connection ID (zero is the server)
	/// </summary>
	/// <value>The connection ID</value>
	public int connectionID{
		get{
			return pConnectionID;
		}
	}
	public bool isServer{
		get{
			return pIsServer;
		}
	}
	/// <summary>
	/// Don't use this unless interacting with WebRTC
	/// </summary>
	/// <value>The net connection I.</value>
	public int netConnectionID{
		get{
			return pNetConnectionID;
		}
	}
}


public class Metwork : MonoBehaviour {
	public static Metwork _instance;
	public static bool isTransferringServer = false;

	//Our fancy matching server (On OpenShift remember?) //wss://because-why-not.com:12777/chatapp
	public static string uSignalingUrl = "wss://nebulex-server.herokuapp.com/chatapp";//"ws://sample-bean.herokuapp.com";//"ws://nebulex-nebulex.193b.starter-ca-central-1.openshiftapps.com/chatapp";//wss://because-why-not.com:12777/chatapp";

	public static string uIceServer = "stun:stun.l.google.com:19302";
	public static string uIceServerUser = "testuser13";
	public static string uIceServerPassword = "6f8d82ca4f19";

	/// <summary>
	/// Mozilla stun server. Used to get through the firewall and establish direct connections.
	/// Replace this with your own production server as well. 
	/// </summary>
	public static string uIceServer2 = "stun:stun.l.google.com:19302";

	/// <summary>
	/// Set true to use send the WebRTC log + wrapper log output to the unity log.
	/// </summary>
	public static bool uLog = false;

	/// <summary>
	/// Debug console to be able to see the unity log on every platform
	/// </summary>
	public static bool uDebugConsole = false;

	/// <summary>
	/// The Metwork interface.
	/// This can be native webrtc or the browser webrtc version.
	/// (Can also be the old or new unity Metwork but this isn't part of this package)
	/// </summary>
	private static IBasicNetwork mMetwork = null;

	private static bool pIsServer = false;
	/// <summary>
	/// Returns true if this device is the server
	/// </summary>
	public static bool isServer{
		get{
			return pIsServer;
		}
	}

	/// <summary>
	/// Keeps track of all current connections
	/// </summary>
	private static List<ConnectionId> mConnections = new List<ConnectionId>();
	/// <summary>
	/// Keeps track of Metwork players. Guaranteed to be the same on all clients
	/// </summary>
	public static List<MetworkPlayer> players = new List<MetworkPlayer>();
	/// <summary>
	/// The local Metwork player
	/// </summary>
	public static MetworkPlayer player;


	private const int MAX_CODE_LENGTH = 256;

	private static MetworkPeerType pPeerType = MetworkPeerType.Disconnected;
	/// <summary>
	/// The connection status of the Metwork (read-only).
	/// </summary>
	public static MetworkPeerType peerType{
		get{
			return pPeerType;
		}
	}

	public static DisconnectReason disconnectReason = DisconnectReason.ClientQuit;
	public static string roomName;
	public static int replaceServerPriority = 0;

	public delegate void OnMetPlayerConnected(MetworkPlayer _player);
	/// <summary>
	/// Delegate invoked on all connections when a player joins
	/// </summary>
	public static OnMetPlayerConnected onPlayerConnected;
	public delegate void OnConnectedToMetServer();
	/// <summary>
	/// Delegate invoked on the joining player
	/// </summary>
	public static OnConnectedToMetServer onConnectedToServer;

	public delegate void OnMetPlayerDisconnected(MetworkPlayer _player);
	/// <summary>
	/// Delegate invoked on all connections when a player disconnects
	/// </summary>
	public static OnMetPlayerDisconnected onPlayerDisconnected;
	public delegate void OnMetServerInitialized();
	/// <summary>
	/// Delegate invoked on server when the server is initialized
	/// </summary>
	public static OnMetServerInitialized onServerInitialized;

	public static Dictionary<int,MetworkView> metViews = new Dictionary<int,MetworkView>();

	public class BufferedMessage{
		public int viewID;
		public MRPCMode mode;
		public int source;
		public int destination;
		public byte[] msg;

		public BufferedMessage (int _viewID, MRPCMode _mode, int _destination,int _source, byte[] _msg){
			this.viewID = _viewID;
			this.mode = _mode;
			this.destination = _destination;
			this.source = _source;
			this.msg = _msg;
		}
	}

	private List<BufferedMessage> reliableBuffer = new List<BufferedMessage>(10000);
	private static Queue<byte[]> reliableQueue = new Queue<byte[]>(10000);

	/// <summary>
	/// Will setup webrtc and create the Metwork object
	/// </summary>
	private void Start ()
	{
		
		//shows the console on all platforms. for debugging only
		if(uDebugConsole)
			DebugHelper.ActivateConsole();
		if(uLog)
			SLog.SetLogger(OnLog);

		SLog.LV("Verbose log is active!");
		SLog.LD("Debug mode is active");

		Append("Setting up WebRtcMetworkFactory");
		WebRtcNetworkFactory factory = WebRtcNetworkFactory.Instance;
		if(factory != null){
			Append("WebRtcMetworkFactory created");
		}
		else{Debug.Log("Failed to set up WebRtcNetworkFactory");
		}


		Metwork._instance = GameObject.FindObjectOfType<Metwork> ();
		//Populate the metViews with MetworkViews from this scene
		SceneManager.sceneLoaded += SceneLoaded;
		SceneLoaded (SceneManager.GetActiveScene(), LoadSceneMode.Additive);



	}

	void SceneLoaded(Scene _scene, LoadSceneMode _mode){
		MetworkView[] _metViewsArr = GameObject.FindObjectsOfType<MetworkView> ();

		for (int i = 0; i < _metViewsArr.Length; i++) {
			if (!Metwork.metViews.ContainsKey (_metViewsArr [i].viewID)) {
				Metwork.metViews.Add (_metViewsArr [i].viewID, _metViewsArr [i]);
			} else {
				Metwork.metViews [_metViewsArr [i].viewID] = _metViewsArr [i];
			}
		}
			
		
		Debug.Log ("Loaded views: " + metViews.Count);

		//We are going to check the
		//buffer of reliable channel calls to make sure we haven't missed a call to a metworkView in the last scene
		CheckBuffer();

	}

	void CheckBuffer(){
		for (int i = 0; i < reliableBuffer.Count; i++) {
			if (metViews[reliableBuffer [i].viewID] != null) {
				HandleIncommingMessage (reliableBuffer [i]);
				reliableBuffer.RemoveAt (i);
				i--;
			}
		}
	}



	

	/// <summary>
	/// Allocates the number for the metwork view.Returns the number. Returns -1 if none could be found.
	/// </summary>
	/// <param name="_playerNumber">Player number. (Between 1-32)</param>
	public static int AllocateMetworkView(int _playerNumber){
		for (int i = _playerNumber * 1000; i < _playerNumber * 1000 + 1000f; i++) {
			if (!metViews.ContainsKey (i)) {
				metViews.Add (i, null);
				return i;
			}
		}
		return -1;
	}


	


	
		
	/// <summary>
	/// Invoked when a new player is added to the game
	/// </summary>
	/// <param name="_connID">Conn I.</param>
	/// <param name="_isServer">If set to <c>true</c> is server.</param>
	[MRPC]
	public void RPC_AddMetworkPlayer(int _connID, bool _isServer, bool _isYou){
		Debug.Log ("Adding Metwork Player");
		MetworkPlayer _player = new MetworkPlayer (_connID, _isServer);



		if (players.FindIndex(x=> x.connectionID == _player.connectionID) == -1) {
			players.Add (_player);
			if (onPlayerConnected != null) {
				onPlayerConnected.Invoke (_player);
			}
		}
		if (player == null && _isYou) {
			//This is the newly connected player
			player = _player;
			Metwork.replaceServerPriority = _connID;
			Debug.Log("Setting player to connectionID: " + _connID);
			if (onConnectedToServer != null) {
				onConnectedToServer.Invoke ();
			}
		}

	}
	/// <summary>
	/// Invoked when a new player is removed from the game
	/// </summary>
	/// <param name="_connID">Conn I.</param>
	[MRPC]
	public void RPC_RemoveMetworkPlayer(int _connID){
		MetworkPlayer _player = players.Find (x => x.connectionID == _connID);
		if (onPlayerDisconnected != null) {
			onPlayerDisconnected.Invoke (_player);
		}
		players.Remove(_player);
	}


	private static void OnLog(object msg, string[] tags)
	{
		StringBuilder builder = new StringBuilder();
		TimeSpan time = DateTime.Now - DateTime.Today;
		builder.Append(time);
		builder.Append("[");
		for (int i = 0; i< tags.Length; i++)
		{
			if(i != 0)
				builder.Append(",");
			builder.Append(tags[i]);
		}
		builder.Append("]");
		builder.Append(msg);
		Debug.Log(builder.ToString());
	}

	private static void Setup()
	{
		Append("Initializing webrtc Metwork");


		mMetwork = WebRtcNetworkFactory.Instance.CreateDefault(uSignalingUrl, new IceServer[] { new IceServer(uIceServer, uIceServerUser, uIceServerPassword), new IceServer(uIceServer2) });
		if (mMetwork != null)
		{
			Append("WebRTCMetwork created");
		}
		else
		{
			Append("Failed to access webrtc ");
		}
		SetGuiState(false);
	}

	private void Reset(bool _keepInstance = false)
	{
		Debug.Log("Cleanup!");



		pIsServer = false;
		if(!isTransferringServer){
			mConnections = new List<ConnectionId>();
			Cleanup(_keepInstance);
		}
		else{
			Cleanup(true);
		}
		
		SetGuiState(true);

		if (metViews.Count < 1) {
			metViews.Add (0,null);
		}

	}

	

	/// <summary>
	/// called during reset and destroy. keepInstance prevents this from destroying the Metwork gameobject
	/// </summary>
	private static void Cleanup(bool _keepInstance = false)
	{
		if (player != null && !isTransferringServer) {
			player = null;
		}
		if (mMetwork != null) {
			Metwork.pIsServer = false;
			Metwork.pPeerType = MetworkPeerType.Disconnected;

			if(!_keepInstance){
				Destroy(Metwork._instance.gameObject);
			}
			if(!isTransferringServer){
				Metwork.reliableQueue.Clear();
				Metwork.players.Clear();
			}
			mMetwork.Dispose ();
			mMetwork = null;
			
		}

		
	}

	///<summary>
	///Destroys the gameobject
	///</summary>
	private void OnDestroy()
	{
		if (mMetwork != null)
		{
			Cleanup();
		}

		Metwork._instance = null;
		//Metwork.disconnectReason = DisconnectReason.ServerQuit;

		Metwork.mConnections.Clear();
		Metwork.metViews.Clear();
		Metwork.mMetwork = null;
		Metwork.onConnectedToServer = null;
		Metwork.onPlayerConnected = null;
		Metwork.onPlayerDisconnected = null;
		Metwork.onServerInitialized = null;
		Metwork.pIsServer = false;
		Metwork.player = null;
		Metwork.players.Clear();
		Metwork.pPeerType = MetworkPeerType.Disconnected;
		Metwork.reliableQueue.Clear();
		
		Metwork.replaceServerPriority = 0;
		Metwork.roomName = "";
		Metwork.disconnectReason = DisconnectReason.ClientQuit;
		
	}

	private void FixedUpdate()
	{
		//The connection was broken unexpectedly
		if(Metwork.peerType == MetworkPeerType.Disconnected && Metwork.disconnectReason == DisconnectReason.Unexpected){
			if(Time.frameCount % 300 == 0){
				StartCoroutine(RecoverConnection(Metwork.isServer,roomName));
			}

		}
		//check each fixed update if we have got new events
		HandleMetwork();

		
	}




	private static void HandleMetwork()
	{
		//check if the Metwork was created
		if (mMetwork != null)
		{
			//first update it to read the data from the underlaying Metwork system
			mMetwork.Update();

			//handle all new events that happened since the last update
			NetworkEvent evt;
			//check for new messages and keep checking if mMetwork is available. it might get destroyed
			//due to an event
			while (mMetwork != null && mMetwork.Dequeue(out evt))
			{
				//print to the console for debugging
				//Debug.Log(evt);

				//check every message
				switch (evt.Type)
				{
				case NetEventType.ServerInitialized:
					{
						//server initialized message received
						//this is the reaction to StartServer -> switch GUI mode
						pIsServer = true;
						string address = evt.Info;
						Debug.Log("Server started. Address: " + address);
						pPeerType = MetworkPeerType.Connected;
						//Add the current MetworkPlayer to the list
						if(!isTransferringServer){
							players.Clear();
						
							player = new MetworkPlayer(1, true, int.Parse(evt.ConnectionId.ToString()));
							players.Add (player);
						}
						if (onServerInitialized != null) {
							onServerInitialized.Invoke ();
						}
					} break;
				case NetEventType.ServerInitFailed:
					{
						pPeerType = MetworkPeerType.Disconnected;
						//user tried to start the server but it failed
						//maybe the user is offline or signaling server down?
						pIsServer = false;
						Debug.Log ("Failed To Initialize Server");
						Append("Server start failed.");
						GameObject.FindObjectOfType<Metwork>().Reset(true);
					} break;
				case NetEventType.ServerClosed:
					{
						pPeerType = MetworkPeerType.Disconnected;
						Debug.Log("Server Closed");
						//TODO: Add something about disconnect reason				
						
						//server shut down. reaction to "Shutdown" call or
						//StopServer or the connection broke down
						pIsServer = false;
						Append("Server closed. No incoming connections possible until restart.");
					} break;
				case NetEventType.NewConnection:
					{
						pPeerType = MetworkPeerType.Connected;
						mConnections.Add(evt.ConnectionId);
						//either user runs a client and connected to a server or the
						//user runs the server and a new client connected
						Append("New local connection! ID: " + evt.ConnectionId);

						//if server -> send announcement to everyone and use the local id as username
						//TODO: Figure out if the player is actually new or returning
						//TODO: Set the server off isTransferringServer
						Debug.Log("Adding New Connection");
						if(pIsServer && !(isTransferringServer && players.Count - 1 > mConnections.Count))
						{
							Debug.Log("Not transferring connection");
							int _playerID = -1;
							//Select the lowest unused MetworkPlayer ID
							for (int k = 1; k < 1000; k++) {//k=2
								if (!players.Exists (x => x.connectionID == k)) {
									_playerID = k;
									break;
								}
							}
							players.Add (new MetworkPlayer (_playerID, false, int.Parse(evt.ConnectionId.ToString())));


							int _playersCount = players.Count;
							//Here I want to inform the other players of the new recruit
							//and I want to inform the new recruit of his new name
							Debug.Log("Adding new player");
							for (int k = 0; k < _playersCount; k++) {
								if (players[k].connectionID == _playerID) {
									GameObject.FindObjectOfType<Metwork> ().GetComponent<MetworkView> ().RPC ("RPC_AddMetworkPlayer", MRPCMode.AllBuffered, new object[] {
										players [k].connectionID,
										players [k].isServer,
										true
									});
								}
								else{
									GameObject.FindObjectOfType<Metwork> ().GetComponent<MetworkView> ().RPC ("RPC_AddMetworkPlayer", MRPCMode.AllBuffered, new object[] {
										players [k].connectionID,
										players [k].isServer,
										false
									});
								}
							}
							if (onPlayerConnected != null) {
								onPlayerConnected.Invoke (players [players.Count - 1]);
							}

                            while(reliableQueue.Count > 1)
                            {
								//TODO: Fix this
                                byte[] _msg = reliableQueue.Dequeue();

								byte[] _tmp = BitGetBytes(_playerID);
								_msg[0] = _tmp[0];
								_msg[1] = _tmp[1];
								_msg[2] = _tmp[2];
								_msg[3] = _tmp[3];

								 _tmp = BitGetBytes((int)MRPCMode.Player);
								_msg[12] = _tmp[0];
								_msg[13] = _tmp[1];
								_msg[14] = _tmp[2];
								_msg[15] = _tmp[3];                              
                                                                
                                SendData(_msg, MRPCMode.Player);
                            }

							
							//user runs a server. announce to everyone the new connection
							//using the server side connection id as identification
							string msg = "New user " + evt.ConnectionId + " joined the room.";
							Append(msg);
							

							//SendString(msg);
						}
					} break;
				case NetEventType.ConnectionFailed:
					{
						if(Metwork.peerType == MetworkPeerType.Connected){
							Debug.Log("Connection Failed");
							Debug.Log(evt.ConnectionId);
							Debug.Log(evt.MessageData);
							break;
						}

						//Uncomment this if you want to deactivate the loading panel this way
						/*Match_Scene_Manager _manager = this.GetComponent<Match_Scene_Manager>();
						if(_manager != null){
							_manager.loadingPanel.SetActive(false);
						}*/

						pPeerType = MetworkPeerType.Disconnected;
						//Outgoing connection failed. Inform the user.
						Append("Connection failed");
						Metwork._instance.Reset(true);
					} break;
				case NetEventType.Disconnected:
					{
						

						mConnections.Remove(evt.ConnectionId);
						//A connection was disconnected
						//If this was the client then he was disconnected from the server
						//if it was the server this just means that one of the clients left
						Append("Local Connection ID " + evt.ConnectionId + " disconnected");
						if (isServer == false)
						{
							Metwork._instance.Reset();
							pPeerType = MetworkPeerType.Disconnected;
						}
						else
						{
							string userLeftMsg = "User " + evt.ConnectionId + " left the room.";

							//show the server the message
							Append(userLeftMsg);


								
							MetworkPlayer tmpPlayer = players.Find (x => x.netConnectionID == int.Parse(evt.ConnectionId.ToString()));

							//Here I want to inform the other players of the lost player
							GameObject.FindObjectOfType<Metwork>().GetComponent<MetworkView> ().RPC ("RPC_RemoveMetworkPlayer", MRPCMode.AllBuffered, new object[] {
								tmpPlayer.connectionID,
							});
									
								
							SendString(userLeftMsg);

						}
					} break;
				case NetEventType.ReliableMessageReceived:
					HandleIncommingMessage (ref evt);
					break;
				case NetEventType.UnreliableMessageReceived:
					{
						HandleIncommingMessage(ref evt);
					} 
					break;
				}
			}

			//finish this update by flushing the messages out if the Metwork wasn't destroyed during update
			if(mMetwork != null)
				mMetwork.Flush();
		}
	}

	private static void HandleIncommingMessage(ref NetworkEvent evt)
	{
		MessageDataBuffer mdb = (MessageDataBuffer)evt.MessageData;

		

		//string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);
		byte[] buffer = mdb.Buffer;

		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		//Find the correct MetworkView to apply the RPC
		//TODO: this may not exist, see other todo, try printing it out to see if the metview ID matches
		//Otherwise check the player metView IDs
		int _viewID = TryToInt32(buffer.Sub(8,4),0);//_segments[2]);

		//Unpack the MRPCMode
		MRPCMode _mode = (MRPCMode) TryToInt32(buffer.Sub(12,4),0);
		//Unpack the destination number
		int _destination = TryToInt32(buffer.Sub(0,4),0);
		if(Time.time % 10 == 0){
			Debug.Log("Destination: " + _destination);
			Debug.Log("Player ConnectionID: " + player.connectionID);
		}
		//Unpack the source number
		int _source = TryToInt32(buffer.Sub(4,4),0);

		if (!Metwork.metViews.ContainsKey (_viewID)) {
			Metwork._instance.reliableBuffer.Add (new BufferedMessage (_viewID, _mode, _destination, _source,  buffer));
		}

		//Debug.Log ("Handling incoming message: " + msg);
		//Debug.Log (_viewID + " " + _mode + " " + _destination + " " + _source);

		//if server -> forward the message to everyone else including the sender
		if (pIsServer)
		{
			//we use the server side connection id to identify the client
			//string idAndMessage = msg; // evt.ConnectionId + ":" + 
			//SendString(idAndMessage);
			//Append(idAndMessage);

			switch (_mode) {
			case MRPCMode.All:
					//Forward the message and recieve it
					//If we sent the message, don't forward it
					//if (_source != 0) {
				SendData (buffer, _mode);
					//}
					
					//Invoke the RPC
				if (Metwork.metViews.ContainsKey (_viewID)) {
					metViews [_viewID].RecieveRPC (buffer);
				}
										
					break;
			case MRPCMode.AllBuffered:
					//Forward the message and recieve it
					//If we sent the message, don't forward it
					
				SendData (buffer, _mode);

                    //Store the message for any new players
                    reliableQueue.Enqueue(buffer);

					//Invoke the RPC
				if (Metwork.metViews.ContainsKey (_viewID)) {
					metViews [_viewID].RecieveRPC (buffer);
				}

					break;
				case MRPCMode.Others:
					//Forward the data
					//If we sent the message, don't forward it
					//if (_source != 0) {
						SendData (buffer, _mode);
					//}
					//If the origin was this player, do NOT recieve
					if (_destination != 0) {
					if (Metwork.metViews.ContainsKey (_viewID)) {
						metViews [_viewID].RecieveRPC (buffer);
					}
						

					}
					break;
				case MRPCMode.OthersBuffered:
					//Forward the data
					//If we sent the message, don't forward it
					//if (_source != 0) {
						SendData (buffer, _mode);

                    //Store the message for any new players
                    reliableQueue.Enqueue(buffer);
                    //}
                    //If the origin was this player, do NOT recieve
                    if (_destination != 0) {
						
						//Invoke the RPC
					if (Metwork.metViews.ContainsKey (_viewID)) {
						metViews [_viewID].RecieveRPC (buffer);
					}
																	

					}
					break;
				case MRPCMode.Player:
					//Forward the data
					//If we sent the message, don't forward it
					//if (_source != 0) {
						SendData (buffer, _mode);
					//}
					//If the destination was this player, recieve the msg
					if (_destination == 0) {
						
						//Invoke the RPC
					if (Metwork.metViews.ContainsKey (_viewID)) {
						metViews [_viewID].RecieveRPC (buffer);
					}


					}
					break;
			case MRPCMode.Server:
					//Recieve the message (we are the server)
					
					//Invoke the RPC
				if (Metwork.metViews.ContainsKey (_viewID)) {
					metViews [_viewID].RecieveRPC (buffer);
				}
							
					break;
				default:
					Debug.LogError ("MRPCMode does not exist!");
					break;
			}
		}
		else
		{
			switch (_mode) {
			case MRPCMode.All:
				//Recieve the msg
				try {
					//Invoke the RPC
					metViews [_viewID].RecieveRPC (buffer);
				} catch {
					Debug.LogError ("Unable to invoke msg: ");// + msg);
				}
				break;
			case MRPCMode.AllBuffered:
				//Recieve the msg
				//Invoke the RPC
				//metViews [_viewID].RecieveRPC (_segments);
				//try {
					//Invoke the RPC
					metViews [_viewID].RecieveRPC (buffer);
				//} catch {
				//	Debug.Log ("Metviews Count: " + metViews.Count);
				//	Debug.Log ("ViewID: " + _viewID);
				//	Debug.LogError ("Unable to invoke msg: " + msg);
				//}
				break;
			case MRPCMode.Others:
				//If the origin was this player, do NOT recieve
				if (_destination != player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (buffer);
					} catch {
						//Debug.LogError ("Unable to invoke msg: " + msg);
					}

				}
				break;
			case MRPCMode.OthersBuffered:
				//If the origin was this player, do NOT recieve
				if (_destination != player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (buffer);
					} catch {
						Debug.LogError ("Unable to invoke msg: ");// + msg);
					}

				}
				break;
			case MRPCMode.Player:
				//If the destination was this player, recieve the msg
				if (_destination == player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (buffer);
					} catch {
					//	Debug.LogError ("Unable to invoke msg: ");// + msg);
					}

				}
				break;
			case MRPCMode.Server:
				//This should not run
				//Ignore
				break;
			default:
				Debug.LogError ("MRPCMode does not exist!");
				break;
			}
			

			//client received a message from the server -> simply print
			//Append(msg);
		}

		//return the buffer so the Metwork can reuse it
		mdb.Dispose();
	}

	private static void HandleIncommingMessage(BufferedMessage bMsg)
	{
		//Find the correct MetworkView to apply the RPC
		int _viewID = bMsg.viewID;
		//Unpack the MRPCMode
		MRPCMode _mode = bMsg.mode;
		//Unpack the destination number
		int _destination = bMsg.destination;
		//Unpack the source number
		int _source = bMsg.source;

		if (!Metwork.metViews.ContainsKey (_viewID)) {
			Metwork._instance.reliableBuffer.Add (new BufferedMessage (_viewID, _mode, _destination, _source, bMsg.msg));
		}


		//Debug.Log ("Handling incoming message: " + msg);
		//Debug.Log (_viewID + " " + _mode + " " + _destination + " " + _source);

		//if server -> forward the message to everyone else including the sender
		if (pIsServer)
		{
			//we use the server side connection id to identify the client
			//string idAndMessage = msg; // evt.ConnectionId + ":" + 
			//SendString(idAndMessage);
			//Append(idAndMessage);

			switch (_mode) {
			case MRPCMode.All:
				

				//Invoke the RPC
				metViews [_viewID].RecieveRPC (bMsg.msg);

				break;
			case MRPCMode.AllBuffered:
				

				//Invoke the RPC
				metViews [_viewID].RecieveRPC (bMsg.msg);

				break;
			case MRPCMode.Others:
				
				//If the origin was this player, do NOT recieve
				if (_destination != 0) {

					metViews [_viewID].RecieveRPC (bMsg.msg);


				}
				break;
			case MRPCMode.OthersBuffered:
				
				//If the origin was this player, do NOT recieve
				if (_destination != 0) {

					//Invoke the RPC
					metViews [_viewID].RecieveRPC (bMsg.msg);


				}
				break;
			case MRPCMode.Player:
				
				//If the destination was this player, recieve the msg
				if (_destination == 0) {

					//Invoke the RPC
					metViews [_viewID].RecieveRPC (bMsg.msg);


				}
				break;
			case MRPCMode.Server:
				//Recieve the message (we are the server)

				//Invoke the RPC
				metViews [_viewID].RecieveRPC (bMsg.msg);

				break;
			default:
				Debug.LogError ("MRPCMode does not exist!");
				break;
			}
		}
		else
		{
			switch (_mode) {
			case MRPCMode.All:
				//Recieve the msg
				try {
					//Invoke the RPC
					metViews [_viewID].RecieveRPC (bMsg.msg);
				} catch {
					Debug.LogError ("Unable to invoke msg: ");// + msg);
				}
				break;
			case MRPCMode.AllBuffered:
				//Recieve the msg
				//Invoke the RPC
				//metViews [_viewID].RecieveRPC (_segments);
				try {
					//Invoke the RPC
					metViews [_viewID].RecieveRPC (bMsg.msg);
				} catch {
					Debug.Log ("Metviews Count: " + metViews.Count);
					Debug.Log ("ViewID: " + _viewID);
					Debug.LogError ("Unable to invoke msg: ");// + msg);
				}
				break;
			case MRPCMode.Others:
				//If the origin was this player, do NOT recieve
				if (_destination != player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (bMsg.msg);
					} catch {
						//Debug.LogError ("Unable to invoke msg: " + msg);
					}

				}
				break;
			case MRPCMode.OthersBuffered:
				//If the origin was this player, do NOT recieve
				if (_destination != player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (bMsg.msg);
					} catch {
						Debug.LogError ("Unable to invoke msg: ");// + msg);
					}

				}
				break;
			case MRPCMode.Player:
				//If the destination was this player, recieve the msg
				if (_destination == player.connectionID) {
					try {
						//Invoke the RPC
						metViews [_viewID].RecieveRPC (bMsg.msg);
					} catch {
						Debug.LogError ("Unable to invoke msg: ");// + msg);
					}

				}
				break;
			case MRPCMode.Server:
				//This should not run
				//Ignore
				break;
			default:
				Debug.LogError ("MRPCMode does not exist!");
				break;
			}


			//client received a message from the server -> simply print
			//Append(msg);
		}


	}


	/// <summary>
	/// Sends a string as UTF8 byte array to all connections
	/// </summary>
	/// <param name="msg">String containing the message to send</param>
	/// <param name="reliable">false to use unreliable messages / true to use reliable messages</param>
	private static void SendString(string msg, bool reliable = true)
	{
		if (mMetwork == null || mConnections.Count == 0)
		{
			Append("No connection. Can't send message.");
		}
		else
		{
			byte[] msgData = Encoding.UTF8.GetBytes(msg);
			foreach (ConnectionId id in mConnections)
			{
				mMetwork.SendData(id, msgData, 0, msgData.Length, reliable);
			}
		}
	}

	/// <summary>
	/// Sends a string as UTF8 byte array
	/// </summary>
	/// <param name="msg">String containing the data to send</param>
	/// <param name="_mode">How to send the data</param>
	public static void SendData(byte[] msg, MRPCMode _mode)
	{
		if (mMetwork == null || mConnections.Count == 0)
		{
			Append("No connection. Can't send message.");
		}
		else
		{
			byte[] msgData;
			switch (_mode){
				case MRPCMode.All:
				case MRPCMode.Others:
					
					for (int i = 0; i < mConnections.Count; i++) {
						mMetwork.SendData (mConnections[i], msg, 0, msg.Length, false);
					}
					break;
				case MRPCMode.OthersBuffered:
				case MRPCMode.AllBuffered:
				case MRPCMode.Player:
				case MRPCMode.Server:
					
					for (int i = 0; i < mConnections.Count; i++) {
						mMetwork.SendData (mConnections[i], msg, 0, msg.Length, true);
					}
					break;
				default:
					break;
			}
		}
	}



	#region UI

	private void OnGUI()
	{
		GUIStyle _style = new GUIStyle();
		_style.fontSize = 13;
		

		try{
			GUI.Label (new Rect (10, 10, 400, 80), player.connectionID.ToString(),_style);

		}catch{
			GUI.Label (new Rect (10, 10, 400, 80), Metwork.peerType.ToString(),_style);
		}
		//draws the debug console (or the show button in the corner to open it)
		DebugHelper.DrawConsole();
	}

	/// <summary>
	/// Adds a new message to the message view
	/// </summary>
	/// <param name="text"></param>
	private static void Append(string text)
	{
		//Debug.Log("chat: " + text);
	}


	/// <summary>
	/// Changes the gui depending on if the user is connected
	/// or disconnected
	/// </summary>
	/// <param name="showSetup">true = user is connected. false = user isn't connected</param>
	private static void SetGuiState(bool showSetup)
	{
		
	}

	/// <summary>
	/// Attempts to connect to the room named "_room"
	/// </summary>
	public static void Connect(string _room)
	{
		
		Setup();
		mMetwork.Connect(_room);
		print("Connecting to " + _room + " ...");
		roomName = _room;
	}


	/// <summary>
	/// Shuts down the server
	/// </summary>
	public static void Disconnect()
	{
		Debug.Log("Shutting Down Server");
		if(Metwork.isServer){
			FindObjectOfType<PHPMasterServerConnect> ().UnregisterHost ();
			
			
			
		}
		Debug.Log("Resetting Network");
		Metwork._instance.Reset();
		SetGuiState(true);
	}

	public void TransferServer(){
	
		
		MetworkPlayer _newServerPlayer = players.Find(x=> x.connectionID != player.connectionID);
		int _id = -1;
		Debug.Log("Anything");
		if(_newServerPlayer != null){
			Debug.Log("Goes");
			_id = _newServerPlayer.connectionID;
		}
		else{
			//There are no other players in the match
			return;
		}
		
		//Here I want to start the new server
		GameObject.FindObjectOfType<Metwork>().GetComponent<MetworkView> ().RPC ("RPC_TransferServer", MRPCMode.OthersBuffered, new object[] {
			//TODO: Guarantee that this player exists!
			_id,
			roomName
		});
		
	}
	[MRPC]
	public void RPC_TransferServer(int _player, string _roomName){
		if(!isServer){
			isTransferringServer = true;
			Invoke("StopTransferringServer", 11f);
		
			//TODO: Confirm if we are the player that needs to take over the server
			if(_player == player.connectionID){//player.connectionID){
				Invoke("DelayedInitializeServer", 5f);
			}
			else{
				Invoke("DelayedConnect", 5f);
			}
			print ("Connecting to new server");
		}
	}
	public void DelayedInitializeServer(){
		Metwork.InitializeServer(roomName);
	}
	public void DelayedConnect(){
		Metwork.Connect(roomName);
	}
	/*DEPRECATED 
	void StopTransferringServer(){
		isTransferringServer = false;
		if(Metwork.isServer){
			//Reshow the host to potential new players
			StartCoroutine(GameObject.FindObjectOfType<PHPMasterServerConnect>().OnServerInitialized());
			Game_Controller _gameController = GameObject.FindObjectOfType<Game_Controller>();
			if(_gameController != null){
				_gameController.ServerTransferred();
			}
		}
	}*/


	/// <summary>
	/// Starts a server and opens a room named _room
	/// </summary>
	public static void InitializeServer(string _room)
	{

		Setup();
		mMetwork.StartServer(_room);

		Debug.Log("StartServer " + _room);
		roomName = _room;

	}

	public void StartRecoverConnection(bool _isServer, string _roomName){
		StartCoroutine(RecoverConnection(_isServer, _roomName));
	}
	public IEnumerator RecoverConnection(bool _isServer, string _roomName){
		
		yield return new WaitForSeconds(0.1f);
		for(int i = 0; i<5; i++){
			print("Attempting Metwork Recovery. Room Name: " + _roomName);
			if(Metwork.peerType == MetworkPeerType.Disconnected){
				if(_isServer){
					print("Attempting Server Recovery");
					Metwork.InitializeServer(_roomName);
				}
				else{
					print("Attempting Client Recovery");
					Metwork.Connect(_roomName);
				}
			}
			yield return new WaitForSeconds(0.5f);
		}

		//If we are still not connected, assume the role of server
		yield return new WaitForSeconds(0.1f);
		for(int i = 0; i< Metwork.replaceServerPriority; i++){
			print("Attempting New Server Connection");
			if(Metwork.peerType == MetworkPeerType.Disconnected){
				
				Metwork.Connect(_roomName);
				
			}
			yield return new WaitForSeconds(0.5f);
		}
		
		if(Metwork.peerType == MetworkPeerType.Disconnected){
			print("Attempting Server Replacement");
			Metwork.InitializeServer(_roomName);
		}
				
		
	}

	

	
	#endregion

	public static int TryToInt32(byte[] _bytes, int _index){
		if(_bytes.Length != 4){
			Debug.Log("Wrong number of bytes converting Int32");
			return 0;
		}
		if(BitConverter.IsLittleEndian){
			Array.Reverse(_bytes);
		}
		return BitConverter.ToInt32(_bytes,_index);
	}
	public static float TryToSingle(byte[] _bytes, int _index){
		if(_bytes.Length != 4){
			Debug.Log("Wrong number of bytes converting single, got: " + _bytes.Length.ToString() + " expected: " + sizeof(float));
			return 0;
		}
		if(BitConverter.IsLittleEndian){
			Array.Reverse(_bytes);
		}
		return BitConverter.ToSingle(_bytes,_index);
	}
	public static byte[] BitGetBytes(int _value){
		
		byte[] _bytes = BitConverter.GetBytes(_value);
		if(BitConverter.IsLittleEndian){
			Array.Reverse(_bytes);
		}
		return _bytes;
	}
	public static byte[] BitGetBytes(float _value){
		
		byte[] _bytes = BitConverter.GetBytes(_value);
		if(BitConverter.IsLittleEndian){
			Array.Reverse(_bytes);
		}
		return _bytes;
	}
}

public static class ExtensionMethods{
	public static T[] Sub<T>(this T[] _data, int _startIndex, int _length){
		T[] _result = new T[_length];
		Array.Copy(_data, _startIndex, _result, 0, _length);
    	return _result;
	} 
	public static void Append(this MemoryStream stream, byte value)
    {
        stream.Append(new[] { value });
    }

    public static void Append(this MemoryStream stream, byte[] values)
    {
        stream.Write(values, 0, values.Length);
    }
	
}

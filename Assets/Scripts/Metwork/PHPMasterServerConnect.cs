using UnityEngine;
//using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Mirror;

[System.Serializable]
public class MHostData{
	public int connectedPlayers;
	public string gameName;
	public string gameType;
	public string guid;
	public int playerLimit;
	public string ip;
	public bool useNat;
	public int port;
	public bool passwordProtected;
	public string comment;
}

public class PHPMasterServerConnect : MonoBehaviour 
{
	public string masterServerURL = "";
	public string gameType = "";
    [HideInInspector]
	public string gameName = "Nebulex";
    [HideInInspector]
	public string comment = "Demo";

	public float delayBetweenUpdates = 30.0f;
	[SerializeField]
	private MHostData[] hostData = null;

	public int maxRetries = 3;
	private int retries = 0;

	private bool registered = false;
	
	static private PHPMasterServerConnect _instance = null;
	static public PHPMasterServerConnect instance {
		get {
			if (_instance == null) {
				_instance = (PHPMasterServerConnect) FindObjectOfType (typeof(PHPMasterServerConnect));
			}
			return _instance;
		}
	}

	CustomNetworkManager manager;
	
	void Awake () {
        if (_instance != null) {
            DestroyImmediate (gameObject);
        } else {
		    DontDestroyOnLoad (gameObject);
        	_instance = this;
			manager = this.GetComponent<CustomNetworkManager>();
			try{
				masterServerURL = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/network.config");
			}
			catch{};
			//Metwork.onPlayerConnected += _instance.OnMetPlayerConnected;
			//Metwork.onPlayerDisconnected += _instance.OnMetPlayerDisconnected;
			
			
		}

	}

    void OnDestroy () {
        if (registered) {
			// Unregister without the CR
	        string url = masterServerURL+"UnregisterHost";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
	        new WWW (url);
        }
    }
	
	public MHostData[] PollHostList()
	{
		if(hostData == null){
			return new MHostData[]{};
		}
		//Pick out only the appropriate game types
		List<MHostData> _hostList = new List<MHostData>();
		foreach (MHostData _host in hostData){
			if(_host.gameType == gameType && _host.connectedPlayers < _host.playerLimit){
				_hostList.Add(_host);
			}
		}
		return _hostList.ToArray();
	}

	private bool atOnce = false;
	public void QueryPHPMasterServer ()
	{
		if (!atOnce) 
			StartCoroutine (QueryPHPMasterServerCR ());
	}
	
	private IEnumerator QueryPHPMasterServerCR ()
	{
		if (!Match_Scene_Manager.Instance){
			yield return new WaitForSeconds(1f);
			yield break;
		}
		//print ("Querying Harder");
		atOnce = true;
		string url = masterServerURL+"QueryMS?gameType="+WWW.EscapeURL(gameType);
    	//Debug.Log ("looking for URL " + url);
    	WWW www = new WWW (url);
    	yield return www;
		Debug.Log(www.text);

    	retries = 0;
	    while (www.error != null && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);

	        yield return www;
	    }

	    if (www.error != null) {
	        //Match_Scene_Manager.Instance.OnQueryMasterServerFailed();
	    }
		if (!Match_Scene_Manager.Instance){
			yield return new WaitForSeconds(1f);
			yield break;
		}
	
		if (www.text == "") {
			atOnce = false;
			Match_Scene_Manager.Instance.DisplayMatches ();
			yield break;
		} else if (www.text == "empty") {
			hostData = null;
			Match_Scene_Manager.Instance.DisplayMatches ();
		} else {
			//Debug.Log("Received message");
			try{
				string[] hosts = new string[www.text.Split (new char[]{';'}, System.StringSplitOptions.RemoveEmptyEntries).Length];
				hosts = www.text.Split (new char[]{';'}, System.StringSplitOptions.RemoveEmptyEntries);
				hostData = new MHostData[hosts.Length];
				int index = 0;
				foreach (string host in hosts) {
					if (host == "")
						continue;
					string[] data = host.Split ("," [0]);
					hostData [index] = new MHostData ();
					hostData [index].ip = data [0];
					hostData [index].port = int.Parse (data [1]);
					hostData [index].useNat = (data [2] == "1");
					hostData [index].guid = data [3];
					hostData [index].gameType = data [4];
					hostData [index].gameName = data [5];
					hostData [index].connectedPlayers = int.Parse (data [6]);
					hostData [index].playerLimit = int.Parse (data [7]);
					hostData [index].passwordProtected = (data [8] == "1");
					hostData [index].comment = data [9];


					index++;
				}
			}
			catch{
				Debug.Log("Error parsing host data");
			}
			//try {
				Match_Scene_Manager.Instance.DisplayMatches ();
			//} catch {
			//	Debug.Log("Failed to display matches");
			//}
		}

		atOnce = false;
	}

	public void RegisterHost () {
		//gameName = pGameName;
		//comment = pComment;
		registered = false;
		StartCoroutine (RegistrationLoop ());
	}

	private IEnumerator RegistrationLoop()
	{
		while (!registered && manager.isServerMachine){//NetworkServer.active) {
            yield return StartCoroutine (RegisterHostCR());
    		yield return new WaitForSeconds(delayBetweenUpdates);
		}

		//registered = false;
	}

    private IEnumerator RegisterHostCR () {
		
	    string url = masterServerURL+"RegisterHost";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&comment="+WWW.EscapeURL (comment);
		url += "&playerLimit=" + manager.maxConnections;
		url += "&connectedPlayers="+manager.numPlayers;
		url += "&internalIp="+"192.168.2.30";
		url += "&internalPort="+manager.GetComponent<kcp2k.KcpTransport>().Port;
		url += "&externalPort="+manager.GetComponent<kcp2k.KcpTransport>().Port;

		
		//TODO
		url += "&externalIp="+"204.123.32.5";
		
	    Debug.Log ("Attempting to register host: Try: " + retries.ToString() + " at url " + url);
	
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "Succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "Succeeded") && retries >= maxRetries) {
			Debug.LogError (www.error);
	        SendMessage ("OnRegisterHostFailed");
		}
		if (www.error == null && www.text == "Succeeded") {
			Debug.Log("Successfully registered host");
			registered = true;
		}
    }

	public void UnregisterHost ()
	{
		float initialTime = Time.time;
		Debug.Log ("Unregistering host");
		StartCoroutine (UnregisterHostCR ());
	}

	void OnRegisterHostFailed(){
		Debug.Log("Failed to register host");
	}
	
	private IEnumerator UnregisterHostCR ()
	{
		
		if (true) {
			
	        string url = masterServerURL+"UnregisterHost";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
			Debug.Log (url);
	        WWW www = new WWW (url);
	        yield return www;
	
	        retries = 0;
	        while ((www.error != null || www.text.ToLower() != "succeeded") && retries < maxRetries) {
				Debug.Log ("Had an error " + www.error + " or text was not succeeded " + www.text);
	            retries ++;
	            www = new WWW (url);
	            yield return www;
	        }
			if ((www.error != null || www.text.ToLower() != "succeeded")) {
				Debug.Log ("Had an error " + www.error + " or text was not succeeded " + www.text);
				SendMessage ("OnUnregisterHostFailed");
			} else {
				Debug.Log ("Successfully unregistered host");
				registered = false;
			}
			
    	}
	}

	void OnUnregisterHostFailed(){

	}

	//public void OnServerInitialized(){
	//	print ("Server Initialized");
	//}

	/*public IEnumerator OnServerInitialized () {



		//while (Network.player.externalPort == 65535) {
		//	print (Network.player.externalPort);
		//	yield return new WaitForSeconds(1f);
		//}

		print ("IEnumerator Server Initialized");

		string url = masterServerURL+"RegisterHost";
		url += "?gameType="+WWW.EscapeURL (gameType);
		url += "&gameName="+WWW.EscapeURL (gameName);
		url += "&comment="+WWW.EscapeURL (comment);
		//url += "&useNat=" + true;//!Network.HavePublicAddress();
		url += "&connectedPlayers="+Metwork.players.Count;//(Network.connections.Length + 1);
		url += "&playerLimit="+32;//Network.maxConnections;
		url += "&internalIp="+"192.168.2.30";//Network.player.ipAddress;
		url += "&internalPort="+10235;//Network.player.port;
		url += "&externalIp="+"204.123.32.5";//Network.player.externalIP;
		url += "&externalPort="+10235;//Network.player.externalPort;
		url += "&guid="+10234915;//Network.player.guid;
		url += "&passwordProtected="+0;//(Network.incomingPassword != "" ? 1 : 0);
		Debug.Log (url);
		var www = new WWW (url);
		yield return www;
		Debug.Log("Returned from url: " + www.text);

		retries = 0;
		while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
			print(www.text);
			retries ++;
			www = new WWW (url);
			yield return www;
			Debug.Log("Returned from url: " + www.text);
		}
		if ((www.error != null || www.text != "succeeded")) {
			Debug.Log("Returned from url with error: " + www.text);
			SendMessage ("OnRegisterHostFailed");
			print(www.text);
			yield break;
		}

		Debug.Log("Returned from url: " + www.text);


		RegisterHost (this.gameName, this.comment);
		Metwork.disconnectReason = DisconnectReason.Unexpected;
	}*/

	public void UpdateHost(string _comment){
		this.comment = _comment;
		StartCoroutine(CoUpdateHost());
	}
	public IEnumerator CoUpdateHost () {



		//while (Network.player.externalPort == 65535) {
		//	print (Network.player.externalPort);
		//	yield return new WaitForSeconds(1f);
		//}

		print ("Updating Host");

		string url = masterServerURL+"UpdateHost";
		url += "?gameType="+WWW.EscapeURL (gameType);
		url += "&gameName="+WWW.EscapeURL (gameName);
		url += "&comment="+WWW.EscapeURL (comment);
		//url += "&useNat=" + true;//!Network.HavePublicAddress();
		url += "&connectedPlayers="+manager.numPlayers;
		url += "&playerLimit=" + manager.maxConnections;
		url += "&internalIp="+"192.168.2.30";
		url += "&internalPort="+manager.GetComponent<kcp2k.KcpTransport>().Port;
		url += "&externalPort="+manager.GetComponent<kcp2k.KcpTransport>().Port;
		//TODO
		url += "&externalIp="+"204.123.32.5";//Network.player.externalIP;
		url += "&guid="+10234915;//Network.player.guid;
		url += "&passwordProtected="+0;//(Network.incomingPassword != "" ? 1 : 0);
		Debug.Log (url);
		var www = new WWW (url);
		yield return www;
		Debug.Log("Returned from url: " + www.text);

		retries = 0;
		while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
			print(www.text);
			retries ++;
			www = new WWW (url);
			yield return www;
			Debug.Log("Returned from url: " + www.text);
		}
		if ((www.error != null || www.text != "succeeded")) {
			Debug.Log("Returned from url with error: " + www.text);
			try{
			SendMessage ("OnUpdateHostFailed");
			}catch{}
			print(www.text);
			yield break;
		}

		Debug.Log("Returned from url: " + www.text);

	}

	void OnMetPlayerConnected(MetworkPlayer _player){
		StartCoroutine (CoOnPlayerConnected(_player));
	}
	// TODO update for the new Networking
	IEnumerator CoOnPlayerConnected(MetworkPlayer player)
	{
		//update players
		string url = masterServerURL+"UpdateHost";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
		url += "&connectedPlayers="+(Metwork.players.Count);
	    Debug.Log ("url " + url);
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "succeeded")) {
	        SendMessage ("OnUpdatePlayersFailed");
	    }
		Metwork.disconnectReason = DisconnectReason.Unexpected;
	}

	void OnMetPlayerDisconnected(MetworkPlayer _player){
		StartCoroutine (CoOnPlayerDisconnected(_player));
	}

	// TODO update for the new Networking
	IEnumerator CoOnPlayerDisconnected(MetworkPlayer player)
	{
		//updateplayers.php
		string url = masterServerURL+"UpdateHost";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
		url += "&connectedPlayers="+Metwork.players.Count;
	    Debug.Log ("url " + url);
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "succeeded")) {
	        SendMessage ("OnUpdatePlayersFailed");
	    }
	}
}

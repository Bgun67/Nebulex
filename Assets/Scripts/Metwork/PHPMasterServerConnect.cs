using UnityEngine;
//using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


public class MHostData{
	public int connectedPlayers;
	public string gameName;
	public string gameType;
	public string guid;
	public int playerLimit;
	public string[] ip;
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
	
	void Awake () {
        if (_instance != null) {
            DestroyImmediate (gameObject);
        } else {
		    DontDestroyOnLoad (gameObject);
        	_instance = this;
			Metwork.onPlayerConnected += _instance.OnMetPlayerConnected;
			Metwork.onPlayerDisconnected += _instance.OnMetPlayerDisconnected;
		}

	}

    void OnDestroy () {
        if (registered) {
			// Unregister without the CR
	        string url = masterServerURL+"UnregisterHost.php";
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
			if(_host.gameType == gameType && _host.connectedPlayers > 0 && _host.connectedPlayers < _host.playerLimit){
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
		print ("Querying Harder");
		atOnce = true;
		string url = masterServerURL+"QueryMS.php?gameType="+WWW.EscapeURL(gameType);
    	Debug.Log ("looking for URL " + url);
    	WWW www = new WWW (url);
    	yield return www;

    	retries = 0;
	    while (www.error != null && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);

	        yield return www;
	    }

	    if (www.error != null) {
	        SendMessage ("OnQueryMasterServerFailed");
	    }
	
		if (www.text == "") {
			atOnce = false;
			GetComponent<Match_Scene_Manager> ().DisplayMatches ();
			yield break;
		} else if (www.text == "empty") {
			hostData = null;
			GetComponent<Match_Scene_Manager> ().DisplayMatches ();
		} else {
			string[] hosts = new string[www.text.Split (";" [0]).Length];
			hosts = www.text.Split (";" [0]);
			hostData = new MHostData[hosts.Length];
			var index = 0;
			foreach (string host in hosts) {
				if (host == "")
					continue;
				string[] data = host.Split ("," [0]);
				hostData [index] = new MHostData ();
				hostData [index].ip = new string[1];
				hostData [index].ip [0] = data [0];
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
			try {
				GetComponent<Match_Scene_Manager> ().DisplayMatches ();
			} catch {

			}
		}

		atOnce = false;
	}

	public void RegisterHost (string pGameName, string pComment) {
		gameName = pGameName;
		comment = pComment;
		registered = true;
		StartCoroutine (RegistrationLoop ());
	}

	private IEnumerator RegistrationLoop()
	{
		while (registered && Metwork.isServer){//NetworkServer.active) {
			print("Registering");
            yield return StartCoroutine (RegisterHostCR());
    		yield return new WaitForSeconds(delayBetweenUpdates);
		}

		registered = false;
	}

    private IEnumerator RegisterHostCR () {
		Debug.Log("Attempting to register host: Try: " + retries.ToString());
	    string url = masterServerURL+"RegisterHost.php";
	    url += "?gameType="+WWW.EscapeURL (gameType);
	    url += "&gameName="+WWW.EscapeURL (gameName);
	    url += "&comment="+WWW.EscapeURL (comment);
		url += "&playerLimit=" + 32;//NetworkManager.singleton.matchSize;
		url += "&connectedPlayers="+Metwork.players.Count;//NetworkManager.singleton.numPlayers;
		url += "&internalIp="+"192.168.2.30";//NetworkManager.singleton.networkAddress;
		url += "&internalPort="+10235;//NetworkManager.singleton.networkPort;
		url += "&externalPort="+10235;//NetworkManager.singleton.networkPort;

		//if (NetworkManager.singleton.serverBindToIP) {
				url += "&externalIp="+"204.123.32.5";//+NetworkManager.singleton.serverBindAddress;
		//} else {
	    //	url += "&externalIp="+NetworkManager.singleton.networkAddress;
		//}
	    Debug.Log (url);
	
	    WWW www = new WWW (url);
	    yield return www;
	
	    retries = 0;
	    while ((www.error != null || www.text != "") && retries < maxRetries) {
	        retries ++;
	        www = new WWW (url);
	        yield return www;
	    }
	    if ((www.error != null || www.text != "")) {
			Debug.LogError (www.error);
	        SendMessage ("OnRegisterHostFailed");
		}
		Debug.Log("Recieved transmision: " + www.text);
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
			
	        string url = masterServerURL+"UnregisterHost.php";
	        url += "?gameType="+WWW.EscapeURL (gameType);
	        url += "&gameName="+WWW.EscapeURL (gameName);
			Debug.Log (url);
	        WWW www = new WWW (url);
	        yield return www;
	
	        retries = 0;
	        while ((www.error != null || www.text != "succeeded") && retries < maxRetries) {
				Debug.Log ("Had an error " + www.error + " or text was not succeeded " + www.text);
	            retries ++;
	            www = new WWW (url);
	            yield return www;
	        }
			if ((www.error != null || www.text != "succeeded")) {
				Debug.Log ("Had an error " + www.error + " or text was not succeeded " + www.text);
				SendMessage ("OnUnregisterHostFailed");
			} else {
				Debug.Log ("Successfully unregistered host");
			}
			registered = false;
    	}
	}

	//public void OnServerInitialized(){
	//	print ("Server Initialized");
	//}

	public IEnumerator OnServerInitialized () {



		//while (Network.player.externalPort == 65535) {
		//	print (Network.player.externalPort);
		//	yield return new WaitForSeconds(1f);
		//}

		print ("IEnumerator Server Initialized");

		string url = masterServerURL+"RegisterHost.php";
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
<<<<<<< HEAD
=======
	}

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

		string url = masterServerURL+"UpdateHost.php";
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
			try{
			SendMessage ("OnUpdateHostFailed");
			}catch{}
			print(www.text);
			yield break;
		}

		Debug.Log("Returned from url: " + www.text);

>>>>>>> Local-Git
	}

	void OnMetPlayerConnected(MetworkPlayer _player){
		StartCoroutine (CoOnPlayerConnected(_player));
	}
	// TODO update for the new Networking
	IEnumerator CoOnPlayerConnected(MetworkPlayer player)
	{
		string url = masterServerURL+"UpdatePlayers.php";
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
		string url = masterServerURL+"UpdatePlayers.php";
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

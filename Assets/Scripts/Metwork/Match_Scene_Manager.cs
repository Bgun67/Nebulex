using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Networking;
using System.Net;
using System.Collections.Specialized;
using System;
using Mirror;
using DOMJson;


public class Match_Scene_Manager : MonoBehaviour
{
    public enum MapType
    {
        Galaxy,
        Tenningrad,
        LHXUltimaBase
    }
    [System.Serializable]
    public class MapClass
    {
        //public MapType type;
        public string sceneName;
        public string displayedName;
        public Sprite mapImage;
        public string[] availableGames = new string[] { "Destruction", "Team Deathmatch", "Capture The Flag", "Astroball", "Meltdown" };
    }
    //public Network netView;
    public InputField gameNameInput;
    public InputField portInput;
    public GameObject loadingPanel;
    public RectTransform loadingCircle;


    public bool testing;
    //public string portString;
    //public string localIP;
    public float pollingTime = 10f;

    [Header("Map")]
    public MapClass[] maps;
    public int currentMapNum;
    public Text mapNameText;
    public Text matchInfoText;

    public Image mapImage;
    public Button[] matchTypeButtons;

    public Button[] hostButtons;
    MHostData[] hostData;
	//Used for finding online servers
    [HideInInspector]
    public PHPMasterServerConnect connection;
	//Used for finding lan servers
	private CustomNetworkDiscovery networkDiscovery;
    public bool isError = false;
    private CustomNetworkManager manager;
    public static Match_Scene_Manager Instance{
        get{
            if(_instance == null)
                _instance = FindObjectOfType<Match_Scene_Manager>();
            return _instance;
        }
    }
    private static Match_Scene_Manager _instance = null;

    // Use this for initialization
    void Awake()
    {
        manager = FindObjectOfType<CustomNetworkManager>();

        foreach (Button hostButton in hostButtons)
        {
            hostButton.gameObject.SetActive(false);
        }
        
        connection = FindObjectOfType<PHPMasterServerConnect>();
		networkDiscovery = FindObjectOfType<CustomNetworkDiscovery>();
        ChangeMatchType("Destruction");
        ChangeMap(0);
        StartCoroutine(GetMatches());
    }
    void Start(){
        if (CustomNetworkManager.m_IsDedicatedServer){
            int mapNum = 0;
            string gameName = "Dedicated Server";
            int port = 7;

            try{
                string configJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/server_config.json");
                JsonObject serverConfig = JsonObject.FromJson(configJson);
                port = serverConfig.Get("port", 7777);
            }
            catch{
                Debug.LogWarning("Could not read server config file, using defaults");
            }
            connection.comment = maps[mapNum].sceneName;
            networkDiscovery.comment = maps[mapNum].sceneName;
            connection.gameName = gameName;
            networkDiscovery.gameName = gameName;
            manager.GetComponent<kcp2k.KcpTransport>().Port = (ushort)7777;

			manager.StartServer();
            manager.onStartServer += OnMetServerInitialized;
		}else{
            networkDiscovery.StartDiscovery();
        }
    }

    void Update(){
        loadingCircle.Rotate(0,0,2f * Time.deltaTime*60f);
    }


    public IEnumerator GetMatches()
    {
        while (this.enabled)
        {

            if (manager.useLan)
            {
                matchInfoText.text = "SEARCHING FOR LAN MATCHES...";
            }
            else
            {
                matchInfoText.text = "SEARCHING FOR ONLINE MATCHES...";
                connection.QueryPHPMasterServer();
            }



            yield return new WaitForSecondsRealtime(pollingTime);
        }

    }

    public void DisplayMatches()
    {
        if (manager.useLan)
        {
            hostData = networkDiscovery.PollHostList();
        }
        else
        {
            hostData = connection.PollHostList();
        }
        foreach (Button hostButton in hostButtons)
        {
            hostButton.gameObject.SetActive(false);
        }
        if (hostData != null && hostData.Length > 0)
        {
            if (manager.useLan)
            {
                matchInfoText.text = hostData.Length.ToString() + " LAN MATCHES FOUND";
            }
            else
            {
                matchInfoText.text = hostData.Length.ToString() + " ONLINE MATCHES FOUND";
            }
            for (int i = 0; i < hostData.Length; i++)
            {
                string _mapDisplayedName = new List<MapClass>(maps).Find(x => x.sceneName == hostData[i].comment).displayedName;
                hostButtons[i].GetComponentInChildren<Text>().text = hostData[i].gameName + " " + hostData[i].gameType + " : " + _mapDisplayedName + " " + hostData[i].connectedPlayers + "/" + hostData[i].playerLimit;
                hostButtons[i].gameObject.SetActive(true);
            }

        }
        else
        {
            if (manager.useLan)
            {
                matchInfoText.text = "NO LAN MATCHES FOUND";
            }
            else
            {
                matchInfoText.text = "NO ONLINE MATCHES FOUND";
            }
            for (int i = 0; i < hostButtons.Length; i++)
            {
                hostButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ChangeMatchType(string _matchType)
    {
        System.IO.File.WriteAllLines(Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon(new string[] {
            "1200",
            _matchType,
            maps[currentMapNum].sceneName
        }));

        connection.gameType = _matchType;
		networkDiscovery.gameType = _matchType;

        //Hide the buttons from the old match type
        for (int i = 0; i < hostButtons.Length; i++)
        {
            hostButtons[i].gameObject.SetActive(false);
        }
        if (manager.useLan)
        {
            matchInfoText.text = "SEARCHING FOR LAN MATCHES...";
        }
        else
        {
            matchInfoText.text = "SEARCHING FOR ONLINE MATCHES...";
        }
        connection.QueryPHPMasterServer();
    }

    public void ChangeMap(int direction)
    {
        currentMapNum += direction;
        if (currentMapNum < 0)
        {
            currentMapNum = maps.Length - 1;
        }
        else if (currentMapNum > maps.Length - 1)
        {
            currentMapNum = 0;
        }
        MapClass map = maps[currentMapNum];
        mapNameText.text = map.displayedName;
        mapImage.sprite = map.mapImage;
        manager.onlineScene = map.sceneName;
        ShowAllowedMatchTypes(map);
    }
    void ShowAllowedMatchTypes(MapClass _map)
    {
        bool _isNewTypeSet = false;
        foreach (Button matchButton in matchTypeButtons)
        {
            matchButton.interactable = false;
            foreach (string availableGame in _map.availableGames)
            {
                if (matchButton.GetComponentInChildren<Text>().text.ToLower() == availableGame.ToLower())
                {
                    if (!_isNewTypeSet)
                    {
                        ChangeMatchType(availableGame);
                        _isNewTypeSet = true;
                    }
                    matchButton.interactable = true;
                }
            }
        }
    }

    public void JoinServer(int index)
    {

        if (testing)
        {

        }
        else
        {
            //Metwork.Connect(hostData[index].gameName);
            //TODO switch this to an IP instead;
            manager.networkAddress = hostData[index].ip;
            manager.GetComponent<kcp2k.KcpTransport>().Port = (ushort)hostData[index].port;
            
			print("Connecting to server: " + manager.networkAddress + " port: " + manager.GetComponent<kcp2k.KcpTransport>().Port);
			manager.StartClient();

        }
        //TODO?
        //Metwork.onConnectedToServer += OnConnectedToMetServer;
        //Metwork.onPlayerConnected += OnMetPlayerConnected;
        System.IO.File.WriteAllLines(Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon(new string[] {
            "1200",
            hostData[index].gameType,
            hostData[index].comment
        }));


        connection.gameType = hostData[index].gameType;
        networkDiscovery.gameType = hostData[index].gameType;
        connection.gameName = hostData[index].gameName;
        networkDiscovery.gameName = hostData[index].gameName;
        //unsecure, but should not fail
        try
        {
            loadingPanel.SetActive(true);
        }
        catch
        {
            Debug.LogWarning("Loading panel could not be activated");
        }

        //Moved from 8 to 12 seconds to increase likelyhood of properly connecting
        Invoke("DeactivateLoadPanel", 12f);



    }


    void DeactivateLoadPanel()
    {
        //TODO: Reload the scene when the network connection fails! or make workaround! 
        //unsecure, but should not fail
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    //Direct Connection without matchmaking
    public void DirectConnect()
    {

        if (testing)
        {

        }
        else
        {
            manager.networkAddress = gameNameInput.text;
            manager.GetComponent<kcp2k.KcpTransport>().Port = (ushort)int.Parse(portInput.text);
            print("Directly connecting to: " + manager.networkAddress + " port: " + manager.GetComponent<kcp2k.KcpTransport>().Port);
            manager.StartClient();
        }

        //unsecure, but should not fail
        try
        {
            loadingPanel.SetActive(true);


        }
        catch
        {
            Debug.LogWarning("Loading panel could not be activated");
        }

        //Moved from 8 to 12 seconds to increase likelyhood of properly connecting
        Invoke("DeactivateLoadPanel", 12f);



    }

    public void OnMetPlayerConnected(MetworkPlayer _player)
    {
        //if (Metwork.player == null || _player.connectionID == Metwork.player.connectionID && (SceneManager.GetActiveScene().name == "MatchScene")) {
        //	SceneManager.LoadScene ("LobbyScene");
        //	Destroy (this);
        //}
    }

    public void OnConnectedToMetServer()
    {
        if (SceneManager.GetActiveScene().name == "MatchScene")
        {
            //SceneManager.LoadScene ("LobbyScene");
        }

        Destroy(this);
    }

    public void QuitToMainMenu()
    {
        if(manager.isServerMachine){
            manager.StopServer();
        }
        manager.StopClient();

        //if (mMetwork != null)
        //{
        //	Cleanup();
        //}
        //try{
        //Byn.Net.WebRtcNetworkFactory.Instance = null;
        //Destroy(GameObject.FindObjectOfType<Byn.Net.WebRtcNetworkFactory>().gameObject);
        Destroy(this.gameObject);
        SceneManager.LoadScene("Start Scene");
    }

    public void OnValueChanged()
    {
        if (gameNameInput.text.Contains("%") || gameNameInput.text.Contains("["))
        {
            gameNameInput.text = gameNameInput.text.Substring(0, gameNameInput.text.Length - 1);
        }
        if (gameNameInput.text.Contains("\n"))
        {
            gameNameInput.text = gameNameInput.text.Substring(0, gameNameInput.text.Length - 1);
            StartServer();
        }

    }

    public void OnUseLanChanged(bool _useLan)
    {
        manager.useLan = _useLan;

        // TODO: Move to common function
        //Hide the buttons from the old match type
        for (int i = 0; i < hostButtons.Length; i++)
        {
            hostButtons[i].gameObject.SetActive(false);
        }
        if (manager.useLan)
        {
            matchInfoText.text = "SEARCHING FOR LAN MATCHES...";
        }
        else
        {
            matchInfoText.text = "SEARCHING FOR ONLINE MATCHES...";
        }
        connection.QueryPHPMasterServer();
    }

    public void StartServer()
    {
        if (isError)
        {
            return;
        }

        if (gameNameInput.text != "")
        {
            connection.comment = maps[currentMapNum].sceneName;
            networkDiscovery.comment = maps[currentMapNum].sceneName;
            connection.gameName = gameNameInput.text;
            networkDiscovery.gameName = gameNameInput.text;
            manager.GetComponent<kcp2k.KcpTransport>().Port = (ushort)int.Parse(portInput.text);

            manager.StartHost();
            manager.onStartServer += OnMetServerInitialized;
            if (testing)
            {
                //Network.InitializeServer (32, int.Parse (portString), false);
            }
            else
            {

                //send server notifications
                //TODO: Disabled for now
                //SendDiscordNotification(gameNameInput.text);
            }

            //unsecure, but should not fail
            try { loadingPanel.SetActive(true); } catch { }
            Invoke("DeactivateLoadPanel", 5f);

        }
        else
        {
            gameNameInput.placeholder.GetComponent<Text>().text = $"<color=red>Server name required!</color>";

        }
    }
    void SendDiscordNotification(string matchName)
    {
        string[] sassyNotificationArray = new string[]
        {
            "...Super",
            "Join it now! You have no choice",
            "Please join, you're my only hope",
            "Join it... you know... if you want I don't care",
            "Looks like everybody gets a server",
            "They're just giving these out to everyone",
            "**Sigh**, why do I always have to tell them?",
            "..And it's already more interesting than you",
            "My date's arrived!",
            "Snape, Snape, Serverus Snape",
            "I want YOU to join this server - Abraham Lincoln",
            "Get to the lifeboats Cap'n",
            "This does please the bot",
            "It's over, I have the high ground",
            "**Shouting from behind: DID YOU TELL THEM ABOUT THE SERVER?** YES MA I TOLD THEM ABOUT THE SERVER!"
        };
        if (!matchName.ToLower().Contains("test"))
        {
            using (WebClient webClient = new WebClient())
            {
                string url = "https://discordapp.com/api/webhooks/549738720337723394/tHDrL0mYrR-bt0IS6FeP2t2ukrGtxt5XyYwqv0BpmBVu0IBHJXQXp3s6gFxywRGPBS9S";
                NameValueCollection pairs = new NameValueCollection()
                {
                    {
                        "username",
                        "Disappointed Server Bot"
                    },
                    {
                        "content",
                        "Server "+ matchName +" has been created at "+DateTime.Now.ToString()+"\r\n"+sassyNotificationArray[UnityEngine.Random.Range(0,sassyNotificationArray.Length)]
                    }
                };
                webClient.UploadValues(url, pairs);
            }

        }


    }
    public void OnMetServerInitialized()
    {
        Destroy(this);
    }

    public void OnQueryMasterServerFailed()
    {
        print("Master Server Query Failed");
        //isError = true;
    }


}

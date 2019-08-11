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
		public string[] availableGames = new string []{ "Destruction", "Team Deathmatch", "Capture The Flag", "Astroball", "Meltdown" };
	}
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

	[Header("Map")]
	public MapClass[] maps;
	public int currentMapNum;
	public Text mapNameText;
	public Image mapImage;
	public Button[] matchTypeButtons;

	public Button[] hostButtons;
	MHostData[] hostData;
	public PHPMasterServerConnect connection;
	public bool isError = false;

	// Use this for initialization
	void Awake()
	{
		foreach (Button hostButton in hostButtons)
		{
			hostButton.gameObject.SetActive(false);
		}
		if (testing)
		{
			localIP = File.ReadAllLines(Application.persistentDataPath + "/Player Data.txt")[3];
		}
		connection = this.GetComponent<PHPMasterServerConnect>();
		ChangeMatchType("Destruction");
		ChangeMap(0);
		StartCoroutine(GetMatches());
	}


	public IEnumerator GetMatches()
	{
		while (this.enabled)
		{

			connection.QueryPHPMasterServer();
			print("Querying");

			yield return new WaitForSecondsRealtime(pollingTime);
		}

	}

	public void DisplayMatches()
	{
		hostData = GetComponent<PHPMasterServerConnect>().PollHostList();
		foreach (Button hostButton in hostButtons)
		{
			hostButton.gameObject.SetActive(false);
		}
		if (hostData != null && hostData.Length > 0)
		{
			for (int i = 0; i < hostData.Length; i++)
			{
				if(hostData[i].comment != this.maps[currentMapNum].sceneName){
					continue;
				}
				hostButtons[i].GetComponentInChildren<Text>().text = hostData[i].gameName + " " + hostData[i].gameType + " " + hostData[i].connectedPlayers + "/" + hostData[i].playerLimit;
				hostButtons[i].gameObject.SetActive(true);
			}

		}
		else
		{
			print("No games currently available");
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

		//Hide the buttons from the old match type
		for (int i = 0; i < hostButtons.Length; i++)
		{
			hostButtons[i].gameObject.SetActive(false);
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
					if(!_isNewTypeSet){
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
			//Network.Connect (localIP, 12345);//hostData [index].port);
		}
		else
		{
			//Network.Connect (hostData [index].guid);
			Metwork.Connect(hostData[index].gameName);

		}

		Metwork.onConnectedToServer += OnConnectedToMetServer;
		Metwork.onPlayerConnected += OnMetPlayerConnected;
		System.IO.File.WriteAllLines(Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon(new string[] {
			"1200",
			hostData[index].gameType,
			maps[currentMapNum].sceneName
		}));
<<<<<<< HEAD
		connection.gameType = hostData [index].gameType;
		connection.gameName = hostData [index].gameName;
		//unsecure, but should not fail
		try { loadingPanel.SetActive(true);

		}catch { }

		Invoke("DeactivateLoadPanel", 8f);
=======
		connection.gameType = hostData[index].gameType;
		connection.gameName = hostData[index].gameName;
		//unsecure, but should not fail
		try
		{
			loadingPanel.SetActive(true);
			

		}
		catch { 
			Debug.LogWarning("Loading panel could not be activated");
		}
>>>>>>> Local-Git

		//Moved from 8 to 12 seconds to increase likelyhood of properly connecting
		Invoke("DeactivateLoadPanel", 12f);



	}
	void DeactivateLoadPanel()
	{
		//TODO: Reload the scene when the network connection fails! or make workaround! 
		Debug.Log("Showing loading panel");
		//unsecure, but should not fail
		loadingPanel.SetActive(false);
	}
<<<<<<< HEAD
	void DeactivateLoadPanel()
	{
		//unsecure, but should not fail
		loadingPanel.SetActive(false);
	}
	public void OnMetPlayerConnected(MetworkPlayer _player){
=======
	public void OnMetPlayerConnected(MetworkPlayer _player)
	{
>>>>>>> Local-Git
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
		Metwork.Disconnect();
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

		if (gameNameInput.text.Contains("\n"))
		{
			gameNameInput.text = gameNameInput.text.Substring(0, gameNameInput.text.Length - 1);
			StartServer();
		}

	}

	public void StartServer()
	{
		if (isError)
		{
			return;
		}

		if (gameNameInput.text != "")
		{
<<<<<<< HEAD
=======
			connection.comment = maps[currentMapNum].sceneName;
>>>>>>> Local-Git
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
<<<<<<< HEAD
=======
			//send server notifications
			SendDiscordNotification(gameNameInput.text);
>>>>>>> Local-Git
		}
		else
		{
			print("Please input a game name");
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
		SceneManager.LoadScene("LobbyScene");
		StartCoroutine(GameObject.FindObjectOfType<PHPMasterServerConnect>().OnServerInitialized());
		Destroy(this);
	}

	public void OnQueryMasterServerFailed()
	{
		print("Master Server Query Failed");
		//isError = true;
	}


}

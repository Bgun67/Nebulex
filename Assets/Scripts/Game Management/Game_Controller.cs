using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game_Controller : MonoBehaviour {

	[System.Serializable]
	public class PlayerStats{
		public string name = "THis is not really a name";
		public int kills = -1;
		public int deaths = -1;
		public int assists = 0;
		public int score = 0;
		public int team = -1;
		public bool isBot = false;
		public bool isFilled = false;
	}

	[System.Serializable]
	public struct GameType{
		public const string Destruction = "Destruction";
		public const string TeamDeathmatch = "Team Deathmatch";
		public const string CTF = "Capture The Flag";
		public const string Meltdown = "Meltdown";
		public const string Soccer = "AstroBall";

	}
	private static Game_Controller instance;
	public static Game_Controller Instance{
		get{
			if(instance == null){
				instance = FindObjectOfType<Game_Controller>();
			}
			return instance;
		}
	}

	public List<GameObject> playerObjects = new List<GameObject> ();
	[HideInInspector]
	public List<Com_Controller> bots = new List<Com_Controller>();



	public PlayerStats[] statsArray = new PlayerStats[32];


	public List<Transform> playerSpawnTransforms = new List<Transform> ();
	public int currentTeamNum;
	public string gameMode;

	public bool finished = false;
	public Damage carrierADmg;
	public Damage carrierBDmg;
	public int winningTeam;

	public Transform shipOneTransform;
	public Transform shipTwoTransform;
	public GameObject sceneCam;
	public MetworkView netView;

	public Flag flagA;
	public Flag flagB;

	public GameObject soccerField;

	public GameObject radiationField;
	//A variable to override the default winner behaviour
	public int overrideWinner = 1000;

	public int netPlayersCount;


	[Space(5f)]
	#region UI
	public GameObject localPlayer;
	[Header("UI Variables")]
	public int matchLength;
	public int currentTime;
	public float fps;
	public int scoreA;
	public int scoreB;
	[Header( "UI Objects")]
	public Text UI_timeText;
	Text UI_homeScoreText;
	Image UI_homeColour;
	Text UI_awayScoreText;
	Image UI_awayColour;
	public Text UI_fpsText;
	public GameObject eventSystem;
	public GameObject gameplayUI;
	[Header("End of Game")]
	public GameObject endOfGameUI;
	public Text winningTeamText;
	public Text winnerNamesText;
	public Text loserNamesText;
	public Text winnerKillsText;
	public Text loserKillsText;
	public Text endTimeText;
	public Text endScoreText;
	public GameObject winPanel;
	public double initialTime;

	public bool GameClipMode = false;
	public GameObject GameClipCameraPrefab;
	public Camera GameClipCamera;
	public float GameClipCameraOffset = 5f;
	public Transform GameClipTarget;


	#endregion

	[MRPC]
	public void RPC_UpdateMatchInfo()
	{
		if (!Metwork.isServer)
		{
			return;
		}
		netView.RPC("RPC_UpdateMatchScore", MRPCMode.OthersBuffered, new object[] { scoreA, scoreB });
		for (int i = 0; i < statsArray.Length; i++)
		{
			PlayerStats stat = statsArray[i];
			if (stat.name == "")
			{
				continue;
			}
			netView.RPC("RPC_UpdateStatsArrayEntry", MRPCMode.OthersBuffered, new object[] { i, stat.name, stat.kills, stat.deaths, stat.assists, stat.score });
		}
	}
	[MRPC]
	public void RPC_UpdateMatchScore(int _scoreA, int _scoreB)
	{
		scoreA = _scoreA;
		scoreB = _scoreB;
	}
	[MRPC]
	public void RPC_UpdateStatsArrayEntry(int _index,string _name, int _kills, int _deaths, int _assists, int _score)
	{
		PlayerStats _stat = statsArray[_index];
		_stat.name = _name;
		_stat.kills = _kills;
		_stat.deaths = _deaths;
		_stat.assists = _assists;
		_stat.score = _score;

		statsArray[_index] = _stat;
	}

	public void Start()
	{

		instance = Instance;
		UI_homeScoreText = UI_Manager.GetInstance.UI_HomeScoreText;
		UI_awayScoreText = UI_Manager.GetInstance.UI_AwayScoreText;

		UI_homeColour = UI_Manager.GetInstance.UI_HomeColour;
		UI_awayColour = UI_Manager.GetInstance.UI_AwayColour;


		netView = this.GetComponent<MetworkView>();
		GetLocalPlayer();
		
		Physics.autoSimulation = false;
		//eventSystem.SetActive (false);
		RPC_SetTeam();


		if (SceneManager.GetActiveScene().name != "LobbyScene")
		{
			InvokeRepeating("GameUpdate", 1f, 1f);
			InvokeRepeating("UpdateUI", 1f, 0.1f);
			InvokeRepeating("UpdateHost", 130f, 10f);
		}
		if (!SceneManager.GetSceneByName("SpawnScene").isLoaded)
		{
			SceneManager.LoadScene("SpawnScene", LoadSceneMode.Additive);
		}



		try
		{
			string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
			this.matchLength = int.Parse(matchSettings[0]);
			initialTime = Time.time;//Network.time;
			this.gameMode = matchSettings[1];
		}
		catch
		{
			Debug.LogError("Failed to read data from Match Settings.txt");

			string[] matchSettings = Profile.RestoreMatchFile();
			this.matchLength = int.Parse(matchSettings[0]);
			//TODO: Update
			initialTime = Time.time;//Network.time;
			this.gameMode = matchSettings[1];
		}
		currentTime = matchLength;

		if (gameMode == GameType.TeamDeathmatch)
		{

		}

		if (gameMode == GameType.CTF)
		{
			flagA.gameObject.SetActive(true);
			flagA.StartGame();
			flagB.gameObject.SetActive(true);
			flagB.StartGame();
			print("LEt's PlAy");
		}
		if (gameMode == GameType.Meltdown)
		{
			radiationField.SetActive(true);
			radiationField.GetComponent<Radiation>().carrier = shipOneTransform.gameObject;
			/*foreach (GameObject position in GameObject.FindGameObjectsWithTag("Spawn Point 1"))
			{
				position.tag = "Untagged";

			}
			GameObject[] spawnPositions = GameObject.FindGameObjectsWithTag("Spawn Point 0");
			for (int i = 0; i < spawnPositions.Length; i++)
			{
				if (i % 2 == 0)
				{
					spawnPositions[i].tag = "Spawn Point 1";
				}
				else
				{
					spawnPositions[i].tag = "Spawn Point 0";

				}


			}*/
			shipTwoTransform.gameObject.SetActive(false);
			shipTwoTransform = shipOneTransform;

		}
		if (gameMode == GameType.Soccer)
		{
			if (soccerField != null)
			{
				soccerField.SetActive(true);
			}
		}
		//call to server to sync the scores on all clients
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_UpdateMatchInfo", MRPCMode.Server, new object[] { });
		}
		Invoke("PhysicsUpdate", 1f);

	}
	public int GetLocalPlayer(){
		//foreach (Player_Controller player in FindObjectsOfType<Player_Controller>()) {
		foreach (GameObject player in playerObjects) {
			Metwork_Object _metObj = player.GetComponent<Metwork_Object>();
			if (_metObj!=null && _metObj.isLocal) {
				localPlayer = player;
				return _metObj.netID;
			}
		}
		return -1;
	}
	void PhysicsUpdate(){
		Physics.autoSimulation = true;
		
	}
	void UpdateHost(){
		if(Metwork.isServer){
			string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
			string _newScene = matchSettings[2];
			FindObjectOfType<PHPMasterServerConnect>().UpdateHost (_newScene);
		}
	}

	[MRPC]
	public void RPC_AddPlayerStat(string name, int _owner, bool _isBot){
		PlayerStats stat = statsArray[_owner];
		stat.name = name;
		stat.kills = 0;
		stat.deaths = 0;
		stat.score = 0;
		stat.isBot = _isBot;
		stat.isFilled = true;

		//De (re)activate the bot here
		int _botIndex = _owner > 64 ? _owner : _owner + 64;
		if(GetBotFromPlayerID(_botIndex) != null)
			GetBotFromPlayerID(_botIndex).gameObject.SetActive(_isBot);
		
		

		//zero is left empty to comply with one indexed
		statsArray[_owner] =  stat;
		RPC_SetTeam ();

	}


	public void RPC_SetTeam(){
		
		for(int i = 1; i<statsArray.Length; i++) {
			int _id = i;
			int team = (i+1) % 2;
			statsArray [_id].team = team;
			if (playerObjects.Count >= i) {
				Player_Controller _player = GetPlayerFromNetID (_id).GetComponent<Player_Controller> ();
				//Set the colour highlights of the player's "jersey" meshes
				for(int j = 0; j < _player.jerseyMeshes.Length; j++){
					_player.jerseyMeshes[j].sharedMaterial = _player.teamMaterials[team];
				}
			}
		}
		
	}
	


	public void GameUpdate(){
		//unlock cursor
		if (Input.GetKey(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		

		foreach (Player_Controller player in GameObject.FindObjectsOfType<Player_Controller>()) {
			if (player.GetComponent<Metwork_Object> ().isLocal) {
				localPlayer = player.gameObject;
			}
		}

		currentTime = currentTime - 1;
		if (currentTime <= 0) {
			EndGame ();
		}
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			UpdateShipHealths ();
			if (Metwork.isServer) {
				netView.RPC ("RPC_UpdateTime", MRPCMode.Others, currentTime);
			}
		}

		fps = 1f / Time.deltaTime;
		
		switch (gameMode) {
		case "Destruction":
			scoreA = carrierADmg.currentHealth;
			scoreB = carrierBDmg.currentHealth;
			if (scoreA <= 0 || scoreB <= 0) {
				EndGame ();
			}
			break;
		case "Team Deathmatch":
			if (scoreA >= 100 || scoreB >= 100) {
				EndGame ();
			}
			break;
		case "Capture The Flag":

			break;
		case "Meltdown":
			scoreA = carrierADmg.currentHealth;
			scoreB = carrierADmg.originalHealth-carrierADmg.currentHealth;

			if (scoreA <= 0) {
				EndGame ();
			}
			break;
		case "Soccer":

			break;
		default:
			break;
		}



	}
	[MRPC]
	public void RPC_UpdateTime(int _time){
		currentTime = _time;
	}
	public void UpdateShipHealths(){
		if(carrierADmg == null){
			return;
		}

		if (carrierADmg.netObj.isLocal) {
			netView.RPC ("RPC_UpdateShipHealths", MRPCMode.OthersBuffered, new object[]{ 0, carrierADmg.currentHealth});
		}
		if (carrierBDmg.netObj.isLocal) {
			netView.RPC("RPC_UpdateShipHealths", MRPCMode.OthersBuffered, new object[]{ 1, carrierBDmg.currentHealth});
		}
	}

	[MRPC]
	public void RPC_UpdateShipHealths(int carrierNum, int _damage){
		if (carrierNum == 0) {
			carrierADmg.currentHealth = _damage;
		} else {
			carrierBDmg.currentHealth = _damage;
		}
	}

	public void UpdateUI(){



		int minutes = Mathf.FloorToInt(currentTime / 60f );
		int seconds =Mathf.FloorToInt( currentTime % 60);

		//UI_fpsText.text = "fps:"+fps;

		//Update the scores of the teams, the local team being on the
		//left side and the opposing team being on the right
		if (localPlayer != null && GetLocalTeam() == 0) {
			UI_homeScoreText.text = scoreA.ToString();
			UI_awayScoreText.text = scoreB.ToString();
			UI_homeColour.color = new Color(0,1f,0);
			UI_awayColour.color = new Color(1f,0f,0);
		} else if (GetLocalTeam() == 1){
			UI_homeScoreText.text = scoreB.ToString();
			UI_awayScoreText.text = scoreA.ToString();
			UI_awayColour.color = new Color(0,1f,0);
			UI_homeColour.color = new Color(1f,0f,0);
		}
		else {
			UI_homeScoreText.text = scoreB.ToString() + "*";
			UI_awayScoreText.text = scoreA.ToString();
			UI_awayColour.color = new Color(0,1f,0);
			UI_homeColour.color = new Color(1f,0f,0);
		}

		//Update the time
		UI_timeText.text = minutes.ToString("00")+":"+ seconds.ToString("00");


	}
	public void EndGame(){
		//Time.timeScale = 0.5f;
		CancelInvoke ("GameUpdate");
		CancelInvoke ("UpdateUI");
		
		switch (gameMode) {
		case "Destruction":
			scoreA = carrierADmg.currentHealth;
			scoreB = carrierBDmg.currentHealth;
			if (scoreA > scoreB) {
				winningTeam = 0;
			}
			else if (scoreA < scoreB) {
				winningTeam = 1;
			}
			 else {
				winningTeam = -1;
			}
			break;
		case "Team Deathmatch":
			if (scoreA > scoreB) {
				winningTeam = 0;
			}
			else if (scoreA < scoreB) {
				winningTeam = 1;
			}
			 else {
				winningTeam = -1;
			}
			break;
		case "Capture The Flag":
			//Check if the overrideWinner is the default
			//If it isn't that means one have the flags have been captured
			if (overrideWinner != 1000) {
				winningTeam = overrideWinner;
			}
			//The game has timed out
			else if (scoreA > scoreB) {
				winningTeam = 0;
			}
			else if (scoreA < scoreB) {
				winningTeam = 1;
			}
			 else {
				winningTeam = -1;
			}
			break;
		case "Meltdown":
			scoreA = carrierADmg.currentHealth;
			scoreB = carrierADmg.originalHealth-carrierADmg.currentHealth;
			if (scoreA >0) {
				winningTeam = 0;
			}
			else {
				winningTeam = 1;
			}
			break;
		default:
			break;

		}

		List<PlayerStats> winners = new List<PlayerStats> ();
		List<PlayerStats> losers = new List<PlayerStats> ();

		foreach (PlayerStats player in statsArray) {
			if (player.name == "") {
				continue;
			}
			if (player.team == winningTeam) {
				player.score += 50;
				winners.Add (player);
				print ("We have a winner");
			}
			//Tie
			else if(winningTeam == -1){
				player.score += 50;
				if(player.team == 0){
					winners.Add (player);
				}
				else{
					losers.Add (player);
				}
				print ("We have a tie");
			} else {
				losers.Add (player);
				print ("loser");
			}

		}
		winnerNamesText.text = "";
		winnerKillsText.text = "";
		loserNamesText.text = "";
		loserKillsText.text = "";
		print ("Winners Length" + winners.Count);
		print ("Losers Length" + losers.Count);
		winners.Sort((x,y) => y.score.CompareTo(x.score));
		losers.Sort((x,y) => y.score.CompareTo(x.score));

		foreach (PlayerStats player in winners) {
			winnerNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16)) + "\r\n";
			winnerKillsText.text += player.kills.ToString() + "\t"  + player.deaths.ToString() + "\t" + player.assists.ToString() + "\r\n";
		}
		foreach (PlayerStats player in losers) {
			loserNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16))  + "\r\n";
			loserKillsText.text += player.kills.ToString() + "\t"  + player.deaths.ToString() + "\t" + player.assists.ToString() + "\r\n";

		}
		//Hide the pause menu, all the players should be destroyed anyway so everything should be peachy
		UI_Manager._instance.pauseMenu.gameObject.SetActive(false);
		if(SceneManager.GetSceneByName("SpawnScene").isLoaded == true){
			SceneManager.UnloadSceneAsync("SpawnScene");
		}
		endOfGameUI.SetActive (true);
		gameplayUI.SetActive (false);

		if (statsArray[localPlayer.GetComponent<Metwork_Object>().netID].team == winningTeam)
		{
			winningTeamText.text = "Your Team Wins!";
		}
		else if(winningTeam == -1){
			winningTeamText.text = "Tie Game!";
		}
		else
		{
			winningTeamText.text = "Your Team Lost!";
		}
		if (scoreA < 0) {
			scoreA = 0;
		}
		if (scoreB < 0) {
			scoreB = 0;
		}
		endScoreText.text = scoreA + ":" + scoreB;
		int minutes = Mathf.FloorToInt(currentTime / 60f );
		int seconds =Mathf.FloorToInt( currentTime % 60);
		if (minutes > 0) {

			endTimeText.text = "Remaining Time: " + minutes.ToString ("00") + ":" + seconds.ToString ("00");
		} else {
			endTimeText.text = "Remaining Time: 0:00"; 
		}
		endTimeText.text = "Next match in 20 sec";
		eventSystem.SetActive(true);
		SavePlayerScore();

		Invoke("RestartGame", 20.0f);


	}
	public void SavePlayerScore(){
		string[] data = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.persistentDataPath+"/Player Data.txt"));
		int previousScore = int.Parse( data [1]);
		data [1] =( previousScore+statsArray [localPlayer.GetComponent<Metwork_Object> ().netID].score).ToString();
		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Player Data.txt", Util.ThiccWatermelon(data));
			

	}
	public void RPC_EndGame(){

	}

	public static GameObject GetGameObjectFromNetID(int _netID){
		Metwork_Object[] netObjects = GameObject.FindObjectsOfType<Metwork_Object> ();

		for (int i = 0; i < netObjects.Length; i++) {
			if (netObjects [i].netID == _netID) {
				return netObjects [i].gameObject;
			}
		}
		print ("Failed to find Net Object " + _netID.ToString());
		return GameObject.CreatePrimitive(PrimitiveType.Sphere);
	}

	public static Com_Controller GetBotFromPlayerID(int _playerID){
		for (int i = 0; i < Instance.bots.Count; i++) {
			if (Instance.bots[i].botID == _playerID) {
				return Instance.bots[i];
			}
		}
		print ("Failed to find Bot " + _playerID.ToString());
		return null;
	}

	//safer than get gameobject but slower
	public GameObject GetPlayerFromNetID(int _netID){
		

		for (int i = 0; i < playerObjects.Count; i++) {
			if (playerObjects [i].GetComponent<Metwork_Object>().netID == _netID) {
				return playerObjects [i];
			}
		}
		Debug.LogWarning ("Could not find player with id: " + _netID.ToString());
		return GameObject.CreatePrimitive(PrimitiveType.Sphere);
	}

	[MRPC]
	public void RPC_ActivatePlayer(int _owner){
		GetPlayerFromNetID (_owner).SetActive (true);
	}

	public void AddAssist(int playerNum, int assistAmount){
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_AddAssist", MRPCMode.AllBuffered, new object[] {
				playerNum, assistAmount
			});
		} else {
			RPC_AddAssist (playerNum, assistAmount);
		}

	}
	[MRPC]
	public void RPC_AddAssist(int playerNum, int assistAmount){
		PlayerStats playerStat = statsArray [playerNum];
		playerStat.score += assistAmount;
		playerStat.assists++;
	}
	public void AddKill(int playerNum){

		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_AddKill", MRPCMode.AllBuffered, new object[] {
				playerNum
			});
		} else {
			RPC_AddKill (playerNum);
		}

	}
	[MRPC]
	public void RPC_AddKill(int playerNum){
		statsArray [playerNum].kills++;
		statsArray [playerNum].score += 100;

		if(playerNum == localPlayer.GetComponent<Metwork_Object>().netID){
			WindowsVoice.Speak("Target Down");
			
		}
	}
	public void AddDeath(int playerNum){
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_AddDeath", MRPCMode.AllBuffered, new object[] {
				playerNum
			});
		} else {
			RPC_AddDeath (playerNum);
		}

	}
	[MRPC]
	public void RPC_AddDeath(int playerNum){

		statsArray [playerNum].deaths++;
		if (statsArray [playerNum].team == 1) {
			scoreA++;
		} else {
			scoreB++;
		}
		
	}

	public void RestartGame()
	{
		print("Loading next scene Scene");
		//FindObjectOfType<Network_Manager>().minStartingPlayers = 8;
		//SceneManager.LoadScene("LobbyScene");
		string newScene = "LobbyScene";
		switch(SceneManager.GetActiveScene().name){
			case "LHX Ultima Base":
				newScene = "Crater";
				break;
			case "Crater":
				newScene = "Fracture";
				break;
			case "Fracture":
				newScene = "Space";
				break;
			case "Space":
				newScene = "Sector 19";
				break;
			case "Sector 19":
				newScene = "LHX Ultima Base";
				break;
			default:
				newScene = "MatchScene";
				break;
		}
		
		string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
		//TODO: Update to support all match types
		if(gameMode == "Team Deathmatch" || gameMode == "Capture The Flag"){
			matchSettings[2] = newScene;
			System.IO.File.WriteAllLines (Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon (matchSettings));
		}
		else{
			newScene = matchSettings[2];
		}
		
		
		if(Metwork.isServer){
			FindObjectOfType<PHPMasterServerConnect>().UpdateHost (newScene);
		}

		
		SceneManager.LoadScene("TransistionScene");
		//TODO: Assign variable gameplayUI of gamecontroller in LHX ULTIMA
		
	

	}
	public int GetLocalTeam()
	{
		GetLocalPlayer();
		return statsArray[localPlayer.GetComponent<Metwork_Object>().netID].team;
	}
	void Update()
	{
		if (Input.GetKeyDown("0") && false)
		{
			GameClipMode = !GameClipMode;
			if (GameClipMode)
			{
				localPlayer.GetComponent<Player_Controller>().mainCamObj.SetActive(false);

				foreach (TextMesh textMesh in FindObjectsOfType<TextMesh>())
				{
					textMesh.text = "";
				}
				GameClipCamera = Instantiate(GameClipCameraPrefab, localPlayer.transform.position, localPlayer.transform.rotation).GetComponent<Camera>();
			}
			else
			{
				localPlayer.GetComponent<Player_Controller>().mainCamObj.SetActive(true);
				localPlayer.GetComponent<Player_Controller>().mainCamObj.transform.position = GameClipCamera.transform.position;
				localPlayer.GetComponent<Player_Controller>().helmet.SetActive(false);
				Destroy(GameClipCamera.gameObject);
			}
		}
		if (GameClipMode)
		{
			if (GameClipTarget == null)
			{
				GameClipTarget = localPlayer.transform;
			}
			localPlayer.GetComponent<Player_Controller>().helmet.SetActive(true);
			GameClipCamera.transform.LookAt(GameClipTarget.position);
			Vector3 targetPosition = GameClipTarget.position - GameClipCamera.transform.forward * GameClipCameraOffset;
			GameClipCamera.transform.position = Vector3.Lerp(GameClipCamera.transform.position, targetPosition, 0.5f);
			GameClipCamera.transform.RotateAround(GameClipTarget.transform.position, Vector3.up, Input.GetAxis("Rotate Y")*5f);
			GameClipCamera.transform.RotateAround(GameClipTarget.transform.position, Vector3.right, Input.GetAxis("Rotate X")*5f);
			if (Input.GetKey(KeyCode.PageUp))
			{
				GameClipCameraOffset++;
			}
			else if (Input.GetKey(KeyCode.PageDown)){
				GameClipCameraOffset--;
			}
			if (Input.GetKeyDown(KeyCode.I)){
				GameClipCamera.enabled = !GameClipCamera.enabled;
			}
			if (Input.GetKeyDown("j"))
			{
				if (GameClipTarget != localPlayer.transform)
				{
					GameClipTarget = localPlayer.transform;
				}
				else
				{
					foreach (Ship_Controller ship in FindObjectsOfType<Ship_Controller>())
					{
						if (ship.player == localPlayer.gameObject)
						{
							GameClipTarget = ship.transform;
							break;
						}
					}
					foreach (Turret_Controller turret in FindObjectsOfType<Turret_Controller>())
					{
						if (turret.player == localPlayer.gameObject)
						{
							GameClipTarget = turret.transform;
							break;
						}
					}
				}
			}
			GameClipCamera.depth = 3;
		}
	}
	public static int GetTeam(int _viewID){
		return Game_Controller.instance.statsArray[_viewID].team;
	}
	public static int GetTeam(Player_Controller _player){
		return Game_Controller.instance.statsArray[_player.netObj.netID].team;
	}
	public static int GetTeam(GameObject _player){
		return Game_Controller.instance.statsArray[_player.GetComponent<Metwork_Object>().netID].team;
	}




}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class Game_Controller : NetworkBehaviour {

	[System.Serializable]
	public class PlayerStats{
		public string name = "Default Name";
		public int kills = 0;
		public int deaths = 0;
		public int assists = 0;
		public int score = 0;
		public int team = -1;
		public bool isBot = true;
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
	public Weapons_Catalog weaponsCatalog;
	public GameObject botPrefab;

	public List<Player_Controller> playerObjects = new List<Player_Controller> ();
	[HideInInspector]
	public List<Com_Controller> bots = new List<Com_Controller>();

	[SerializeField]
	public readonly SyncList<PlayerStats> playerStats = new SyncList<PlayerStats>();
	public PlayerStats[] debugPlayerStats = new PlayerStats[32];


	public List<Transform> playerSpawnTransforms = new List<Transform> ();
	public int currentTeamNum;
	[SyncVar (hook = nameof(SetGameMode))]
	public string gameMode;

	public bool finished = false;
	public Damage carrierADmg;
	public Damage carrierBDmg;
	public int winningTeam;

	public Transform shipOneTransform;
	public Transform shipTwoTransform;
	public Camera sceneCam;
	//public MetworkView netView;

	public Flag flagA;
	public Flag flagB;

	public GameObject soccerField;

	public GameObject radiationField;
	//A variable to override the default winner behaviour
	public int overrideWinner = 1000;

	public int netPlayersCount;


	
	#region UI
	public Player_Controller _localPlayer;
	public Player_Controller localPlayer{
		get{
			return _localPlayer;
		}
		set{
			_localPlayer = value;
		}
	}
	[Space(5f)]
	[Header("UI Variables")]
	public int matchLength;
	[SyncVar]
	//TODO: make this not an int
	public int currentTime;
	public float fps;
	[SyncVar]
	public int scoreA;
	[SyncVar]
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
	//Awake runs before any players are added
	public void Awake()
	{
		instance = Instance;
		UI_homeScoreText = UI_Manager.GetInstance.UI_HomeScoreText;
		UI_awayScoreText = UI_Manager.GetInstance.UI_AwayScoreText;

		UI_homeColour = UI_Manager.GetInstance.UI_HomeColour;
		UI_awayColour = UI_Manager.GetInstance.UI_AwayColour;

		playerStats.Callback += OnPlayerSync;

		//netView = this.GetComponent<MetworkView>();
		//Add the bots (we're going to ignore this for now)
		//This part should only be done on the server, but since is server only functions
		//on spawned objects, I don't really know what to do
		//TODO
		
		for(int i = 0; i<32; i++){
			PlayerStats _player = new PlayerStats();
			_player.name = "Player " + i.ToString();
			_player.isBot = true;
			//TODO: Properly assign the teams
			_player.team = i%2;
			playerStats.Add(_player);
		}

		//Force a copy of the debug array
		OnPlayerSync(SyncList<PlayerStats>.Operation.OP_ADD, 0, new PlayerStats(), new PlayerStats());
		
		//CHECK
		GetLocalPlayer();
		
		Physics.autoSimulation = false;
		//eventSystem.SetActive (false);
		RPC_SetTeam();


		if (SceneManager.GetActiveScene().name != "LobbyScene")
		{
			//TODO This should only runn on the server
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
		
		foreach(Spawn_Point spawn in FindObjectsOfType<Spawn_Point>()){
			spawn.DeactivateSpawn();
		}

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
		//TODO
		//if (Metwork.peerType != MetworkPeerType.Disconnected)
		//{
		//	netView.RPC("RPC_UpdateMatchInfo", MRPCMode.Server, new object[] { });
		//}
		Invoke("PhysicsUpdate", 1f);

	}
	void Start(){
		for(int i = 0; i<maxPlayers; i++){
			if(!playerStats[i].isBot)
				continue;
			PlayerStats _player = playerStats[i];
			_player.name = "Bot " + i.ToString();
			
			//TODO: Properly assign the teams
			
			playerStats[i] = _player;
			if (CustomNetworkManager.IsServerMachine())
			{
				//TODO: Assign these to spawn points
				GameObject _bot = Instantiate(botPrefab, Vector3.zero, Quaternion.identity);
				_bot.GetComponent<Com_Controller>().botID = i;
				bots.Add(_bot.GetComponent<Com_Controller>());
				NetworkServer.Spawn(_bot);
			}
		}
	}

	//Invoked when the game mode has changed
	void SetGameMode(string oldValue, string newValue){
		
	}
	

	[MRPC]
	public void RPC_UpdateMatchInfo()
	{
		if (!isServer)
		{
			return;
		}
		//TODO
		for (int i = 0; i < playerStats.Count; i++)
		{
			PlayerStats stat = playerStats[i];
			if (stat.name == "")
			{
				continue;
			}
			//TODO Turn this into a hash table or something
			//netView.RPC("RPC_UpdateplayerStatsEntry", MRPCMode.OthersBuffered, new object[] { i, stat.name, stat.kills, stat.deaths, stat.assists, stat.score });
		}
	}
	
	[MRPC]
	public void RPC_UpdateplayerStatsEntry(int _index,string _name, int _kills, int _deaths, int _assists, int _score)
	{
		PlayerStats _stat = playerStats[_index];
		_stat.name = _name;
		_stat.kills = _kills;
		_stat.deaths = _deaths;
		_stat.assists = _assists;
		_stat.score = _score;

		playerStats[_index] = _stat;
	}

	public int GetLocalPlayer(){
		if(localPlayer != null)
			return localPlayer.playerID;

		//TODO Replace this with playerObjects
		foreach (Player_Controller player in FindObjectsOfType<Player_Controller>()) {
			if (player.isLocalPlayer){
				localPlayer = player;
				return localPlayer.playerID;
			}
		}
		return -1;
	}
	void PhysicsUpdate(){
		Physics.autoSimulation = true;
		
	}
	/*Remove if unneeded
	void UpdateHost(){
		if(Metwork.isServer){
			string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
			string _newScene = matchSettings[2];
			FindObjectOfType<PHPMasterServerConnect>().UpdateHost (_newScene);
		}
	}*/

	public int AssignPlayerID(){
		int _playerID = -1;
		for(int i = 0; i<playerStats.Count; i++){
			if(playerStats[i].isBot == true){
				playerStats[i].isBot = false;
				_playerID = i;
				//TODO: Change this to a new playerstat so it can be updated automatically
				break;
			}
		}
		if(_playerID == -1)
			Debug.LogError("Invalid player ID: " + _playerID);
		return _playerID;
	}

	/*TODO: Remove
	[MRPC]
	public void RPC_AddPlayerStat(string name, int _owner, bool _isBot){
		PlayerStats stat = playerStats[_owner];
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
		playerStats[_owner] =  stat;
		RPC_SetTeam ();

	}*/


	public void RPC_SetTeam(){
		
		for(int i = 1; i<playerStats.Count; i++) {
			int _id = i;
			int team = (i+1) % 2;
			//TODO: Maybe set the teams
			//playerStats [_id].team = team;
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
			if (player.isLocalPlayer) {
				localPlayer = player;
			}
		}

		currentTime = currentTime - 1;
		if (currentTime <= 0) {
			EndGame ();
		}
		//TODO: Do through syncvar in damage class
		//if (Metwork.peerType != MetworkPeerType.Disconnected) {
			//UpdateShipHealths ();
		//}

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
		//TODO: These can be handled by a syncvar of the damage
		/*if (carrierADmg.netObj.isLocal) {
			//netView.RPC ("RPC_UpdateShipHealths", MRPCMode.OthersBuffered, new object[]{ 0, carrierADmg.currentHealth});
		}
		if (carrierBDmg.netObj.isLocal) {
			//netView.RPC("RPC_UpdateShipHealths", MRPCMode.OthersBuffered, new object[]{ 1, carrierBDmg.currentHealth});
		}*/
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

		foreach (PlayerStats player in playerStats) {
			if (player.name == "") {
				continue;
			}
			if (player.team == winningTeam) {
				player.score += 50;
				winners.Add (player);
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
			} else {
				losers.Add (player);
			}

		}
		winnerNamesText.text = "";
		winnerKillsText.text = "";
		loserNamesText.text = "";
		loserKillsText.text = "";

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

		if (playerStats[localPlayer.playerID].team == winningTeam)
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
		int previousKills = int.Parse( data [2]);
		int previousDeaths = int.Parse( data [3]);
		data [1] =( previousScore+playerStats [localPlayer.playerID].score).ToString();
		data [2] =( previousKills+playerStats [localPlayer.playerID].kills).ToString();
		data [3] =( previousDeaths+playerStats [localPlayer.playerID].deaths).ToString();

		System.IO.File.WriteAllLines (Application.persistentDataPath+"/Player Data.txt", Util.ThiccWatermelon(data));
		
	}
	public void RPC_EndGame(){

	}

	public static GameObject GetGameObjectFromNetID(int _netID){
		//CHECK Function never really used
		Metwork_Object[] netObjects = GameObject.FindObjectsOfType<Metwork_Object> ();

		for (int i = 0; i < netObjects.Length; i++) {
			if (netObjects [i].netID == _netID) {
				return netObjects [i].gameObject;
			}
		}
		print ("Failed to find Net Object " + _netID.ToString());
		return GameObject.CreatePrimitive(PrimitiveType.Sphere);
	}

	/*TODO Remove
	public static Com_Controller GetBotFromPlayerID(int _playerID){
		for (int i = 0; i < Instance.bots.Count; i++) {
			if (Instance.bots[i].botID == _playerID) {
				return Instance.bots[i];
			}
		}
		print ("Failed to find Bot " + _playerID.ToString());
		return null;
	}*/

	//safer than get gameobject but slower
	public Player_Controller GetPlayerFromNetID(int _netID){
		

		for (int i = 0; i < playerObjects.Count; i++) {
			if (playerObjects [i].playerID == _netID) {
				return playerObjects [i];
			}
		}
		Debug.LogWarning ("Could not find player with id: " + _netID.ToString());
		return null;//GameObject.CreatePrimitive(PrimitiveType.Sphere);
	}

	//[MRPC]
	//public void RPC_ActivatePlayer(int _owner){
//		GetPlayerFromNetID (_owner).gameObject.SetActive (true);
	//}

	public void AddAssist(int playerNum, int assistAmount){
		/*TODO This can be done through a synced hash list
		if (Metwork.peerType != MetworkPeerType.Disconnected) {
			netView.RPC ("RPC_AddAssist", MRPCMode.AllBuffered, new object[] {
				playerNum, assistAmount
			});
		} else {
			RPC_AddAssist (playerNum, assistAmount);
		}*/
		//CHECK Player stats MUST be updated this way, otherwise the 
		//Network will not notice that the list has changed
		PlayerStats playerStat = playerStats [playerNum];
		playerStat.score += assistAmount;
		playerStat.assists++;
		playerStats [playerNum] = playerStat;

	}
	[MRPC]
	public void RPC_AddAssist(int playerNum, int assistAmount){
		
	}
	public void AddKill(int playerNum){
		PlayerStats playerStat = playerStats [playerNum];
		playerStat.kills++;
		playerStat.score += 100;
		playerStats [playerNum] = playerStat;

		RPC_AddKill(playerNum);

	}
	[ClientRpc]
	public void RPC_AddKill(int playerNum){
		if(playerNum == localPlayer.playerID){
			WindowsVoice.Speak("Target Down");
		}
	}
	public void AddDeath(int playerNum){
		//TODO: Send Cmd to allow players to kill themselves
		PlayerStats playerStat = playerStats [playerNum];
		playerStat.deaths++;
		playerStats[playerNum] = playerStat;
		if (playerStats [playerNum].team == 1) {
			scoreA++;
		} else {
			scoreB++;
		}

	}

	//TODO: Remove
	[MRPC]
	public void RPC_AddDeath(int playerNum){

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
		
		
		if(isServer){
			FindObjectOfType<PHPMasterServerConnect>().UpdateHost (newScene);
		}

		
		SceneManager.LoadScene("TransistionScene");
		//TODO: Assign variable gameplayUI of gamecontroller in LHX ULTIMA
		
	

	}
	public int GetLocalTeam()
	{

		int _localPlayerID = GetLocalPlayer();
		//At the start of the match there may be no local player, so we can return the team of the 
		//First bot instead
		if(_localPlayerID == -1)
			return playerStats[0].team;
		else
			return playerStats[localPlayer.playerID].team;
	}
	void Update()
	{
		if (Input.GetKeyDown("0") && false)
		{
			GameClipMode = !GameClipMode;
			if (GameClipMode)
			{
				localPlayer.mainCamObj.SetActive(false);

				foreach (TextMesh textMesh in FindObjectsOfType<TextMesh>())
				{
					textMesh.text = "";
				}
				GameClipCamera = Instantiate(GameClipCameraPrefab, localPlayer.transform.position, localPlayer.transform.rotation).GetComponent<Camera>();
			}
			else
			{
				localPlayer.mainCamObj.SetActive(true);
				localPlayer.mainCamObj.transform.position = GameClipCamera.transform.position;
				localPlayer.helmet.SetActive(false);
				Destroy(GameClipCamera.gameObject);
			}
		}
		if (GameClipMode)
		{
			if (GameClipTarget == null)
			{
				GameClipTarget = localPlayer.transform;
			}
			localPlayer.helmet.SetActive(true);
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
		return Game_Controller.instance.playerStats[_viewID].team;
	}
	public static int GetTeam(Player_Controller _player){
		//TODO _player.netObj.netID
		return Game_Controller.instance.playerStats[_player.playerID].team;
	}

	public void OnPlayerSync(SyncList<PlayerStats>.Operation op, int index, PlayerStats oldItem, PlayerStats newItem){
		playerStats.CopyTo(debugPlayerStats, 0);
	}

	//public static int GetTeam(GameObject _player){
	//	return Game_Controller.instance.playerStats[_player.GetComponent<Metwork_Object>().netID].team;
	//}




}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public class Game_Controller : NetworkBehaviour {

	[System.Serializable]
	public class PlayerStat{
		public string name = "Default Name";
		public int kills = 0;
		public int deaths = 0;
		public int assists = 0;
		public int score = 0;
		public int team = -1;
		public bool isBot = true;
		public bool isFilled = false;
		public int connectionID = -1;
	}

	[System.Serializable]
	public struct GameType{
		public const string Destruction = "Destruction";
		public const string TeamDeathmatch = "Team Deathmatch";
		public const string CTF = "Capture The Flag";
		public const string ControlPoint = "Control Point";
		public const string Meltdown = "Meltdown";
		public const string Soccer = "AstroBall";

	}
	
	public enum GameControllerState{
		NotStarted,
		MatchStarting,
		MatchRunning,
		MatchEnding

	}

	private Dictionary<GameControllerState, IMatchState> m_StateClasses = new Dictionary<GameControllerState, IMatchState>(){
		{GameControllerState.MatchStarting, new StartMatchState()},
		{GameControllerState.MatchRunning, new RunningMatchState()},
		{GameControllerState.MatchEnding, new EndMatchState()}
	};
	private IMatchState m_CurrentState = null;
	
	private static Game_Controller instance;
	public static Game_Controller Instance{
		get{
			if(instance == null){
				instance = FindObjectOfType<Game_Controller>(true);
			}
			return instance;
		}
	}

	[SerializeField]
	[SyncVar (hook = nameof(SetMatchState))]
	public GameControllerState m_State = GameControllerState.NotStarted;
	public Weapons_Catalog weaponsCatalog;
	public int maxPlayers = 32;
	public GameObject botPrefab;

	public List<Player_Controller> playerObjects = new List<Player_Controller> ();
	[HideInInspector]
	public List<Com_Controller> bots = new List<Com_Controller>();

	[SerializeField]
	public readonly SyncList<PlayerStat> playerStats = new SyncList<PlayerStat>();
	public PlayerStat[] debugPlayerStats;


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
	public int localTeam = 0;
	[Space(5f)]
	[Header("UI Variables")]
	public int matchLength;
	[SyncVar]
	public double currentTime;

	public float fps;
	[SyncVar]
	public int scoreA;
	[SyncVar]
	public int scoreB;
	[Header( "UI Objects")]
	public Text UI_timeText;
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
		

		playerStats.Callback += OnPlayerSync;

		//netView = this.GetComponent<MetworkView>();
		//Add the bots (we're going to ignore this for now)
		//This part should only be done on the server, but since is server only functions
		//on spawned objects, I don't really know what to do
		//TODO
		
		for(int i = 0; i<maxPlayers; i++){
			PlayerStat _player = new PlayerStat();
			_player.name = "Player " + i.ToString();
			_player.isBot = true;
			//TODO: Properly assign the teams
			_player.team = i%2;
			playerStats.Add(_player);
		}

		//Force a copy of the debug array
		OnPlayerSync(SyncList<PlayerStat>.Operation.OP_ADD, 0, new PlayerStat(), new PlayerStat());

		//CHECK
		GetLocalPlayer();

		Physics.autoSimulation = false;
		//eventSystem.SetActive (false);
		RPC_SetTeam();


		if (SceneManager.GetActiveScene().name != "LobbyScene")
		{
			//TODO This should only runn on the server
			InvokeRepeating("UpdateHost", 130f, 10f);
		}

		try
		{
			string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
			this.matchLength = int.Parse(matchSettings[0]);
			this.gameMode = matchSettings[1];
		}
		catch
		{
			Debug.LogError("Failed to read data from Match Settings.txt");

			string[] matchSettings = Profile.RestoreMatchFile();
			this.matchLength = int.Parse(matchSettings[0]);
			this.gameMode = matchSettings[1];
		}
		//currentTime = matchLength;

		foreach(Spawn_Point spawn in FindObjectsOfType<Spawn_Point>()){
			spawn.DeactivateSpawn();
		}

		foreach(GameItem item in FindObjectsOfType<GameItem>()){
			item.Initialize(gameMode);
		}

		switch (gameMode){
			case GameType.TeamDeathmatch:
				break;
			case GameType.ControlPoint:
				break;
			case GameType.CTF:
				flagA.gameObject.SetActive(true);
				flagA.StartGame();
				flagB.gameObject.SetActive(true);
				flagB.StartGame();
				print("LEt's PlAy");
				break;
		
			case GameType.Meltdown:
				radiationField.SetActive(true);
				radiationField.GetComponent<Radiation>().carrier = shipOneTransform.gameObject;
				
				shipTwoTransform.gameObject.SetActive(false);
				shipTwoTransform = shipOneTransform;
				break;

			case GameType.Soccer:
				if (soccerField != null)
				{
					soccerField.SetActive(true);
				}
				break;
			default:
				Debug.LogWarning("Gamemode: " + gameMode + " not found");
				break;
		}

		Invoke("PhysicsUpdate", 1f);

		if (CustomNetworkManager.IsServerMachine()){
			print("Starting State Machine");
			//StartCoroutine(RunStateMachine());
			ChangeMatchState(GameControllerState.MatchStarting);
		}

	}
	void Start(){
		if (CustomNetworkManager.IsServerMachine())
		{
			for(int i = 0; i<maxPlayers; i++){
				//TODO: Assign these to spawn points
				GameObject _bot = Instantiate(botPrefab, Vector3.zero, Quaternion.identity);
				_bot.name = "Bot " + i;
				_bot.GetComponent<Com_Controller>().playerID = i;
				bots.Add(_bot.GetComponent<Com_Controller>());
				

				if(!playerStats[i].isBot){
					_bot.gameObject.SetActive(false);
            		_bot.GetComponent<Com_Controller>().StopAllCoroutines();
				}else{
					PlayerStat _player = playerStats[i];
					_player.name = "Bot " + i.ToString();
					playerStats[i] = _player;
				}
				NetworkServer.Spawn(_bot);
				
			}
		}
	}

	// ////////////////////////////////////////////////
	// State Machine stuff
	// ///////////////////////////////////////////////

	public void ChangeMatchState(GameControllerState newState){
		if (m_CurrentState != null){
			m_CurrentState.OnExit(this);
		}
		m_CurrentState = m_StateClasses[newState];
		if (isServer){
			m_State = newState;
		}

		m_CurrentState.OnEnter(this);
	}
	
	public void SetMatchState(GameControllerState oldState, GameControllerState newState){
		if(!isServer){
			//Loop through all the states so they all get run
			var allStates = Enum.GetValues(typeof(GameControllerState));

			if (oldState < newState){
				for(int i = 1 + (int)oldState; i <= (int)newState; i++){
					this.ChangeMatchState((GameControllerState) i);
				}
			}
			else{
				for(int i = 1 + (int)oldState; i < allStates.Length; i++){
					this.ChangeMatchState((GameControllerState) i);
				}
				for(int i = 1; i <= (int)newState; i++){
					this.ChangeMatchState((GameControllerState) i);
				}
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
			PlayerStat stat = playerStats[i];
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
		PlayerStat _stat = playerStats[_index];
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
	

	public int AssignPlayerID(int connectionID){
		int _playerID = -1;
		for(int i = 0; i<playerStats.Count; i++){
			if(playerStats[i].isBot == true){
				PlayerStat playerStat = playerStats[i]; 
				playerStat.connectionID = connectionID;
				playerStat.isBot = false;
				//Wipe data
				playerStat.kills = 0;
				playerStat.deaths = 0;
				playerStat.assists = 0;
				playerStat.score = 0;
				
				
				playerStats[i] = playerStat;

				_playerID = i;
				break;
			}
		}
		if(_playerID == -1)
			Debug.LogError("Invalid player ID: " + _playerID);
		return _playerID;
	}
	public int RemovePlayerID(int connectionID){
		int _playerID = -1;
		for(int i = 0; i<playerStats.Count; i++){
			if(playerStats[i].connectionID == connectionID){
				PlayerStat playerStat = playerStats[i]; 
				playerStat.isBot = true;
				playerStat.connectionID = -1;
				playerStats[i] = playerStat;

				_playerID = i;
				break;
			}
		}
		if(_playerID == -1)
			Debug.LogError("Invalid player ID: " + _playerID);
		return _playerID;
	}

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
		PlayerStat playerStat = playerStats [playerNum];
		playerStat.score += assistAmount;
		playerStat.assists++;
		playerStats [playerNum] = playerStat;

	}
	[MRPC]
	public void RPC_AddAssist(int playerNum, int assistAmount){
		
	}
	public void AddKill(int playerNum){
		PlayerStat playerStat = playerStats [playerNum];
		playerStat.kills++;
		playerStat.score += 100;
		playerStats [playerNum] = playerStat;

		RPC_AddKill(playerNum);

	}
	[ClientRpc]
	public void RPC_AddKill(int playerNum){
		if(localPlayer!= null && playerNum == localPlayer.playerID){
			WindowsVoice.Speak("Target Down");
		}
	}
	public void AddDeath(int playerNum){
		//TODO: Send Cmd to allow players to kill themselves
		PlayerStat playerStat = playerStats [playerNum];
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

	
	
	
	void Update()
	{
		if (m_CurrentState != null){
			m_CurrentState.OnUpdate(this);
		}

	}
	public static int GetTeam(int _viewID){
		return Game_Controller.instance.playerStats[_viewID].team;
	}
	public static int GetTeam(Player _player){
		//TODO _player.netObj.netID
		return Game_Controller.instance.playerStats[_player.playerID].team;
	}

	public void OnPlayerSync(SyncList<PlayerStat>.Operation op, int index, PlayerStat oldItem, PlayerStat newItem){
		if(debugPlayerStats == null || debugPlayerStats.Length != playerStats.Count)
			debugPlayerStats = new PlayerStat[playerStats.Count];
		playerStats.CopyTo(debugPlayerStats, 0);
	}

    public void ServerAddPlayer(NetworkConnection conn)
    {
		return;
    }

    //public static int GetTeam(GameObject _player){
    //	return Game_Controller.instance.playerStats[_player.GetComponent<Metwork_Object>().netID].team;
    //}




}
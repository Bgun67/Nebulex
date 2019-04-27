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
	}

	[System.Serializable]
	public struct GameType{
		public const string Destruction = "Destruction";
		public const string TeamDeathmatch = "Team Deathmatch";
		public const string CTF = "Capture The Flag";
		public const string Meltdown = "Meltdown";
		public const string Soccer = "AstroBall";

	}

	public static Game_Controller GetInstance;

	public List<Player> players = new List<Player>();
	public List<GameObject> playerObjects = new List<GameObject> ();



	public PlayerStats[] statsArray = new PlayerStats[32];


	public List<Transform> playerSpawnTransforms = new List<Transform> ();
	public int currentTeamNum;
	public string gameMode;
	public Text lblWinners;
	public bool finished = false;
	public Damage carrierADmg;
	public Damage carrierBDmg;
	public int winningTeam;

	public GameObject sceneCam;
	public Transform shipOneTransform;
	public Transform shipTwoTransform;
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
		GetInstance = GameObject.FindObjectOfType<Game_Controller>();
		
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
			foreach (GameObject position in GameObject.FindGameObjectsWithTag("Spawn Point 1"))
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


			}
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
	public void GetLocalPlayer(){
		foreach (Player_Controller player in FindObjectsOfType<Player_Controller>()) {
			if (player.GetComponent<Metwork_Object>()!=null && player.GetComponent<Metwork_Object> ().isLocal) {
				localPlayer = player.gameObject;
			}
		}
	}
	void PhysicsUpdate(){
		Physics.autoSimulation = true;
		
	}

	[MRPC]
	public void RPC_AddPlayerStat(string name, int _owner){

		PlayerStats stat = statsArray[_owner];
		stat.name = name;
		stat.kills = 0;
		stat.deaths = 0;
		stat.score = 0;

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
		Vector3 shipDisplacement = (shipTwoTransform.position-shipOneTransform.position  )/2f;
		if (Vector3.SqrMagnitude(shipDisplacement) == 0) {
			sceneCam.transform.position = shipOneTransform.position + new Vector3 (0f, 700f, 0);
		} else {
			sceneCam.transform.position = shipOneTransform.position + shipDisplacement + new Vector3 (0f, Vector3.Magnitude (shipDisplacement * (4f / 3f) * 1.5f), 0f);
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
			if (scoreA >scoreB) {
				winningTeam = 0;
			}
			else {
				winningTeam = 1;
			}
			break;
		case "Team Deathmatch":
			if (scoreA > scoreB) {
				winningTeam = 0;
			} else {
				winningTeam = 1;
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
			} else {
				winningTeam = 1;
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


		foreach (PlayerStats player in winners) {
			winnerNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16)) + "\r\n";
			winnerKillsText.text += player.kills.ToString() + "\r\n";
		}
		foreach (PlayerStats player in losers) {
			loserNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16))  + "\r\n";
			loserKillsText.text += player.kills.ToString() + "\r\n";

		}
		endOfGameUI.SetActive (true);
		gameplayUI.SetActive (false);
		if (statsArray[localPlayer.GetComponent<Metwork_Object>().netID].team == winningTeam)
		{
			winningTeamText.text = "Your Team Wins!";
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
		eventSystem.SetActive(true);
		SavePlayerScore();


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
		PlayerStats playerStat = statsArray [playerNum];
		playerStat.kills++;
		playerStat.score += 100;
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
		PlayerStats playerStat = statsArray [playerNum];
		playerStat.deaths++;
		if (playerStat.team == 0) {
			scoreA++;
		} else {
			scoreB++;
		}
	}

	public void RestartGame()
	{
		print("Loading Spawn Scene");
		FindObjectOfType<Network_Manager>().minStartingPlayers = 8;
		SceneManager.LoadScene("LobbyScene");
	}
	public int GetLocalTeam()
	{
		GetLocalPlayer();
		return statsArray[localPlayer.GetComponent<Metwork_Object>().netID].team;
	}
	void Update()
	{
		if (Input.GetKeyDown("0"))
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




}
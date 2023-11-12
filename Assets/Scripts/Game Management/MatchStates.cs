using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public interface IMatchState{
    
    public void OnEnter(Game_Controller gc);
    public void OnUpdate(Game_Controller gc);
    public void OnExit(Game_Controller gc);

}

public class StartMatchState : IMatchState{
    float START_GAME_WAIT = 15f;
    private double m_EnterTime{
        get; set;
    }
    public void OnEnter(Game_Controller gc){
        m_EnterTime = NetworkTime.time;

		//Unlock and show cursor
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		//Show start game ui
		UI_Manager.Instance.m_StartGameUI.gameObject.SetActive(true);
		
	}

    public void OnUpdate(Game_Controller gc){

        if (NetworkTime.time - m_EnterTime > START_GAME_WAIT){
			if (gc.isServer)
            	gc.ChangeMatchState(Game_Controller.GameControllerState.MatchRunning);
        }
        else{
			double timeRemaining = Math.Clamp(START_GAME_WAIT - (NetworkTime.time - m_EnterTime), 0, START_GAME_WAIT);
            //Update the time on the start game 
			UI_Manager.Instance.m_StartGameUI.UpdateTime(timeRemaining);
        }
    }

    public void OnExit(Game_Controller gc){
		UI_Manager.Instance.m_StartGameUI.UpdateTime(0.0);
    }
}

public class RunningMatchState : IMatchState{
    private double m_EnterTime{
        get; set;
    }
    public void OnEnter(Game_Controller gc){
        m_EnterTime = NetworkTime.time;
		
		if (gc.isServer)
        	gc.currentTime = gc.matchLength;

		//Show the spawn scene
        UI_Manager.Instance.m_StartGameUI.MatchReady();
        UI_Manager.Instance.m_SpawnUI.SetActive(true);
	}
    public void OnUpdate(Game_Controller gc){
        //unlock cursor
		if (Input.GetKey(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		

		foreach (Player_Controller player in GameObject.FindObjectsOfType<Player_Controller>()) {
			if (player.isLocalPlayer) {
				gc.localPlayer = player;
			}
		}
		gc.fps = 1f / Time.deltaTime;
		UpdateUI(gc);

		if (!gc.isServer)
			return;
		
		gc.currentTime = gc.matchLength - (NetworkTime.time - m_EnterTime);
		if (gc.currentTime <= 0) {
			gc.ChangeMatchState(Game_Controller.GameControllerState.MatchEnding);
		}

		switch (gc.gameMode) {
		case "Destruction":
			gc.scoreA = (int)gc.carrierADmg.currentHealth;
			gc.scoreB = (int)gc.carrierBDmg.currentHealth;
			if (gc.scoreA <= 0 || gc.scoreB <= 0) {
				gc.ChangeMatchState(Game_Controller.GameControllerState.MatchEnding);
			}
			break;
		case "Team Deathmatch":
			if (gc.scoreA >= 100 || gc.scoreB >= 100) {
				gc.ChangeMatchState(Game_Controller.GameControllerState.MatchEnding);
			}
			break;
		case "Capture The Flag":

			break;
		case Game_Controller.GameType.ControlPoint:
			ControlPoint[] cps = Game_Controller.FindObjectsOfType<ControlPoint>();
			gc.scoreA = 0;
			gc.scoreB = 0;
			foreach(ControlPoint cp in cps){
				if (cp.m_Status == GameItem.ControlStatus.TeamA){
					gc.scoreA += 1;
				}
				else if (cp.m_Status == GameItem.ControlStatus.TeamB){
					gc.scoreB += 1;
				}
			}
			if (gc.scoreA >= cps.Length || gc.scoreB >= cps.Length){
				gc.ChangeMatchState(Game_Controller.GameControllerState.MatchEnding);
			}
			break;
		case "Meltdown":
			gc.scoreA = (int)gc.carrierADmg.currentHealth;
			gc.scoreB = (int)gc.carrierADmg.originalHealth-(int)gc.carrierADmg.currentHealth;

			if (gc.scoreA <= 0) {
				gc.ChangeMatchState(Game_Controller.GameControllerState.MatchEnding);
			}
			break;
		case "Soccer":

			break;
		default:
			break;
		}
    }

    public void OnExit(Game_Controller gc){

    }

	private void UpdateUI(Game_Controller gc){

		//Update the scores of the teams, the local team being on the
		//left side and the opposing team being on the right
		//TODO: Move this entire function to the UI manager
		#if !UNITY_SERVER
		if (gc.localPlayer != null && gc.localTeam == 0) {
			UI_Manager.Instance.UI_HomeScoreText.text = gc.scoreA.ToString();
			UI_Manager.Instance.UI_AwayScoreText.text = gc.scoreB.ToString();
			UI_Manager.Instance.UI_HomeColour.color = new Color(0,1f,0);
			UI_Manager.Instance.UI_AwayColour.color = new Color(1f,0f,0);
		} else if (gc.localTeam == 1){
			UI_Manager.Instance.UI_HomeScoreText.text = gc.scoreB.ToString();
			UI_Manager.Instance.UI_AwayScoreText.text = gc.scoreA.ToString();
			UI_Manager.Instance.UI_AwayColour.color = new Color(0,1f,0);
			UI_Manager.Instance.UI_HomeColour.color = new Color(1f,0f,0);
		}
		else {
			UI_Manager.Instance.UI_HomeScoreText.text = gc.scoreB.ToString() + "*";
			UI_Manager.Instance.UI_AwayScoreText.text = gc.scoreA.ToString();
			UI_Manager.Instance.UI_AwayColour.color = new Color(0,1f,0);
			UI_Manager.Instance.UI_HomeColour.color = new Color(1f,0f,0);
		}
		#endif

		TimeSpan timeSpan = TimeSpan.FromSeconds(gc.currentTime);

		//Update the time
		gc.UI_timeText.text = timeSpan.ToString("mm':'ss");
	}
}

public class EndMatchState : IMatchState{
    private double m_EnterTime{
        get; set;
    }
    public void OnEnter(Game_Controller gc){
        m_EnterTime = NetworkTime.time;
		
		switch (gc.gameMode) {
		case "Destruction":
			gc.scoreA = (int)gc.carrierADmg.currentHealth;
			gc.scoreB = (int)gc.carrierBDmg.currentHealth;
			if (gc.scoreA > gc.scoreB) {
				gc.winningTeam = 0;
			}
			else if (gc.scoreA < gc.scoreB) {
				gc.winningTeam = 1;
			}
			 else {
				gc.winningTeam = -1;
			}
			break;
		case "Team Deathmatch":
			if (gc.scoreA > gc.scoreB) {
				gc.winningTeam = 0;
			}
			else if (gc.scoreA < gc.scoreB) {
				gc.winningTeam = 1;
			}
			 else {
				gc.winningTeam = -1;
			}
			break;
		case "Capture The Flag":
			//Check if the overrideWinner is the default
			//If it isn't that means one have the flags have been captured
			if (gc.overrideWinner != 1000) {
				gc.winningTeam = gc.overrideWinner;
			}
			//The game has timed out
			else if (gc.scoreA > gc.scoreB) {
				gc.winningTeam = 0;
			}
			else if (gc.scoreA < gc.scoreB) {
				gc.winningTeam = 1;
			}
			 else {
				gc.winningTeam = -1;
			}
			break;
		case "Meltdown":
			gc.scoreA = (int)gc.carrierADmg.currentHealth;
			gc.scoreB = (int)gc.carrierADmg.originalHealth-(int)gc.carrierADmg.currentHealth;
			if (gc.scoreA >0) {
				gc.winningTeam = 0;
			}
			else {
				gc.winningTeam = 1;
			}
			break;
		default:
			break;

		}

		// TODO: Tease out which of this stuff is server and which is client
		List<Game_Controller.PlayerStat> winners = new List<Game_Controller.PlayerStat> ();
		List<Game_Controller.PlayerStat> losers = new List<Game_Controller.PlayerStat> ();

		foreach (Game_Controller.PlayerStat player in gc.playerStats) {
			if (player.name == "") {
				continue;
			}
			if (player.team == gc.winningTeam) {
				if (gc.isServer)
					player.score += 50;
				winners.Add (player);
			}
			//Tie
			else if(gc.winningTeam == -1){
				//TODO: Why are player scores added here and above??
				if (gc.isServer)
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

        //TODO: Move these to the UI manager
		gc.winnerNamesText.text = "";
		gc.winnerKillsText.text = "";
		gc.loserNamesText.text = "";
		gc.loserKillsText.text = "";

		winners.Sort((x,y) => y.score.CompareTo(x.score));
		losers.Sort((x,y) => y.score.CompareTo(x.score));

		foreach (Game_Controller.PlayerStat player in winners) {
			gc.winnerNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16)) + "\r\n";
			gc.winnerKillsText.text += player.kills.ToString() + "\t"  + player.deaths.ToString() + "\t" + player.assists.ToString() + "\r\n";
		}
		foreach (Game_Controller.PlayerStat player in losers) {
			gc.loserNamesText.text += player.name.Substring(0,Mathf.Min(player.name.Length,16))  + "\r\n";
			gc.loserKillsText.text += player.kills.ToString() + "\t"  + player.deaths.ToString() + "\t" + player.assists.ToString() + "\r\n";

		}
		//Hide the pause menu, all the players should be destroyed anyway so everything should be peachy
		UI_Manager.Instance.pauseMenu.gameObject.SetActive(false);
		UI_Manager.Instance.m_SpawnUI.SetActive(false);
		gc.endOfGameUI.SetActive (true);
		gc.gameplayUI.SetActive (false);

		if (gc.playerStats[gc.localPlayer.playerID].team == gc.winningTeam)
		{
			gc.winningTeamText.text = "Your Team Wins!";
		}
		else if(gc.winningTeam == -1){
			gc.winningTeamText.text = "Tie Game!";
		}
		else
		{
			gc.winningTeamText.text = "Your Team Lost!";
		}
		if (gc.scoreA < 0) {
			gc.scoreA = 0;
		}
		if (gc.scoreB < 0) {
			gc.scoreB = 0;
		}
		gc.endScoreText.text = gc.scoreA + ":" + gc.scoreB;
		TimeSpan timeSpan = TimeSpan.FromSeconds(Math.Max(gc.currentTime, 0));
		
		//TODO: These should be different text fields???
		gc.endTimeText.text = "Remaining Time: " + timeSpan.ToString("mm':'ss");
		gc.endTimeText.text = "Next match in 20 sec";

		gc.eventSystem.SetActive(true);
		//TODO: This should be controlled by the server (ideally a MySQL server)
		// And read by the client
		gc.SavePlayerScore();

	}
    public void OnUpdate(Game_Controller gc){
		if (!gc.isServer)
			return;

        if (NetworkTime.time - m_EnterTime > 20.0f){
            gc.ChangeMatchState(Game_Controller.GameControllerState.MatchStarting);
        }
    }

    public void OnExit(Game_Controller gc){
		if(!gc.isServer)
			return;
        
		Debug.Log("Loading next scene Scene");

		string newScene = "LobbyScene";
		switch(SceneManager.GetActiveScene().name){
			case "LHX Ultima Base":
				newScene = "Sector 9";
				break;
			/*case "Crater":
				newScene = "Fracture";
				break;
			case "Fracture":
				newScene = "Space";
				break;
			case "Space":
				newScene = "Sector 9";
				break;*/
			case "Sector 9":
				newScene = "LHX Ultima Base";
				break;
			default:
				newScene = "MatchScene";
				break;
		}
		
		string[] matchSettings = Util.LushWatermelon(System.IO.File.ReadAllLines(Application.persistentDataPath + "/Match Settings.txt"));
		//TODO: Update to support all match types
		if(gc.gameMode == "Team Deathmatch" || gc.gameMode == "Capture The Flag"){
			matchSettings[2] = newScene;
			System.IO.File.WriteAllLines (Application.persistentDataPath + "/Match Settings.txt", Util.ThiccWatermelon (matchSettings));
		}
		else{
			newScene = matchSettings[2];
		}
		
		//TODO: Why is this here?
		if(gc.isServer){
			Game_Controller.FindObjectOfType<PHPMasterServerConnect>().UpdateHost (newScene);
			CustomNetworkManager.Instance.ServerChangeScene(newScene);
		}

    }
}

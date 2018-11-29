using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player{
	public int playerNumber;
	public string name;
	public GameObject playerPrefab;
	public int score;

	public Player(string playerName, GameObject newPrefab){
		this.name = playerName;
		this.playerPrefab = newPrefab;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using kcp2k;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Mirror;


public enum PlayerSpawnMethod { Random, RoundRobin }
public enum NetworkManagerMode { Offline, ServerOnly, ClientOnly, Host }


[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkManager")]
[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager")]
public class CustomNetworkManager : Mirror.NetworkManager
{
    private static CustomNetworkManager instance;
	public static CustomNetworkManager Instance{
		get{
			if(instance == null){
				instance = FindObjectOfType<CustomNetworkManager>();
			}
			return instance;
		}
	}
    public delegate void EventHandler();
    //Subscribe to this callback to get notified of the host starting
    public event EventHandler onStartHost;
    //Subscribe to this callback to get notified of the server (no host) starting
    public event EventHandler onStartServer;

    //Returns true if this machine is running the server
    public bool isServerMachine = false;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        print("OnServerAddPlayer");
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        int _playerID = Game_Controller.Instance.AssignPlayerID();
        player.GetComponent<Player_Controller>().playerID = _playerID;
        Game_Controller.Instance.playerObjects.Add(player.GetComponent<Player_Controller>());
        //TODO: Solve for the edge case of a bot being dead as a player spawns
        Game_Controller.Instance.bots[_playerID].gameObject.SetActive(false);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public static bool IsServerMachine(){
        return Instance.isServerMachine;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        isServerMachine = true;
        onStartServer?.Invoke();
        //TODO: Make this not publically display the IP of private matches
        FindObjectOfType<PHPMasterServerConnect>().RegisterHost();

    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        onStartHost?.Invoke();

    }
    public override void OnStopServer(){
        base.OnStopServer();
        isServerMachine = false;
        FindObjectOfType<PHPMasterServerConnect> ().UnregisterHost ();
    }
    public override void OnStopHost(){
        base.OnStopHost();
    }
}


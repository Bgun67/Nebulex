using System;
using System.Collections.Generic;
using System.Linq;
using kcp2k;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Mirror;

#if UNITY_EDITOR
using ParrelSync;
#endif


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

    public static bool m_IsDedicatedServer { 
        get{
            #if UNITY_EDITOR
                // Get the custom argument for this clone project.  
                string customArgument = ClonesManager.GetArgument();
                // Do what ever you need with the argument string.
                return customArgument == "dedicated";    
            #elif UNITY_SERVER
                return true;       
            #else
                return UnityEngine.SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
            #endif
        } 
    }

    public delegate void EventHandler();
    //Subscribe to this callback to get notified of the host starting
    public event EventHandler onStartHost;
    //Subscribe to this callback to get notified of the server (no host) starting
    public event EventHandler onStartServer;

    //Returns true if this machine is running the server
    public bool isServerMachine = false;
    public bool useLan = false;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("OnServerAddPlayer");
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        int _playerID = Game_Controller.Instance.AssignPlayerID(conn.connectionId);
        player.GetComponent<Player_Controller>().playerID = _playerID;
        Game_Controller.Instance.playerObjects.Add(player.GetComponent<Player_Controller>());
        //TODO: Solve for the edge case of a bot being dead as a player spawns
        if(Game_Controller.Instance.bots.Count > _playerID){
            Game_Controller.Instance.bots[_playerID].gameObject.SetActive(false);
            Game_Controller.Instance.bots[_playerID].StopAllCoroutines();
        }
        NetworkServer.AddPlayerForConnection(conn, player);
        Game_Controller.Instance.ServerAddPlayer(conn);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        print("OnServerRemovePlayer");
        
        int _playerID = Game_Controller.Instance.RemovePlayerID(conn.connectionId);
        
        Game_Controller.Instance.playerObjects.RemoveAll(player => player.playerID == _playerID);
        //TODO: Solve for the edge case of a bot being dead as a player spawns
        if(Game_Controller.Instance.bots.Count > _playerID){
            Game_Controller.Instance.bots[_playerID].gameObject.SetActive(true);
        }
        NetworkServer.RemovePlayerForConnection(conn, true);
    }

    public static bool IsServerMachine(){
        return Instance.isServerMachine;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        isServerMachine = true;
        onStartServer?.Invoke();
        //TODO: Test that discovery can both be done locally and globally
        //if(!useLan)
            FindObjectOfType<PHPMasterServerConnect>().RegisterHost();
        //else
            GetComponent<CustomNetworkDiscovery>().AdvertiseServer();

    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        onStartHost?.Invoke();

    }
    public override void OnStopServer(){
        base.OnStopServer();
        isServerMachine = false;
        if(!useLan)
            GetComponent<PHPMasterServerConnect> ().UnregisterHost ();
        else
            GetComponent<CustomNetworkDiscovery>().StopDiscovery();
        
    }
    public override void OnStopHost(){
        base.OnStopHost();
    }
}


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
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        int _playerID = Game_Controller.Instance.AssignPlayerID();
        player.GetComponent<Player_Controller>().playerID = _playerID;
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}


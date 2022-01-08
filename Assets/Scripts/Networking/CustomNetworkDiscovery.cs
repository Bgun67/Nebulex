using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using System.Net;


public class DiscoveryRequest : NetworkMessage
{
    // Add properties for whatever information you want sent by clients
    // in their broadcast messages that servers will consume.
}
public class DiscoveryResponse : NetworkMessage
{ 
    // Add properties for whatever information you want the server to return to
    // clients for them to display or consume for establishing a connection.
    public int connectedPlayers;
	public string gameName;
	public string gameType;
	public int playerLimit;
	public string ip;
	public int port;
	public bool passwordProtected;
	public string comment;
}

[DisallowMultipleComponent]
public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    #region Server

    public long ServerId { get; private set; }

    [Tooltip("Transport to be advertised during discovery")]
    private Transport transport;
    private Dictionary<string,MHostData> hostData = new Dictionary<string, MHostData>(); 

    [HideInInspector]
    public string gameType = "";
    [HideInInspector]
	public string gameName = "Nebulex";
    [HideInInspector]
	public string comment = "Demo";
    [HideInInspector]
    public string ip;
	
    public override void Start()
    {
        ServerId = RandomLong();

        // active transport gets initialized in awake
        // so make sure we set it here in Start()  (after awakes)
        // Or just let the user assign it in the inspector
        if (transport == null)
            transport = Transport.activeTransport;

        var _host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var _ip in _host.AddressList)
        {
            if (_ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ip = _ip.ToString();
            }
        }

        base.Start();
    }

    /// <summary>
    /// Process the request from a client
    /// </summary>
    /// <remarks>
    /// Override if you wish to provide more information to the clients
    /// such as the name of the host player
    /// </remarks>
    /// <param name="request">Request coming from client</param>
    /// <param name="endpoint">Address of the client that sent the request</param>
    /// <returns>The message to be sent back to the client or null</returns>
    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        print("Processing Request");
        // In this case we don't do anything with the request
        // but other discovery implementations might want to use the data
        // in there,  This way the client can ask for
        // specific game mode or something

        try
        {
            // this is an example reply message,  return your own
            // to include whatever is relevant for your game
            DiscoveryResponse _response = new DiscoveryResponse();
            
            _response.gameType = this.gameType;
	        _response.gameName = this.gameName;
	        _response.comment = this.comment;
		    _response.playerLimit = GetComponent<CustomNetworkManager>().maxConnections;
		    _response.connectedPlayers = GetComponent<CustomNetworkManager>().numPlayers;
		    _response.port = GetComponent<kcp2k.KcpTransport>().Port;
		    _response.ip = ip;
            print("Returning Response");
            

            return _response;
            
        }
        catch (System.NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Create a message that will be broadcasted on the network to discover servers
    /// </summary>
    /// <remarks>
    /// Override if you wish to include additional data in the discovery message
    /// such as desired game mode, language, difficulty, etc... </remarks>
    /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
    protected override DiscoveryRequest GetRequest() => new DiscoveryRequest();

    /// <summary>
    /// Process the answer from a server
    /// </summary>
    /// <remarks>
    /// A client receives a reply from a server, this method processes the
    /// reply and raises an event
    /// </remarks>
    /// <param name="response">Response that came from the server</param>
    /// <param name="endpoint">Address of the server that replied</param>
    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        // we received a message from the remote endpoint
        //response.EndPoint = endpoint.Address;

        // although we got a supposedly valid url, we may not be able to resolve
        // the provided host
        // However we know the real ip address of the server because we just
        // received a packet from it,  so use that as host.
        //System.UriBuilder realUri = new System.UriBuilder(response.uri)
        //{
        //    Host = response.EndPoint.Address.ToString()
        //};
        //response.uri = realUri.Uri;

        OnDiscoveredServer(response, endpoint);
    }

    public void OnDiscoveredServer(DiscoveryResponse info, IPEndPoint endpoint)
    {
        if(!hostData.ContainsKey(info.ip)){
			print("Discovered server");
			MHostData data = new MHostData();
            data.ip = info.ip;
            data.comment = info.comment;
            data.connectedPlayers = info.connectedPlayers;
            data.gameName = info.gameName;
            data.gameType = info.gameType;
            data.passwordProtected = info.passwordProtected;
            data.playerLimit = info.playerLimit;
            data.port = info.port;
            
            hostData.Add(info.ip, data);
            
        }
		this.GetComponent<Match_Scene_Manager>().DisplayMatches();
        
    }

    public MHostData[] PollHostList()
	{
		if(hostData == null){
			return new MHostData[]{};
		}
		//Pick out only the appropriate game types
		List<MHostData> _hostList = new List<MHostData>();
		foreach (MHostData _host in hostData.Values){
			if(_host.gameType == gameType && _host.connectedPlayers < _host.playerLimit){
				_hostList.Add(_host);
			}
		}
		return _hostList.ToArray();
	}

    #endregion
}

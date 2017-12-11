using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class MyNetworkManager : NetworkManager
{
    public void StartQuickMatch()
    {
        mQuickMatch = true;
        StartClient();
    }

    //  Server/Host callbacks
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController playerController)
    {
        base.OnServerRemovePlayer(conn, playerController);
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        base.OnServerError(conn, errorCode);
    }

    //  Client Callbacks
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        if (mQuickMatch)
        {
            mQuickMatch = false;
            if (conn.lastError == NetworkError.Timeout)
            {
                StartHost();
            }
        }
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
    }

    public override void OnClientNotReady(NetworkConnection conn)
    {
        base.OnClientNotReady(conn);
    }

    //  Matchmaker Callbacks
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        base.OnMatchList(success, extendedInfo, matchList);
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    private bool mQuickMatch;
}

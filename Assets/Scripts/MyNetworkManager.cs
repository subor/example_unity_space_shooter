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
        Debug.Log("On Server Connect");
        base.OnServerConnect(conn);
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("On Server Disconnect");
        base.OnServerDisconnect(conn);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("On Server Ready");
        base.OnServerReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("On Server Add Player");
        base.OnServerAddPlayer(conn, playerControllerId);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController playerController)
    {
        Debug.Log("On Server Remove Player");
        base.OnServerRemovePlayer(conn, playerController);
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.Log("On Server Error");
        base.OnServerError(conn, errorCode);
    }

    //  Client Callbacks
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("On Client Connect");
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("On Client Disconnect");
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
        Debug.Log("On Client Error");
        base.OnClientError(conn, errorCode);
    }

    public override void OnClientNotReady(NetworkConnection conn)
    {
        Debug.Log("On Client Not Ready");
        base.OnClientNotReady(conn);
    }

    //  Matchmaker Callbacks
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        Debug.Log("On Match Create");
        base.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        Debug.Log("On Match Joined");
        base.OnMatchJoined(success, extendedInfo, matchInfo);
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        Debug.Log("On Match List");
        base.OnMatchList(success, extendedInfo, matchList);
    }

    private bool mQuickMatch;
}

using Ruyi.SDK.Online;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class Lobby : Panel
{
    public override void Open()
    {
        base.Open();

        ShowLoadingCircle();
        CleanProfileData();

        QuickMatch();
    }

    public override void Close()
    {
        base.Close();

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable &&
            RuyiNet.CurrentLobby != null)
        {
            if (RuyiNet.CurrentLobby.MemberCount <= 1)
            {
                RuyiNet.LobbyService.CloseLobby(RuyiNet.ActivePlayerIndex, RuyiNet.CurrentLobbyId, null);
            }
            else
            {
                RuyiNet.LobbyService.LeaveLobby(RuyiNet.ActivePlayerIndex, RuyiNet.CurrentLobbyId, null);
            }
        }
    }

    public void StartGame()
    {
        if (RuyiNet.ActivePlayer.profileId == RuyiNet.CurrentLobby.OwnerProfileId)
        {
            var networkManager = FindObjectOfType<MyNetworkManager>();
            if (networkManager != null)
            {
                networkManager.StartMatchMaker();
                networkManager.matchMaker.CreateMatch("Name", 4, true, "", "", "", 0, 0, OnMatchCreate);
            }
        }
    }
    
    private void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            base.Close();

            var overlay = FindObjectOfType<TutorialInfo>();
            overlay.gameObject.SetActive(false);
            AudioListener.volume = 1f;
            Time.timeScale = 1f;

            var networkManager = FindObjectOfType<MyNetworkManager>();
            networkManager.StartHost(matchInfo);

            Debug.Log(matchInfo.networkId.ToString());
            RuyiNet.LobbyService.StartGame(RuyiNet.ActivePlayerIndex, RuyiNet.CurrentLobbyId,
                matchInfo.networkId.ToString(), null);
        }
    }

    private void Start()
    {
        RuyiNet.LobbyService.OnPlayerJoinLobby -= OnPlayerJoined;
        RuyiNet.LobbyService.OnPlayerLeaveLobby -= OnPlayerLeave;
        RuyiNet.LobbyService.OnLobbyStartGame -= OnStartGame;
        RuyiNet.LobbyService.OnLobbyClosed -= OnClosed;

        RuyiNet.LobbyService.OnPlayerJoinLobby += OnPlayerJoined;
        RuyiNet.LobbyService.OnPlayerLeaveLobby += OnPlayerLeave;
        RuyiNet.LobbyService.OnLobbyStartGame += OnStartGame;
        RuyiNet.LobbyService.OnLobbyClosed += OnClosed;
    }

    private void OnPlayerJoined(string profileId)
    {
        UpdateLobbyInfo(RuyiNet.CurrentLobby);
    }

    private void OnPlayerLeave(string profileId)
    {
        UpdateLobbyInfo(RuyiNet.CurrentLobby);
    }

    private void OnStartGame()
    {
        if (RuyiNet.ActivePlayer.profileId != RuyiNet.CurrentLobby.OwnerProfileId)
        {
            var networkManager = FindObjectOfType<MyNetworkManager>();
            if (networkManager != null)
            {
                networkManager.StartMatchMaker();
                networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnListMatches);
            }
        }
    }

    private void OnListMatches(bool success, string extendedInfo, List<MatchInfoSnapshot> matchInfo)
    {
        Debug.Log("Num Matches found: " + matchInfo.Count);
        var networkManager = FindObjectOfType<MyNetworkManager>();
        if (networkManager != null)
        {
            var networkId = (NetworkID)Enum.Parse(typeof(NetworkID), RuyiNet.CurrentLobby.ConnectionString);
            networkManager.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, OnJoinMatch);
        }
    }

    private void OnJoinMatch(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        Debug.Log("Join Match: " + success + " - " + extendedInfo);
        if (success)
        {
            base.Close();

            var overlay = FindObjectOfType<TutorialInfo>();
            overlay.gameObject.SetActive(false);
            AudioListener.volume = 1f;
            Time.timeScale = 1f;

            var networkManager = FindObjectOfType<MyNetworkManager>();
            networkManager.StartClient(matchInfo);
        }
    }

    private void OnClosed()
    {
        Close();
    }

    private void QuickMatch()
    {
        RuyiNet.LobbyService.FindLobbies(RuyiNet.ActivePlayerIndex, 10, RuyiNetLobbyType.PLAYER, 1, OnQuickMatchFind);
    }

    private void OnQuickMatchFind(RuyiNetLobby[] lobbies)
    {
        if (lobbies != null &&
            lobbies.Length > 0)
        {
            RuyiNet.LobbyService.JoinLobby(RuyiNet.ActivePlayerIndex, lobbies[0].LobbyId, UpdateLobbyInfo);
        }
        else
        {
            RuyiNet.LobbyService.CreateLobby(RuyiNet.ActivePlayerIndex, 4, RuyiNetLobbyType.PLAYER, UpdateLobbyInfo);
        }
    }

    private void UpdateLobbyInfo(RuyiNetLobby lobby)
    {
        if (lobby != null)
        {
            if (lobby.MemberProfileIds != null)
            {
                ShowLoadingCircle();
                RuyiNet.FriendService.GetProfiles(RuyiNet.ActivePlayerIndex, lobby.MemberProfileIds, OnGotProfiles);
            }
            else
            {
                HideLoadingCircle();
            }
        }
    }

    private void OnGotProfiles(RuyiNetGetProfilesResponse response)
    {
        HideLoadingCircle();
        CleanProfileData();
        var profiles = response.data.response;
        var y = START_Y_POSITION;
        foreach (var i in profiles)
        {
            var playerProfile = AddProfileEntry(y, i.profileName, i.profileId, i.pictureUrl, "");

            var button = playerProfile.GetComponentInChildren<Button>();
            var buttonText = button.GetComponentInChildren<Text>();

            if (i.profileId == RuyiNet.ActivePlayer.profileId)
            {
                button.gameObject.SetActive(false);
            }
            else if (i.friend)
            {
                button.onClick.AddListener(() =>
                {
                    RemoveFriend(button, i.profileId);
                });

                buttonText.text = "REMOVE FRIEND";
            }
            else
            {
                button.onClick.AddListener(() =>
                {
                    AddFriend(button, i.profileId);
                });
                
                buttonText.text = "ADD FRIEND";
            }

            y += Y_POSITION_OFFSET;
        }
    }

    private void RemoveFriend(Button button, string profileId)
    {
        button.interactable = false;
        var buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = "REMOVING...";

        RuyiNet.FriendService.RemoveFriend(RuyiNet.ActivePlayerIndex, profileId, (RuyiNetResponse) =>
        {
            UpdateLobbyInfo(RuyiNet.CurrentLobby);
        });
    }

    private void AddFriend(Button button, string profileId)
    {
        button.interactable = false;
        var buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = "ADDING...";

        RuyiNet.FriendService.AddFriend(RuyiNet.ActivePlayerIndex, profileId, (RuyiNetResponse) =>
        {
            buttonText.text = "ADDED";
        });
    }
}

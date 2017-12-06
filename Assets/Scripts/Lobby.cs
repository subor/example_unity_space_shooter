using Ruyi;
using UnityEngine;
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
            RuyiNet.IsRuyiNetAvailable)
        {
            if (RuyiNet.LobbyService.CurrentLobby.MemberCount <= 1)
            {
                RuyiNet.LobbyService.CloseLobby(RuyiNet.ActivePlayerIndex, RuyiNet.LobbyService.CurrentLobby.LobbyId, null);
            }
            else
            {
                RuyiNet.LobbyService.LeaveLobby(RuyiNet.ActivePlayerIndex, RuyiNet.LobbyService.CurrentLobby.LobbyId, null);
            }
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
        Debug.Log("On Player Joined");
        UpdateLobbyInfo(RuyiNet.LobbyService.CurrentLobby);
    }

    private void OnPlayerLeave(string profileId)
    {
        Debug.Log("On Player Leave");
        UpdateLobbyInfo(RuyiNet.LobbyService.CurrentLobby);
    }

    private void OnStartGame()
    {
        Debug.Log("On Start Game");
        //  TODO
    }

    private void OnClosed()
    {
        Debug.Log("On Closed");
        Close();
    }

    private void QuickMatch()
    {
        RuyiNet.LobbyService.FindLobbies(RuyiNet.ActivePlayerIndex, 10, OnQuickMatchFind);
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
            RuyiNet.LobbyService.CreateLobby(RuyiNet.ActivePlayerIndex, UpdateLobbyInfo);
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
            UpdateLobbyInfo(RuyiNet.LobbyService.CurrentLobby);
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

using Ruyi.SDK.BrainCloudApi;
using Ruyi.SDK.Online;
using UnityEngine.UI;

public class PlayerList : Panel
{
    public void GetDefaultLeaderboard()
    {
        gameObject.SetActive(true);
        Title.text = "Leaderboard - Your Position";

        CleanProfileData();
        ShowLoadingCircle();

        RuyiNet.LeaderboardService.GetGlobalLeaderboardView(RuyiNet.ActivePlayerIndex, "Shooter",
            SortOrder.HIGH_TO_LOW, 4, 4, ShowHighScores);
    }

    public void GetTopScoresLeaderboard()
    {
        gameObject.SetActive(true);
        Title.text = "Leaderboard - Highest Scorers";

        CleanProfileData();
        ShowLoadingCircle();

        RuyiNet.LeaderboardService.GetGlobalLeaderboardPage(RuyiNet.ActivePlayerIndex, "Shooter", 
            SortOrder.HIGH_TO_LOW, 0, 8, ShowHighScores);
    }

    public void GetFriendsLeaderboard()
    {
        gameObject.SetActive(true);
        Title.text = "Leaderboard - Friends' Scores";

        CleanProfileData();
        ShowLoadingCircle();

        RuyiNet.LeaderboardService.GetSocialLeaderboard(RuyiNet.ActivePlayerIndex, "Shooter", true,
            (RuyiNetLeaderboardPage leaderboard) =>
            {
                HideLoadingCircle();
                
                if (leaderboard != null)
                {
                    var y = START_Y_POSITION;
                    foreach (var i in leaderboard.Entries)
                    {
                        var playerProfile = AddProfileEntry(y, i.Name, i.PlayerId, i.PictureUrl, i.Score.ToString());

                        var button = playerProfile.GetComponentInChildren<Button>();
                        if (i.Name == "You")
                        {
                            button.gameObject.SetActive(false);
                        }
                        else
                        {
                            button.onClick.AddListener(() =>
                            {
                                RemoveFriendFromLeaderboard(button, i.PlayerId);
                            });

                            var buttonText = button.GetComponentInChildren<Text>();
                            buttonText.text = "REMOVE FRIEND";
                        }

                        y += Y_POSITION_OFFSET;
                    }
                }
            });
    }

    public void ShowFriendsList()
    {
        gameObject.SetActive(true);
        Title.text = "Friends List";

        CleanProfileData();
        ShowLoadingCircle();

        RuyiNet.FriendService.ListFriends(RuyiNet.ActivePlayerIndex, (RuyiNetListFriendsResponse response) =>
        {
            HideLoadingCircle();

            var friends = response.data.response.friends;
            if (friends != null)
            {
                var y = START_Y_POSITION;
                foreach (var i in friends)
                {
                    var playerProfile = AddProfileEntry(y, i.name, i.playerId, i.pictureUrl, "");

                    var button = playerProfile.GetComponentInChildren<Button>();
                    button.onClick.AddListener(() =>
                    {
                        RemoveFriend(button, i.playerId);
                    });

                    var buttonText = button.GetComponentInChildren<Text>();
                    buttonText.text = "REMOVE FRIEND";

                    y += Y_POSITION_OFFSET;
                }
            }
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

    private void RemoveFriend(Button button, string profileId)
    {
        button.interactable = false;
        var buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = "REMOVING...";

        RuyiNet.FriendService.RemoveFriend(RuyiNet.ActivePlayerIndex, profileId, (RuyiNetResponse) =>
        {
            RuyiNet.FriendService.ListFriends(RuyiNet.ActivePlayerIndex, (RuyiNetListFriendsResponse response) =>
            {
                CleanProfileData();

                var y = START_Y_POSITION;
                foreach (var i in response.data.response.friends)
                {
                    var playerProfile = AddProfileEntry(y, i.name, i.playerId, i.pictureUrl, "");

                    var newButton = playerProfile.GetComponentInChildren<Button>();
                    newButton.onClick.AddListener(() =>
                    {
                        RemoveFriend(newButton, i.playerId);
                    });

                    var newButtonText = button.GetComponentInChildren<Text>();
                    newButtonText.text = "REMOVE FRIEND";

                    y += Y_POSITION_OFFSET;
                }
            });
        });
    }

    private void RemoveFriendFromLeaderboard(Button button, string profileId)
    {
        button.interactable = false;
        var buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = "REMOVING...";

        RuyiNet.FriendService.RemoveFriend(RuyiNet.ActivePlayerIndex, profileId, (RuyiNetResponse) =>
        {
            GetFriendsLeaderboard();
        });
    }

    private void ShowHighScores(RuyiNetLeaderboardPage leaderboard)
    {
        HideLoadingCircle();

        if (leaderboard != null)
        {
            var y = START_Y_POSITION;
            foreach (var i in leaderboard.Entries)
            {
                var playerProfile = AddProfileEntry(y, i.Name, i.PlayerId, i.PictureUrl, i.Score.ToString());
                var button = playerProfile.GetComponentInChildren<Button>();
                /*if (i.friend)
                {
                    button.interactable = false;

                    var buttonText = button.GetComponentInChildren<Text>();
                    buttonText.text = "ADDED";
                }
                else */
                if (i.PlayerId == RuyiNet.ActivePlayer.profileId)
                {
                    button.gameObject.SetActive(false);
                }
                else
                {
                    button.onClick.AddListener(() =>
                    {
                        AddFriend(button, i.PlayerId);
                    });

                    var buttonText = button.GetComponentInChildren<Text>();
                    buttonText.text = "ADD FRIEND";
                }

                y += Y_POSITION_OFFSET;
            }
        }
    }

    public Text Title;
}

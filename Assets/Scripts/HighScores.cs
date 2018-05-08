using Ruyi;
using Ruyi.SDK.BrainCloudApi;
using Ruyi.SDK.Online;
using UnityEngine.UI;

public class HighScores : Panel
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

        RuyiNet.LeaderboardService.GetGLobalLeaderboardPage(RuyiNet.ActivePlayerIndex, "Shooter",
            SortOrder.HIGH_TO_LOW, 0, 8, ShowHighScores);
    }

    public void GetFriendsLeaderboard()
    {
        gameObject.SetActive(true);
        Title.text = "Leaderboard - Friends' Scores";

        CleanProfileData();
        ShowLoadingCircle();

        RuyiNet.LeaderboardService.GetSocialLeaderboard(RuyiNet.ActivePlayerIndex, "Shooter", true,
            (RuyiNetSocialLeaderboardResponse response) =>
            {
                HideLoadingCircle();

                var socialLeaderboard = response.data.response.social_leaderboard;
                if (socialLeaderboard != null)
                {
                    var y = START_Y_POSITION;
                    foreach (var i in socialLeaderboard)
                    {                        
                        var playerProfile = AddProfileEntry(y, i.playerName, i.playerId, i.pictureUrl, i.score.ToString());

                        var button = playerProfile.GetComponentInChildren<Button>();
                        if (i.playerName == "You")
                        {
                            button.gameObject.SetActive(false);
                        }
                        else
                        {
                            button.onClick.AddListener(() =>
                            {
                                RemoveFriend(button, i.playerId);
                            });

                            var buttonText = button.GetComponentInChildren<Text>();
                            buttonText.text = "REMOVE FRIEND";
                        }

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
            GetFriendsLeaderboard();
        });
    }

    private void ShowHighScores(RuyiNetLeaderboardResponse response)
    {
        HideLoadingCircle();

        if (response.data.response.leaderboard != null)
        {
            var y = START_Y_POSITION;
            foreach (var i in response.data.response.leaderboard)
            {
                var entry = i.rank.ToString() + ") " + i.name;
                var playerProfile = AddProfileEntry(y, entry, i.playerId, i.pictureUrl, i.score.ToString());
                var button = playerProfile.GetComponentInChildren<Button>();
                if (i.friend)
                {
                    button.interactable = false;

                    var buttonText = button.GetComponentInChildren<Text>();
                    buttonText.text = "ADDED";
                }
                else if (i.playerId == RuyiNet.ActivePlayer.profileId)
                {
                    button.gameObject.SetActive(false);
                }
                else
                {
                    button.onClick.AddListener(() =>
                    {
                        AddFriend(button, i.playerId);
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

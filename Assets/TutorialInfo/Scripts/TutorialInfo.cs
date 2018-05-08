using Ruyi;
using Ruyi.SDK.Online;
using UnityEngine;

// Hi! This script presents the overlay info for our tutorial content, linking you back to the relevant page.
public class TutorialInfo : MonoBehaviour 
{
	// store the GameObject which renders the overlay info
	public GameObject overlay;
    public GameObject loading;

    void Awake()
    {
        ShowLaunchScreen();
    }

    void Start()
    {
        loading.SetActive(true);
        var ruyiNet = FindObjectOfType<RuyiNet>();
        ruyiNet.Initialise(OnRuyiNetInitialised);
    }

	// show overlay info, pausing game time, disabling the audio listener 
	// and enabling the overlay info parent game object
	public void ShowLaunchScreen()
	{
		Time.timeScale = 0f;
        AudioListener.volume = 0f;
	    overlay.SetActive (true);
	}

	// continue to play, by ensuring the preference is set correctly, the overlay is not active, 
	// and that the audio listener is enabled, and time scale is 1 (normal)
	public void StartGame()
	{		
		overlay.SetActive (false);
        AudioListener.volume = 1f;
        Time.timeScale = 1f;

        var networkManager = FindObjectOfType<MyNetworkManager>();
        if (networkManager != null)
        {
            networkManager.StartHost();
        }
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }

    private void OnRuyiNetInitialised()
    {
        var ruyiNet = FindObjectOfType<RuyiNet>();
        ruyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
        {
            if (profile != null)
            {
                Debug.Log("GC: Player " + index);
                //if (ruyiNet.LeaderboardService != null)
                {
                    //    ruyiNet.LeaderboardService.CreateLeaderboard(index, "Shooter", RuyiNetLeaderboardType.HIGH_VALUE, RuyiNetRotationType.MONTHLY, null);
                }

                //if (ruyiNet.MatchmakingService != null)
                //{
                //    ruyiNet.MatchmakingService.EnableMatchmaking(index, null);
                //
                //    if (ruyiNet.NewUser)
                //    {
                //        ruyiNet.MatchmakingService.SetPlayerRating(index, 1000, null);
                //    }
                //}

                if (ruyiNet.CloudService != null)
                {
                    ruyiNet.CloudService.RestoreData(index, OnRestoreData);
                }
                else
                {
                    OnRestoreData(null);
                }
            }
        });
    }

    private void OnRestoreData(RuyiNetResponse response)
    {
        loading.SetActive(false);
    }
}

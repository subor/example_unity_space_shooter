using Ruyi;
using System;
using UnityEngine;

public class RuyiNet : MonoBehaviour
{
    public void Initialise(Action onInitialised)
    {
        if (!string.IsNullOrEmpty(AppId))
        {
            if (IsRuyiNetAvailable)
            {
                Debug.Log("Initialising RuyiNet");
                mSDK.RuyiNetService.Initialise(AppId, AppSecret, onInitialised);
            }
            else
            {
                mOnInitialised = onInitialised;
            }
        }
    }

    public void ForEachPlayer(Action<int, RuyiNetProfile> action)
    {
        if (IsRuyiNetAvailable)
        {
            for (int i = 0; i < 4; ++i)
            {
                action(i, CurrentPlayers[i]);
            }
        }
    }

    private void Awake()
    {
        Console.SetOut(new DebugLogWriter());

        mSDKContext = new RuyiSDKContext()
        {
            endpoint = RuyiSDKContext.Endpoint.Console
        };

        mSDK = RuyiSDK.CreateInstance(mSDKContext);

        Debug.Log("Awake RuyiNet SDK");

        if (mOnInitialised != null)
        {
            if (IsRuyiNetAvailable)
            {
                Debug.Log("Initialising RuyiNet");
                mSDK.RuyiNetService.Initialise(AppId, AppSecret, mOnInitialised);
            }
        }

        //DontDestroyOnLoad(this);
    }

    private void Update()
    {
        mSDK.Update();
    }

    private void OnDestroy()
    {
        if (mSDK != null)
        {
            mSDK.Dispose();
            mSDK = null;
        }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }

    public string AppId;
    public string AppSecret;    //  App secret is unused for now, but will
                                //  become important for security when this is
                                //  implemented properly.

    public RuyiNetProfile[] CurrentPlayers { get { return mSDK.RuyiNetService.CurrentPlayers; } }
    public RuyiNetProfile ActivePlayer { get { return mSDK.RuyiNetService.ActivePlayer; } }
    public int ActivePlayerIndex { get { return mSDK.RuyiNetService.ActivePlayerIndex; } }

    public bool NewUser { get { return mSDK.RuyiNetService.NewUser; } }

    public bool IsRuyiNetAvailable { get { return mSDK != null && mSDK.RuyiNetService != null; } }

    public RuyiNetCloudService CloudService { get { return mSDK.RuyiNetService.CloudService; } }
    public RuyiNetFriendService FriendService { get { return mSDK.RuyiNetService.FriendService; } }
    public RuyiNetLeaderboardService LeaderboardService { get { return mSDK.RuyiNetService.LeaderboardService; } }
    public RuyiNetLobbyService LobbyService { get { return mSDK.RuyiNetService.LobbyService; } }
    public RuyiNetMatchmakingService MatchmakingService { get { return mSDK.RuyiNetService.MatchmakingService; } }
    public RuyiNetPartyService PartyService { get { return mSDK.RuyiNetService.PartyService; } }
    public RuyiNetProfileService ProfileService { get { return mSDK.RuyiNetService.ProfileService; } }
    public RuyiNetUserFileService UserFileService { get { return mSDK.RuyiNetService.UserFileService; } }
    public RuyiNetVideoService VideoService { get { return mSDK.RuyiNetService.VideoService; } }

    private RuyiSDKContext mSDKContext;
    private RuyiSDK mSDK;

    private Action mOnInitialised;
}
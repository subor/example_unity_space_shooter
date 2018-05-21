using Ruyi.Layer0;
using Ruyi.SDK.Online;
using UnityEngine;

// Hi! This script presents the overlay info for our tutorial content, linking you back to the relevant page.
public class TutorialInfo : MonoBehaviour 
{
	// store the GameObject which renders the overlay info
	public GameObject overlay;
    public GameObject loading;

    [SerializeField]
    private UnityEngine.UI.Button[] Buttons;
    private int m_BtnSelected = 0;
    private bool m_IsBtnSelectedChanged = false;
    private bool m_IsEnter = false;
    private bool m_IsReturn = false;

    void Awake()
    {
        ShowLaunchScreen();
    }

    void Start()
    {
        loading.SetActive(true);
        var ruyiNet = FindObjectOfType<RuyiNet>();
        ruyiNet.Initialise(OnRuyiNetInitialised);

        //our input event is listener in sub-thread, in which you can't directly renderer UnityEngine Object (you can't use any UnityEngine-related object in sub-thread)
        //you can use middle values £¨int,float,string,etc,non-UnityEngine-ojbect-type£© to receive the RuyiSDK input value, then listen it in UnityEngine's main thread (Monobehaviour.update(), etc)
        ruyiNet.Subscribe.Subscribe("service/" + ServiceIDs.USER_SERVICE_EXTERNAL.ToString().ToLower());
        ruyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.UserServiceExternal.InputActionEvent>(RuyiInputStateChangeHandler);
    }

    //if use fixedupdate may lead to no response
    void Update()
    {
        RuyiInputListener();      
    }

    private void RuyiInputListener()
    {
        Debug.Log("RuyiInputListener m_IsBtnSelectedChanged:" + m_IsBtnSelectedChanged + " m_IsEnter:" + m_IsEnter);
        if (m_IsBtnSelectedChanged)
        {
            m_IsBtnSelectedChanged = false;

            if (m_BtnSelected >= Buttons.Length) m_BtnSelected = 0;
            else if (m_BtnSelected < 0) m_BtnSelected = Buttons.Length - 1;
            else { }

            for (int i = 0; i < Buttons.Length; ++i)
            {
                if (m_BtnSelected == i)
                {
                    Buttons[i].Select();
                    break;
                }
            }
        }

        if (m_IsEnter)
        {
            m_IsEnter = false;

            Buttons[m_BtnSelected].onClick.Invoke();
        }

        if (m_IsReturn)
        {
            m_IsReturn = false;           
        }
    }

    void RuyiInputStateChangeHandler(string topic, Ruyi.SDK.UserServiceExternal.InputActionEvent msg)
    {
        //TriggerKeys 
        //DeviceType: to identify your input device
        //Key: the key of your input device
        //action: use this value to identify the input button, this value can be configed in the config file
        //NewValue/OldValue:  could be three value:0,1,2.  1 means press Down 2 means release 0 not define yet
        //NewValue is the current key state, if your press down, NewValue will be 1, when you release, NewValue will be 2, OldValue will be 1

        //you can judge the input key by "action" value of Triggers structure. The value of "action" can be modified
        //in config file of the game package. Now I just hard-core in code. We'll try to optimise this part
        //in future release
        //all default system action value: (Layer0/RuyiLocalRoot/Resources/configs/UserSetting)
        //GamePad_LB
        //GamePad_LT
        //GamePad_L3
        //GamePad_RB
        //GamePad_RT
        //GamePad_R3
        //GamePad_UP
        //GamePad_Down
        //GamePad_Left
        //GamePad_Down
        //GamePad_Home
        //GamePad_Back
        //GamePad_Start
        //GamePad_X
        //GamePad_Y
        //GamePad_A
        //GamePad_B
        //GamePad_LJoyX
        //GamePad_LJoyY
        //GamePad_RJoyX
        //GamePad_RJoyY
        for (int i = 0; i < msg.Triggers.Count; ++i)
        {           
            Debug.Log("TutorialInfo RuyiInputStateChangeHandler topic:" + topic + " action:" + msg.Action + " key:" + msg.Triggers[i].Key + " newValue:" + msg.Triggers[i].NewValue);
           
            if (msg.Action.Equals("GamePad_Up") && 1 == msg.Triggers[i].NewValue)
            {
                --m_BtnSelected;
                m_IsBtnSelectedChanged = true;
            }
            if (msg.Action.Equals("GamePad_Down") && 1 == msg.Triggers[i].NewValue)
            {
                ++m_BtnSelected;
                m_IsBtnSelectedChanged = true;
            }
            if (msg.Action.Equals("GamePad_A") && 1 == msg.Triggers[i].NewValue)
            {
                m_IsEnter = true;
            }
            if (msg.Action.Equals("GamePad_B") && 1 == msg.Triggers[i].NewValue)
            {
                m_IsReturn = true;
            }
            /*
            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Up == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonUp == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyY == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                --m_BtnSelected;
                m_IsBtnSelectedChanged = true;
            }
            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Down == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonDown == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyY == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                ++m_BtnSelected;
                m_IsBtnSelectedChanged = true;
            }

            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.A == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonA == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                m_IsEnter = true;
            }

            if ((int)Ruyi.SDK.GlobalInputDefine.Key.S == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonX == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                m_IsReturn = true;
            }*/
        }
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

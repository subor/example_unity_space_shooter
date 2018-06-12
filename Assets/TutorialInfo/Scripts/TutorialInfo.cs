using Ruyi.SDK.Online;
using UnityEngine;

using XInputDotNetPure;

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

    private GamePadState m_GamePadState;

    private bool m_IsLJoystickUp = false;
    private bool m_IsLJoystickDown = false;

    private RuyiNet m_RuyiNet;

    void Awake()
    {
        ShowLaunchScreen();
    }

    void Start()
    {
        //loading.SetActive(true);
        m_RuyiNet = FindObjectOfType<RuyiNet>();
        m_RuyiNet.Initialise(OnRuyiNetInitialised);

        //our input event is listener in sub-thread, in which you can't directly renderer UnityEngine Object (you can't use any UnityEngine-related object in sub-thread)
        //you can use middle values £¨int,float,string,etc,non-UnityEngine-ojbect-type£© to receive the RuyiSDK input value, then listen it in UnityEngine's main thread (Monobehaviour.update(), etc)
        m_RuyiNet.Subscribe.Subscribe("service/inputmgr_int");
        m_RuyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiGamePadInput>(RuyiGamePadInputListener);
        m_RuyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiKeyboardInput>(RuyiKeyboardInputListener);
        m_RuyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiMouseInput>(RuyiMouseInputListener);
    }

    void RuyiGamePadInputListener(string topic, Ruyi.SDK.InputManager.RuyiGamePadInput msg)
    {       
        float leftThumbX = MappingThumbValue(msg.LeftThumbX);
        float leftThumbY = MappingThumbValue(msg.LeftThumbY);   

        // add condition ture == m_IsLJoystickDown to avoid repeated operation 
        // because joystick event will call continuesly while pushing the joystick
        // you need to filter the message according to your logic
        if (leftThumbY <= -0.5f && !m_IsLJoystickDown)
        {
            m_IsLJoystickDown = true;
            ++m_BtnSelected;
            m_IsBtnSelectedChanged = true;
        }
        if (leftThumbY >= 0.5f && !m_IsLJoystickUp)
        {
            m_IsLJoystickUp = true;
            --m_BtnSelected;
            m_IsBtnSelectedChanged = true;
        }
        //release the joystick
        if (Mathf.Abs(leftThumbY) < 0.1f)
        {
            m_IsLJoystickUp = false;
            m_IsLJoystickDown = false;
        }

        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Up == msg.ButtonFlags)
        {
            --m_BtnSelected;
            m_IsBtnSelectedChanged = true;
        }
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Down == msg.ButtonFlags)
        {
            ++m_BtnSelected;
            m_IsBtnSelectedChanged = true;
        }
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_A == msg.ButtonFlags)
        {
            m_IsEnter = true;
        }
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_B == msg.ButtonFlags)
        {
            m_IsReturn = true;
        }

        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Y == msg.ButtonFlags)
        {
            if (null != m_RuyiNet)
            {
                Debug.Log("Viberating Please !!!!!!");

                //public bool SetRuyiControllerStatus(sbyte channel, bool enableR, bool enableG, bool enableB, bool enableMotor1, bool enableMotor2, bool shutdown, sbyte RValue, sbyte GValue, sbyte BValue, sbyte motor1Value, sbyte motor1Time, sbyte motor2Value, sbyte motor2Time);
                //"channel" use 4, when using wire. 0 wireless
                //"bool enableR, bool enableG, bool enableB" controls light on input
                //"sbyte RValue, sbyte GValue, sbyte BValue" the rgb path value of the light of input, there is still some bugs on this function
                //"bool enableMotor1, bool enableMotor2" controls if left/right of the input vibrates
                //"sbyte motor1Value, sbyte motor1Time, sbyte motor2Value, sbyte motor2Time" is the strengh of viberation and duration
                m_RuyiNet.mSDK.InputMgr.SetRuyiControllerStatus(4, false, false, false,
                true, true, false,
                127, 127, 127,
                127, 127,
                127, 127);

            }
        }
    }

    void RuyiKeyboardInputListener(string topic, Ruyi.SDK.InputManager.RuyiKeyboardInput msg)
    {

    }

    void RuyiMouseInputListener(string topic, Ruyi.SDK.InputManager.RuyiMouseInput msg)
    {

    }

    private float MappingThumbValue(float value)
    {
        return value / Mathf.Pow(2f, 15);
    }

    //if use fixedupdate may lead to no response
    void Update()
    {
        RuyiInputListener();      
    }  

    private void RuyiInputListener()
    {        
        //Debug.Log("RuyiInputListener m_IsBtnSelectedChanged:" + m_IsBtnSelectedChanged + " m_IsEnter:" + m_IsEnter);
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
        Debug.Log("TutorialInfo OnRuyiNetInitialised");

        var ruyiNet = FindObjectOfType<RuyiNet>();      
        ruyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
        {
            //Debug.Log("index:" + index + " profile " + profile.profileId);
            if (profile != null)
            {
                //Debug.Log("GC: Player " + index);
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

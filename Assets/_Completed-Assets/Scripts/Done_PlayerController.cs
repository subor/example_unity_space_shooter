using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Done_Boundary 
{
	public float xMin, xMax, zMin, zMax;
}

public class Done_PlayerController : NetworkBehaviour
{
    public float speed;
    public float tilt;
    public Done_Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;

    public int PlayerIndex;

    private void Start()
    {
        var ruyiProfileId = "";
        var ruyiProfileName = "";
        var ruyiNet = FindObjectOfType<RuyiNet>();
        if (ruyiNet != null &&
            ruyiNet.IsRuyiNetAvailable)
        {
            var activePlayer = ruyiNet.ActivePlayer;
            if (activePlayer != null)
            {
                ruyiProfileId = activePlayer.profileId;
                ruyiProfileName = activePlayer.profileName;
            }
            //ruyiNet.Subscribe.Subscribe("service/" + Ruyi.Layer0.ServiceIDs.INPUTMANAGER_INTERNAL.ToString().ToLower());
            ruyiNet.Subscribe.Subscribe("service/inputmgr_int");
            ruyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiGamePadInput>(RuyiGamePadInputListener);
            ruyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiKeyboardInput>(RuyiKeyboardInputListener);
            ruyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.InputManager.RuyiMouseInput>(RuyiMouseInputListener);
        }

        if (isLocalPlayer)
        {
            CmdRegister(ruyiProfileId, ruyiProfileName);
        }
    }
    
    void RuyiGamePadInputListener(string topic, Ruyi.SDK.InputManager.RuyiGamePadInput msg)
    {
        float leftThumbX = MappingThumbValue(msg.LeftThumbX);
        float leftThumbY = MappingThumbValue(msg.LeftThumbY);

        //Debug.Log("ssss leftThumbX:" + leftThumbX + " leftThumbY:" + leftThumbY);

        //joystick release
        if (Mathf.Abs(leftThumbX) <= 0.1f)
        {
            horizontalAxis = 0;
        }
        if (Mathf.Abs(leftThumbY) <= 0.1f)
        {
            verticalAxis = 0;
        }

        //button release
        if (0 == msg.ButtonFlags)
        {
            horizontalAxis = 0;
            verticalAxis = 0;
        }

        //pushing joystick
        if (leftThumbX <= -0.3f)
        {
            horizontalAxis = -1;
        }
        if (leftThumbX >= 0.3f)
        {
            horizontalAxis = 1;
        }
        if (leftThumbY <= -0.3f)
        {
            verticalAxis = -1;
        }
        if (leftThumbY >= 0.3f)
        {
            verticalAxis = 1;
        }
   
        //press button
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Left == msg.ButtonFlags)
        {
            horizontalAxis = -1;
        }
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Right == msg.ButtonFlags)
        {
            horizontalAxis = 1;
        }      
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Up == msg.ButtonFlags)
        {
            verticalAxis = 1;
        }
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_Down == msg.ButtonFlags)
        {
            verticalAxis = -1;
        }

        //fire
        if ((int)Ruyi.SDK.CommonType.RuyiGamePadButtonFlags.GamePad_X == msg.ButtonFlags)
        {
            isFire = true;
        }
    }
    void RuyiKeyboardInputListener(string topic, Ruyi.SDK.InputManager.RuyiKeyboardInput msg)
    {
        //Debug.Log("RuyiKeyboardInputListener topic:" + topic);
    }

    void RuyiMouseInputListener(string topic, Ruyi.SDK.InputManager.RuyiMouseInput msg)
    {
        //Debug.Log("RuyiMouseInputListener topic:" + topic);
    }

    float MappingThumbValue(float value)
    {
        return value / Mathf.Pow(2f, 15);
    }

    [Command]
    private void CmdRegister(string ruyiProfileId, string ruyiProfileName)
    {
        var gameController = FindObjectOfType<Done_GameController>();
        gameController.RegisterPlayer(netId, ruyiProfileId, ruyiProfileName);
    }

    [Command]
    private void CmdSpawnBullet()
    {
        var bullet = Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        var mover = bullet.GetComponent<Done_Mover>();
        mover.PlayerNetId = netId;

        GetComponent<AudioSource>().Play();

        NetworkServer.Spawn(bullet);
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
            Debug.Log("Done_PlayerController topic:" + topic + " RuyiInputStateChangeHandler key:" + msg.Triggers[i].Key + " newValue:" + msg.Triggers[i].NewValue);

            //press Down
            if (msg.Action.Equals("GamePad_Left") && 1 == msg.Triggers[i].NewValue)
            {
                horizontalAxis = -1;
            }
            //release
            if (msg.Action.Equals("GamePad_Left") && 2 == msg.Triggers[i].NewValue)
            {
                horizontalAxis = 0;
            }

            if (msg.Action.Equals("GamePad_Right") && 1 == msg.Triggers[i].NewValue)
            {
                horizontalAxis = 1;
            }
            if (msg.Action.Equals("GamePad_Right") && 2 == msg.Triggers[i].NewValue)
            {
                horizontalAxis = 0;
            }

            if (msg.Action.Equals("GamePad_Up") && 1 == msg.Triggers[i].NewValue)
            {
                verticalAxis = 1;
            }
            if (msg.Action.Equals("GamePad_Up") && 2 == msg.Triggers[i].NewValue)
            {
                verticalAxis = 0;
            }

            if (msg.Action.Equals("GamePad_Down") && 1 == msg.Triggers[i].NewValue)
            {
                verticalAxis = -1;
            }
            if (msg.Action.Equals("GamePad_Down") && 2 == msg.Triggers[i].NewValue)
            {
                verticalAxis = 0;
            }

            //fire
            if (msg.Action.Equals("GamePad_X") && 1 == msg.Triggers[i].NewValue)
            {
                isFire = true;
            }
            /*
            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Left == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonLeft == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyX == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                horizontalAxis = -1;
                isMove = true;
            }

            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Right == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonRight == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyX == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                horizontalAxis = 1;
                isMove = true;
            }

            if (((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonLeft == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyX == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonRight == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyX == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue))
            {
                horizontalAxis = 0;
            }

            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Up == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonUp == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyY == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                verticalAxis = 1;
                isMove = true;
            }
            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Down == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonDown == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyY == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                verticalAxis = -1;
                isMove = true;
            }

            if (((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonDown == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyY == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonUp == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyY == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue))
            {
                verticalAxis = 0;
            }

            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.E == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonB == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                isFire = true;
            }*/
        }
    }

    int horizontalAxis = 0;
    int verticalAxis = 0;
    bool isMove = false;
    bool isFire = false;
    private void RuyiInputValueListener()
    {
        /*
        if (isMove)
        {
            isMove = false;
        } else
        {
            horizontalAxis = 0;
            verticalAxis = 0;
        }*/

        if (isLocalPlayer)
        {
            //Debug.Log("RuyiInputValueListener sssss horizontalAxis:" + horizontalAxis + " verticalAxis:" + verticalAxis);

            Vector3 movement = new Vector3(horizontalAxis, 0.0f, verticalAxis);
            GetComponent<Rigidbody>().velocity = movement * speed;

            GetComponent<Rigidbody>().position = new Vector3
            (
                Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
                0.0f,
                Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
            );

            GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
        }

        if (isFire)
        {
            isFire = false;
            CmdSpawnBullet();
        }
    }

    private void Update ()
	{
        RuyiInputValueListener();

        if (isLocalPlayer)
        {
            if (Input.GetButton("Fire" + PlayerIndex) && 
                Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                CmdSpawnBullet();
            }
        }
	}

	private void FixedUpdate ()
	{
        /*
        if (isLocalPlayer)
        {
            float moveHorizontal = Input.GetAxis("Horizontal" + PlayerIndex);
            float moveVertical = Input.GetAxis("Vertical" + PlayerIndex);

            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            GetComponent<Rigidbody>().velocity = movement * speed;

            GetComponent<Rigidbody>().position = new Vector3
            (
                Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),
                0.0f,
                Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)
            );

            GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
        }*/
    }

    private void OnDestroy()
    {
        var gameController = FindObjectOfType<Done_GameController>();
        if (gameController != null)
        {
            gameController.UnregisterPlayer(this);
        }
    }

    private float nextFire;
}

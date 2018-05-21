using Ruyi.Layer0;
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
            ruyiNet.Subscribe.Subscribe("service/" + ServiceIDs.USER_SERVICE_EXTERNAL.ToString().ToLower());
            ruyiNet.Subscribe.AddMessageHandler<Ruyi.SDK.UserServiceExternal.InputActionEvent>(RuyiInputStateChangeHandler);
        }

        if (isLocalPlayer)
        {
            CmdRegister(ruyiProfileId, ruyiProfileName);
        }
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
                vertiacalAxis = 1;
            }
            if (msg.Action.Equals("GamePad_Up") && 2 == msg.Triggers[i].NewValue)
            {
                vertiacalAxis = 0;
            }

            if (msg.Action.Equals("GamePad_Down") && 1 == msg.Triggers[i].NewValue)
            {
                vertiacalAxis = -1;
            }
            if (msg.Action.Equals("GamePad_Down") && 2 == msg.Triggers[i].NewValue)
            {
                vertiacalAxis = 0;
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
                vertiacalAxis = 1;
                isMove = true;
            }
            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.Down == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonDown == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyY == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                vertiacalAxis = -1;
                isMove = true;
            }

            if (((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonDown == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogLeftJoyY == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonUp == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eAnalogRightJoyY == msg.Triggers[i].Key && 2 == msg.Triggers[i].NewValue))
            {
                vertiacalAxis = 0;
            }

            if ( ((int)Ruyi.SDK.GlobalInputDefine.Key.E == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue)
                || ((int)Ruyi.SDK.GlobalInputDefine.RuyiControllerKey.eButtonB == msg.Triggers[i].Key && 1 == msg.Triggers[i].NewValue))
            {
                isFire = true;
            }*/
        }
    }

    int horizontalAxis = 0;
    int vertiacalAxis = 0;
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
            vertiacalAxis = 0;
        }*/

        if (isLocalPlayer)
        {
            Debug.Log("RuyiInputValueListener sssss horizontalAxis:" + horizontalAxis + " vertiacalAxis:" + vertiacalAxis);

            Vector3 movement = new Vector3(horizontalAxis, 0.0f, vertiacalAxis);
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

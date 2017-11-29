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

    private void Update ()
	{
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
        }
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

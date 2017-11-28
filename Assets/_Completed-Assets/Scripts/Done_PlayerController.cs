using System;
using System.IO;
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

    [SyncVar]
    public int Strength;

    [SyncVar]
    public int Score;

    public void TakeDamage()
    {
        if (isServer)
        {
            Strength = Math.Max(Strength - 1, 0);
        }
    }

    public void AddScore(int score)
    {
        if (isServer)
        {
            Score += score;
        }
    }

    public void ResetScore()
    {
        if (isServer)
        {
            Score = 0;
        }
    }

    public void ApplyProgression(int level)
    {
        if (level > CurrentGame.Strength)
        {
            ++CurrentGame.Strength;
        }
    }

    public void LoadGame(string savePath)
    {
        try
        {
            CurrentGame = SaveLoad.Load<Game>(savePath);
        }
        catch (FileNotFoundException)
        {
            CurrentGame = new Game();
        }

        if (isServer)
        {
            Strength = CurrentGame.Strength;
        }
    }

    [Serializable]
    public class Game
    {
        public int Strength = 1;
    }

    public Game CurrentGame { get; private set; }

    private void Start()
    {
        var gameController = FindObjectOfType<Done_GameController>();
        gameController.RegisterPlayer(this);

        GetComponent<MeshRenderer>().materials[0].color = Color.red;
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

    [Command]
    private void CmdSpawnBullet()
    {
        var bullet = Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        var mover = bullet.GetComponent<Done_Mover>();
        mover.Index = PlayerIndex - 1;
        mover.Strength = Strength;

        GetComponent<AudioSource>().Play();

        NetworkServer.Spawn(bullet);
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
        gameController.UnregisterPlayer(this);
    }

    private float nextFire;
}

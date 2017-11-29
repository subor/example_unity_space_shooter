using UnityEngine;
using UnityEngine.Networking;

public class Done_DestroyByContact : NetworkBehaviour
{
	public GameObject explosion;
	public GameObject playerExplosion;
	public int scoreValue;
	private Done_GameController gameController;
    private int health;

	void Start ()
    {
        gameController = FindObjectOfType<Done_GameController>();
        if (gameController != null)
		{
            health = gameController.Level;
        }
        else
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}

    [Server]
	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Enemy")
		{
			return;
		}

        if (other.tag == "Shot")
        {
            var mover = other.gameObject.GetComponent<Done_Mover>();
            if (health <= gameController.GetPlayerStrength(mover.PlayerNetId))
            {
                gameController.AddScore(mover.PlayerNetId, scoreValue);
                health = 0;
            }
            else
            {
                health--;
            }
        }

        if (explosion != null)
		{
			var boom = Instantiate(explosion, transform.position, transform.rotation);
            NetworkServer.Spawn(boom);
		}

        if (other.tag == "Player")
        {
            var boom = Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
            NetworkServer.Spawn(boom);

            var player = other.gameObject.GetComponent<Done_PlayerController>();
            gameController.DamagePlayer(player.netId);
            health = 0;

            if (gameController.GetPlayerStrength(player.netId) <= 0)
            {
                Destroy(other.gameObject);
                gameController.CheckGameOver();
            }
        }
        else
        {
            Destroy(other.gameObject);
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }		
	}
}
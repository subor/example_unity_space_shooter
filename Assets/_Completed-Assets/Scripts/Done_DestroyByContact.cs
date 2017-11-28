using UnityEngine;

public class Done_DestroyByContact : MonoBehaviour
{
	public GameObject explosion;
	public GameObject playerExplosion;
	public int scoreValue;
	private Done_GameController gameController;
    private int health;

	void Start ()
	{
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		if (gameControllerObject != null)
		{
			gameController = gameControllerObject.GetComponent <Done_GameController>();

            health = gameController.level;
        }

		if (gameController == null)
		{
			Debug.Log ("Cannot find 'GameController' script");
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.tag == "Boundary" || other.tag == "Enemy")
		{
			return;
		}

        if (other.tag == "Shot")
        {
            var mover = other.gameObject.GetComponent<Done_Mover>();
            if (health <= mover.Strength)
            {
                gameController.AddScore(mover.Index, scoreValue);
                health = 0;
            }
            else
            {
                health--;
            }
        }

        if (explosion != null)
		{
			Instantiate(explosion, transform.position, transform.rotation);
		}

        if (other.tag == "Player")
        {
            var player = other.gameObject.GetComponent<Done_PlayerController>();
            Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
            player.TakeDamage();
            gameController.UpdateScore();
            health = 0;

            if (player.Strength <= 0)
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
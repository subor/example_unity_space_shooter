using Ruyi;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Done_GameController : MonoBehaviour
{
    public GameObject[] hazards;
	public Vector3 spawnValues;
	public int hazardCount;
	public float spawnWait;
	public float startWait;
	public float waveWait;
	
	public GUIText[] scoreText;
	public GUIText restartText;
    public GUIText gameOverText;
    public GUIText levelText;

    public int level = 1;
    private List<Done_PlayerController> PlayerControllers = new List<Done_PlayerController>();

    private bool gameOver;
	private bool restart;
    private int kills;

    private const string SAVEGAME_LOCATION = "savedData.gd";
    private readonly Color[] playerColors = {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    public void RegisterPlayer(Done_PlayerController player)
    {
        player.GetComponent<MeshRenderer>().materials[0].color = playerColors[PlayerControllers.Count % 4];
        PlayerControllers.Add(player);
        OnRestoreData(null);
        UpdateScore();
    }

    void Start ()
    {
        level = 1;

        gameOver = false;
        restart = false;
        restartText.text = "";
        gameOverText.text = "";
        kills = 0;

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.Initialise(OnRuyiNetInitialised);
        }

		StartCoroutine (SpawnWaves ());
	}
	
	void Update ()
	{
		if (restart)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}
	
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait + level - 1);
		while (true)
		{
			for (int i = 0; i < hazardCount; i++)
			{
				GameObject hazard = hazards [UnityEngine.Random.Range (0, hazards.Length)];
				Vector3 spawnPosition = new Vector3 (UnityEngine.Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
			yield return new WaitForSeconds (waveWait + (spawnWait * (level - 0)));
			
			if (gameOver)
			{
				restart = true;
				break;
			}
		}
	}
	
	public void AddScore (int index, int newScoreValue)
	{
        PlayerControllers[index].AddScore(newScoreValue * level * level);
        kills += 1;
        level = (kills / 10) + 1;
        UpdateScore();
	}
	
	public void UpdateScore()
	{
        if (RuyiNet.IsRuyiNetAvailable)
        {
            for (int i = 0; i < 4; i++)
            {
                if (RuyiNet.CurrentPlayers[i] != null)
                {
                    scoreText[i].text =
                        "<b>" + RuyiNet.CurrentPlayers[i].profileName + "</b>\n" +
                        "Score: " + PlayerControllers[i].Score +
                        " Strength: " + PlayerControllers[i].Strength;
                }
                else
                {
                    scoreText[i].text = "";
                }
            }
        }
        else
        {
            scoreText[0].text = 
                "Score: " + PlayerControllers[0].Score +
                " Strength: " + PlayerControllers[0].Strength;

            for (int i = 1; i < 4; ++i)
            {
                scoreText[i].text = "";
            }
        }

        levelText.text = " Level: " + level;
    }

    public bool CheckGameOver()
    {
        Debug.Log("CheckGameOver");
        foreach (var i in PlayerControllers)
        {
            if (i != null &&
                i.Strength > 0)
            {
                return false;
            }
        }

        Debug.Log("Game Over");
        GameOver();

        return true;
    }

    public void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOver = true;

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
            {
                if (profile != null)
                {
                    Debug.Log("Current Game: " + index);
                    PlayerControllers[index].ApplyProgression(level);

                    var savePath = Path.Combine(profile.profileName, SAVEGAME_LOCATION);
                    SaveLoad.Save(PlayerControllers[index].CurrentGame, savePath);

                    //if (RuyiNet.CloudService != null)
                    //{
                    //    RuyiNet.CloudService.BackupData(index, Application.persistentDataPath, null);
                    //}

                    var score = PlayerControllers[index].Score;
                    if (RuyiNet.LeaderboardService != null)
                    {
                        RuyiNet.LeaderboardService.PostScoreToLeaderboard(index, "Shooter", score, null);
                    }

                    if (RuyiNet.MatchmakingService != null)
                    {
                        if (score <= 500)
                        {
                            RuyiNet.MatchmakingService.DecrementPlayerRating(index, 5, null);
                        }

                        if (score >= 1000)
                        {
                            RuyiNet.MatchmakingService.IncrementPlayerRating(index, 5, null);
                        }
                    }
                }
            });

        }
        else
        {
            PlayerControllers[0].ApplyProgression(level);
            SaveLoad.Save(PlayerControllers[0].CurrentGame, SAVEGAME_LOCATION);
        }
    }

    public RuyiNet RuyiNet;

    private void OnRuyiNetInitialised()
    {
        Debug.Log("Game Controller: RuyiNet is available");
        RuyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
        {
            if (profile != null)
            {
                Debug.Log("GC: Player " + index);
                //if (RuyiNet.LeaderboardService != null)
                {
                //    RuyiNet.LeaderboardService.CreateLeaderboard(index, "Shooter", RuyiNetLeaderboardType.HIGH_VALUE, RuyiNetRotationType.MONTHLY, null);
                }

                if (RuyiNet.MatchmakingService != null)
                {
                    RuyiNet.MatchmakingService.EnableMatchmaking(index, null);

                    if (RuyiNet.NewUser)
                    {
                        RuyiNet.MatchmakingService.SetPlayerRating(index, 1000, null);
                    }
                }

                //if (RuyiNet.CloudService != null)
                //{
                //    RuyiNet.CloudService.RestoreData(0, Application.persistentDataPath, OnRestoreData);
                //}
                //else
                //{
                //    OnRestoreData(null);
                //}
            }
        });

        OnRestoreData(null);
        UpdateScore();
    }

    private void OnRestoreData(RuyiNetResponse response)
    {
        Debug.Log("Restore Data");

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
            {
                if (profile != null)
                {
                    Debug.Log("RD Profile " + index);
                    var savePath = Path.Combine(profile.profileName, SAVEGAME_LOCATION);
                    PlayerControllers[index].LoadGame(savePath);
                }
            });
        }
        else
        {
            PlayerControllers[0].LoadGame(SAVEGAME_LOCATION);
        }
    }
}
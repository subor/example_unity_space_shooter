using Ruyi;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Done_GameController : MonoBehaviour
{
    public GameObject[] players;
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
    public int[] currentStrength = new int[4] { 1, 1, 1, 1 };

    [Serializable]
    private class Game
    {
        public int strength = 1;
    }

    private Game[] currentGame = new Game[4];

    private bool gameOver;
	private bool restart;
	private int[] score = new int[4] { 0, 0, 0, 0 };
    private int kills;

    private const string SAVEGAME_LOCATION = "savedData.gd";

    void Start ()
    {
        level = 1;

        gameOver = false;
        restart = false;
        restartText.text = "";
        gameOverText.text = "";
        score = new int[4] { 0, 0, 0, 0 };
        kills = 0;

        for (int i = 0; i < 4; ++i)
        {
            players[i].SetActive(false);
        }

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.Initialise(OnRuyiNetInitialised);
        }
        else
        {
            players[0].SetActive(true);
            OnRestoreData(null);
            UpdateScore();
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
		score[index] += newScoreValue * level * level;
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
                        "Score: " + score[i] +
                        " Strength: " + currentStrength[i];
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
                "Score: " + score[0] +
                " Strength: " + currentStrength[0];

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
        foreach (var i in currentStrength)
        {
            if (i > 0)
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
                    if (level > currentGame[index].strength)
                    {
                        currentGame[index].strength++;
                    }

                    var savePath = Path.Combine(profile.profileName, SAVEGAME_LOCATION);
                    SaveLoad.Save(currentGame[index], savePath);

                    //if (RuyiNet.CloudService != null)
                    //{
                    //    RuyiNet.CloudService.BackupData(index, Application.persistentDataPath, null);
                    //}

                    if (RuyiNet.LeaderboardService != null)
                    {
                        RuyiNet.LeaderboardService.PostScoreToLeaderboard(index, "Shooter", score[index], null);
                    }

                    if (RuyiNet.MatchmakingService != null)
                    {
                        if (score[index] <= 500)
                        {
                            RuyiNet.MatchmakingService.DecrementPlayerRating(index, 5, null);
                        }

                        if (score[index] >= 1000)
                        {
                            RuyiNet.MatchmakingService.IncrementPlayerRating(index, 5, null);
                        }
                    }
                }
            });

        }
        else
        {
            if (level > currentGame[0].strength)
            {
                currentGame[0].strength++;
            }

            SaveLoad.Save(currentGame[0], SAVEGAME_LOCATION);
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

                players[index].SetActive(true);

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
        currentStrength = new int[4];

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
            {
                if (profile != null)
                {
                    Debug.Log("RD Profile " + index);
                    var savePath = Path.Combine(profile.profileName, SAVEGAME_LOCATION);
                    try
                    {
                        currentGame[index] = SaveLoad.Load<Game>(savePath);
                    }
                    catch (FileNotFoundException)
                    {
                        currentGame[index] = new Game();
                    }

                    currentStrength[index] = currentGame[index].strength;
                }
            });
        }
        else
        {
            try
            {
                currentGame[0] = SaveLoad.Load<Game>(SAVEGAME_LOCATION);
            }
            catch (FileNotFoundException)
            {
                currentGame[0] = new Game();
            }

            currentStrength[0] = currentGame[0].strength;
        }
    }
}
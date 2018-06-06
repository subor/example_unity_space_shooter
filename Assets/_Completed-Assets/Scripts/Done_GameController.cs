using Ruyi.SDK.Online;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Done_GameController : NetworkBehaviour
{
    public void AddScore(NetworkInstanceId netId, int increment)
    {
        if (isServer)
        {
            for (var i = 0; i < mPlayerState.Count; ++i)
            {
                if (mPlayerState[i].NetId == netId)
                {
                    var state = mPlayerState[i];
                    state.Score += increment * mLevel * mLevel;
                    mPlayerState[i] = state;
                }
            }

            mTotalKills += 1;
            mLevel = (mTotalKills / 10) + 1;
        }
    }

    public void CheckGameOver()
    {
        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            if (mPlayerState[i].CurrentStrength > 0)
            {
                return;
            }
        }

        RpcGameOver();
    }

    public void RegisterPlayer(NetworkInstanceId netId, string ruyiProfileId, string ruyiProfileName)
    {
        Debug.Log("Register Player: " + netId.ToString() + " | Host: " + isServer);

        //  TODO:   Get Profile Name
        var savePath = string.IsNullOrEmpty(ruyiProfileName) ? SAVEGAME_LOCATION : Path.Combine(ruyiProfileName, SAVEGAME_LOCATION);
        SaveGame saveGame;
        try
        {
            saveGame = mSaveLoad.Load<SaveGame>(RuyiNet.GetActivePersistentDataPath(), savePath);
        }
        catch (FileNotFoundException)
        {
            saveGame = new SaveGame();
        }

        PlayerStateData playerState = new PlayerStateData
        {
            NetId = netId,
            ProfileId = ruyiProfileId,
            ProfileName = string.IsNullOrEmpty(ruyiProfileName) ? "Player " + netId : ruyiProfileName,
            Score = 0,
            CurrentStrength = saveGame.Strength,
            MaxStrength = saveGame.Strength
        };
        
        if (mPlayerState != null)
        {
            mPlayerState.Add(playerState);
        }

        RpcUpdatePlayerColors();
    }

    public void UnregisterPlayer(Done_PlayerController player)
    {
        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            if (mPlayerState[i].NetId == netId)
            {
                Debug.Log("UnregisterPlayer");
                mPlayerState.RemoveAt(i);
                break;
            }
        }
    }

    public PlayerStateData GetPlayerState(NetworkInstanceId netId)
    {
        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            if (mPlayerState[i].NetId == netId)
            {
                return mPlayerState[i];
            }
        }

        throw new KeyNotFoundException();
    }

    public int GetPlayerScore(NetworkInstanceId netId)
    {
        return GetPlayerState(netId).Score;
    }

    public int GetPlayerStrength(NetworkInstanceId netId)
    {
        return GetPlayerState(netId).CurrentStrength;
    }

    public void DamagePlayer(NetworkInstanceId netId)
    {
        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            if (mPlayerState[i].NetId == netId)
            {
                var state = mPlayerState[i];
                state.CurrentStrength = Math.Max(state.CurrentStrength - 1, 0);
                mPlayerState[i] = state;
            }
        }
    }

    public void ResetPlayer(NetworkInstanceId netId)
    {
        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            if (mPlayerState[i].NetId == netId)
            {
                var state = mPlayerState[i];
                state.CurrentStrength = state.MaxStrength;
                state.Score = 0;
                mPlayerState[i] = state;
            }
        }
    }

    public void LogDestructionEvent(string tag, Vector3 position)
    {
        if (RuyiNet &&
            RuyiNet.IsRuyiNetAvailable)
        {
            var customData = new Dictionary<string, string>();
            customData["position"] = "[" + position.x + ", " + position.z + "]";
            RuyiNet.TelemetryService.LogTelemetryEvent(0, mTelemetrySessionId, "kill", tag, customData, null);
        }
    }

    public struct PlayerStateData
    {
        public NetworkInstanceId NetId;
        public string ProfileId;
        public string ProfileName;
        public int Score;
        public int CurrentStrength;
        public int MaxStrength;
    };

    public class SyncListPlayerState : SyncListStruct<PlayerStateData> {}

    public SyncListPlayerState PlayerState { get { return mPlayerState; } }
    public int Level { get { return mLevel; } }
    public int TotalKills { get { return mTotalKills; } }

    public RuyiNet RuyiNet;
    public GUIText[] scoreText;
    public GUIText gameOverText;
    public GUIText levelText;

    private void Awake()
    {
        mSaveLoad = new SaveLoad();
    }

    [ClientRpc]
    private void RpcUpdatePlayerColors()
    {
        var playerControllers = FindObjectsOfType<Done_PlayerController>();
        for (int i = 0; i < PlayerState.Count && i < playerColors.Length; i++)
        {
            foreach (var pc in playerControllers)
            {
                if (pc.netId == PlayerState[i].NetId)
                {
                    pc.GetComponent<MeshRenderer>().materials[0].color = playerColors[i];
                    break;
                }
            }
        }
    }

    [ClientRpc]
    private void RpcUpdateScore(SyncListPlayerState.Operation op, int itemIndex)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < PlayerState.Count)
            {
                scoreText[i].text =
                    "<b>" + PlayerState[i].ProfileName + "</b>\n" +
                    "Score: " + PlayerState[i].Score +
                    " Strength: " + PlayerState[i].CurrentStrength;
            }
            else
            {
                scoreText[i].text = "";
            }
        }

        levelText.text = " Level: " + mLevel;
    }

    [ClientRpc]
    private void RpcGameOver()
    {
        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        gameOverText.text = "Game Over!";

        yield return new WaitForSeconds(5);

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            Debug.Log("Ending telemetry session with ID " + mTelemetrySessionId);
            RuyiNet.TelemetryService.EndTelemetrySession(0, mTelemetrySessionId, null);
        }

        for (var i = 0; i < mPlayerState.Count; ++i)
        {
            var saveGame = new SaveGame() { Strength = mPlayerState[i].MaxStrength };
            if (mLevel > saveGame.Strength)
            {
                ++saveGame.Strength;
            }

            Debug.Log("RuyiNet: " + RuyiNet);
            Debug.Log("IsRuyiNetAvailable: " + RuyiNet.IsRuyiNetAvailable);
            Debug.Log("ProfileId: " + mPlayerState[i].ProfileId);

            if (RuyiNet == null ||
                !RuyiNet.IsRuyiNetAvailable ||
                string.IsNullOrEmpty(mPlayerState[i].ProfileId))
            {
                mSaveLoad.Save(saveGame, RuyiNet.GetActivePersistentDataPath(), SAVEGAME_LOCATION);
                ReturnToMenu();
            }
            else
            {
                RuyiNet.ForEachPlayer((int index, RuyiNetProfile profile) =>
                {
                    if (profile != null &&
                        profile.profileId == mPlayerState[i].ProfileId)
                    {
                        var savePath = Path.Combine(profile.profileName, SAVEGAME_LOCATION);
                        mSaveLoad.Save(saveGame, RuyiNet.GetPersistentDataPath(index), savePath);

                        if (RuyiNet.CloudService != null)
                        {
                            Debug.Log("Backup Data");
                            RuyiNet.CloudService.BackupData(index, null);
                        }

                        var score = mPlayerState[i].Score;
                        if (RuyiNet.LeaderboardService != null)
                        {
                            RuyiNet.LeaderboardService.PostScoreToLeaderboard(index, "Shooter", score, null);
                        }
                    }
                });

                while (RuyiNet.IsWorking)
                {
                    yield return null;
                }

                ReturnToMenu();
            }
        }
    }

    private void ReturnToMenu()
    {
        var networkManager = FindObjectOfType<MyNetworkManager>();
        if (networkManager != null)
        {
            if (isServer)
            {
                networkManager.StopHost();
            }
            else
            {
                networkManager.StopClient();
            }

            networkManager.StopMatchMaker();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Start()
    {
        Debug.Log("Game Start" + " | Host: " + isServer);

        mLevel = 1;
        mTotalKills = 0;
        mPlayerState.Callback = RpcUpdateScore;

        if (RuyiNet != null &&
            RuyiNet.IsRuyiNetAvailable)
        {
            RuyiNet.TelemetryService.StartTelemetrySession(0,
                (RuyiNetTelemetrySession session) =>
            {
                RuyiNet.TelemetryService.LogTelemetryEvent(0, session.Id, "GAME_STARTED", null);
                mTelemetrySessionId = session.Id;

                Debug.Log("Started telemetry session with ID " + session.Id);
            });
        }


        gameOverText.text = "";
    }

    [Serializable]
    private class SaveGame
    {
        public int Strength = 1;
    }

    private readonly Color[] playerColors = {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    private const string SAVEGAME_LOCATION = "savedData.gd";

    [SyncVar] private int mLevel;
    [SyncVar] private int mTotalKills;
    SyncListPlayerState mPlayerState = new SyncListPlayerState();
    private SaveLoad mSaveLoad;
    private string mTelemetrySessionId;
}

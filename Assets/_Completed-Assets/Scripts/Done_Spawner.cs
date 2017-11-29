using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Done_Spawner : NetworkBehaviour
{
    public Vector3 spawnValues;
    public GameObject[] hazards;
    public int hazardCount;
    public float startWait;
    public float spawnWait;
    public float waveWait;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    public IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        while (true)
        {
            for (int i = 0; i < hazardCount; i++)
            {
                GameObject hazard = hazards[UnityEngine.Random.Range(0, hazards.Length)];
                Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
                Quaternion spawnRotation = Quaternion.identity;
                var spawned = Instantiate(hazard, spawnPosition, spawnRotation);
                NetworkServer.Spawn(spawned);
                yield return new WaitForSeconds(spawnWait);
            }

            yield return new WaitForSeconds(waveWait + (spawnWait));
        }
    }
}

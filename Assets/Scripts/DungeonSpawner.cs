using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSpawner : MonoBehaviour
{

    [SerializeField] List<GameObject> spawnPos;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemyNumber++;
        gameManager.instance.updateGameGoal();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && spawnPos.Count > 0)
        {
            for (int i = 0; i < spawnPos.Count; i++)
            {
                spawnPos[i].GetComponent<SpawnLocal>().spawnEnemy();
            }
        }
    }

    public void addLocal(GameObject local)
    {
        spawnPos.Add(local);
    }
}

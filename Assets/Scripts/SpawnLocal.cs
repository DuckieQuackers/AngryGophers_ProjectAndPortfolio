using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocal : MonoBehaviour
{
    GameObject spawn;
    [SerializeField] string spawnTag;
    [SerializeField] GameObject enemyHeld;

    bool hasSpawned;

    // Start is called before the first frame update
    void Start()
    {
        spawn = GameObject.FindGameObjectWithTag(spawnTag);
        spawn.GetComponent<DungeonSpawner>().addLocal(gameObject);
    }

    public void spawnEnemy()
    {
        if (!hasSpawned)
        {
            Instantiate(enemyHeld, transform.position, transform.rotation);
            hasSpawned = true;
        }
    }
}

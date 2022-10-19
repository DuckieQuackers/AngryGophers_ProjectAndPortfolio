using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomSpawn : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] int maxEnemies;
    [SerializeField] int timer;
    int spawnedEnemies;
    bool isSpawning;
    bool canSpawn;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemyNumber = maxEnemies;
        gameManager.instance.updateGameGoal();
    }

    // Update is called once per frame
    void Update()
    {
        if (canSpawn && !isSpawning && spawnedEnemies < maxEnemies)
        {
            StartCoroutine(spawn());
        }
    }

    IEnumerator spawn()
    {
        isSpawning = true;
        int picked = Random.Range(0, enemies.Length - 1);
        if(transform.parent != null)
            Instantiate(enemies[picked], transform.parent.position, enemies[picked].transform.rotation);
        else
            Instantiate(enemies[picked], transform.position, enemies[picked].transform.rotation);
        spawnedEnemies++;
        yield return new WaitForSeconds(timer);
        isSpawning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSpawn = true;
        }
    }
}
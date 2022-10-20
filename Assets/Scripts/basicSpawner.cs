using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicSpawner : MonoBehaviour
{

    [SerializeField] GameObject spawnPos;
    [SerializeField] GameObject specificEnemy;
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
        if(spawnPos != null)
            Instantiate(specificEnemy, spawnPos.transform.position, specificEnemy.transform.rotation);
        else
            Instantiate(specificEnemy, transform.position, specificEnemy.transform.rotation);
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

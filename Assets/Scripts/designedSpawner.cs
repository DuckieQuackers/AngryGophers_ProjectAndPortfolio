using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class designedSpawner : MonoBehaviour
{

    [SerializeField] GameObject[] chosenEnemies;
    [SerializeField] int allEnemies;
    [SerializeField] int timer;
    int spawnedEnemies;
    int picked = 0;
    bool isSpawning;
    bool canSpawn;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemyNumber = allEnemies;
        gameManager.instance.updateGameGoal();
    }

    // Update is called once per frame
    void Update()
    {
        if (canSpawn && !isSpawning && spawnedEnemies < allEnemies)
        {
            StartCoroutine(spawn());
        }
    }

    IEnumerator spawn()
    {
        isSpawning = true;
        if (picked <= chosenEnemies.Length - 1)
        {
            if(transform.parent != null)
                Instantiate(chosenEnemies[picked], transform.parent.position, chosenEnemies[picked].transform.rotation);
            else
                Instantiate(chosenEnemies[picked], transform.position, chosenEnemies[picked].transform.rotation);
            picked++;
            spawnedEnemies++;
            yield return new WaitForSeconds(timer);
            isSpawning = false;
        }
        else
        {
            yield return new WaitForEndOfFrame();
            isSpawning = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSpawn = true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, iDamage
{
    [SerializeField] int hp;
    [SerializeField] NavMeshAgent agent;

    public bool playerInRange;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
            agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
    }

    public void takeDamage(int dmg)
    {
        hp -= dmg;
        
        if(hp <= 0)
            Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            playerInRange= false;
    }
}

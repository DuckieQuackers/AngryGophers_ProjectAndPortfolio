using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, iDamage
{
    [SerializeField] int hp;
    [SerializeField] NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);
    }

    public void takeDamage(int dmg)
    {
        hp -= dmg;
        
        if(hp <= 0)
            Destroy(gameObject);
    }
}

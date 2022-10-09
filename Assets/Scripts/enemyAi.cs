using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, iDamage
{
    [Header ("----- Components -----")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject bulletOrigin;

    [Header("----- Enemy stats -----")]
    [SerializeField] int hp;
    [SerializeField] float shootRate;


    bool playerInRange;
    bool isShooting;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            agent.SetDestination(GameObject.FindGameObjectWithTag("Player").transform.position);

            if(!isShooting)
                StartCoroutine(shoot());
        }
    }

    public void takeDamage(int dmg)
    {
        hp -= dmg;
        
        if(hp <= 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            playerInRange= false;
    }
    IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(bullet, bulletOrigin.transform.position, transform.rotation);
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
}

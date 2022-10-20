using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossAI : enemyAi
{
    [Header("----- Boss Stats -----")]
    [SerializeField] float closeRange;
    [SerializeField] int spawnRate;
    [SerializeField] GameObject attackPort1;
    [SerializeField] GameObject attackPort2;
    [SerializeField] GameObject poison;
    [SerializeField] GameObject child;

    bool spawning;

    public override IEnumerator attack()
    {
        isAttacking = true;

        float playerDis = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

        if(playerDis > closeRange)
        {
            Instantiate(bullet, eyes.transform.position, transform.rotation);
            Instantiate(bullet, attackPort1.transform.position, attackPort1.transform.rotation);
            Instantiate(bullet, attackPort2.transform.position, attackPort2.transform.rotation);
        }
        else
        {
            if(!spawning)
                StartCoroutine(spawnMore());

            Instantiate(poison, eyes.transform.position, transform.rotation);
        }

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

    IEnumerator spawnMore()
    {
        spawning = true;
        Instantiate(child, attackPort1.transform.position, transform.rotation);
        Instantiate(child, attackPort2.transform.position, transform.rotation);

        yield return new WaitForSeconds(spawnRate);
        spawning = false;
    }
}

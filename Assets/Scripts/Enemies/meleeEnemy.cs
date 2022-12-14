using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class meleeEnemy : enemyAi, iDamage
{
    [Header("----- Melee combat -----")]
    [Range(1, 10)] [SerializeField] int biteDamage;
    [Range(1, 10)] [SerializeField] int biteRange;

    public override IEnumerator attack()
    {
        isAttacking = true;

        RaycastHit hit;
        if(Physics.Raycast(eyes.transform.position, eyes.transform.forward, out hit, biteRange))
        {
            if(hit.transform.tag == "Player")
            {
                aud.PlayOneShot(attackAud, attackVol);
                gameManager.instance.playerScript.takeDamage(biteDamage);
            }
        }

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }
}

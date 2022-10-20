using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poisonShot : bullet
{
    [Header("-----Poison stats-----")]
    [SerializeField] int ticks;
    [SerializeField] float tick;

    public override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerScript.startDoT(ticks);
        }

        Destroy(gameObject);
    }
}

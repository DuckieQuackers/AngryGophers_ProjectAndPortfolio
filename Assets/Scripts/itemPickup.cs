using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemPickup : MonoBehaviour
{
    [SerializeField] itemGrabs itemValues;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerScript.itemPickup(itemValues);
            gameManager.instance.playerScript.UpdatePlayerHud();
            Destroy(gameObject);
        }
    }
}
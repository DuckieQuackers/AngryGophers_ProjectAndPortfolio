using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]

public class itemGrabs : ScriptableObject
{
    //Gun and other ranged weapon values
    public float fireRate;
    public int fireDistance;
    public int damage;
    public int ammoCount;

    //Player upgrades
    public int addSpeed;
    public int addJumps;
    public int addHealth;
    public int addStamina;

    //Components
    public GameObject designModel;
}

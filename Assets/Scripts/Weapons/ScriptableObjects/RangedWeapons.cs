using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RangedWeapons : ScriptableObject
{
    //Gun and other ranged weapon values
    public float fireRate;
    public int fireDistance;
    public int damage;
    public int ammoCount;
    public int chamber;
    public int reloadCount;
    public int reloadTime;
    public int maxAmmo;

    //Components
    public GameObject designModel;
    public AudioClip triggerSound;
    public GameObject hitEffect;
    public GameObject triggerEffect;
}
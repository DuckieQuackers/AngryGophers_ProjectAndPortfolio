using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public float shootRate;
    public int shootDist;
    public int shootDamage;
    public int ammoCount;
    public GameObject gunModel;
    public AudioClip sound;
    public GameObject hitEffect;
    public GameObject muzzleEffect;
}

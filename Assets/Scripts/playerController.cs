using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, iDamage
{
    [Header("----Player Stats----")]
    [SerializeField] int HP;
    [SerializeField] float sprintSpeed;
    [SerializeField] CharacterController controller;
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravityModifier;
    //stamina required

    [SerializeField] int jumpsMax;
    private int jumpCount;

    [Header("---Weapon Stats---")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int shootDmg;
    [SerializeField] int chamber;
    [SerializeField] int reloadCount;
    [SerializeField] int reloadTime;

    [Header("----- Audio -----")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] playerTookDamage;
    [Range(0, 1)] [SerializeField] float playerTookDamageAudVolume;
    [SerializeField] AudioClip[] playerMoving;
    [Range(0, 1)] [SerializeField] float playerMovingAudVolume;
    [SerializeField] AudioClip[] playerJumps;
    [Range(0, 1)] [SerializeField] float playerJumpsAudVolume;
    private AudioClip gunFireSound;
    [Range(0, 1)] [SerializeField] float gunFireSoundAudVolume;


    [SerializeField] public List<RangedWeapons> weaponListStats = new List<RangedWeapons>();


    [Header("----- Gun Components -----")]
    [SerializeField] GameObject gunModel;

    int HPOrig;
    private Vector3 playerVelocity;
    Vector3 move;
    public bool isShooting;
    private bool isReloading =false;
    public bool playingMoveAudio;
    public bool playerSprinting;
    public bool grabbedPickup;
    public int selectedGun;
    private int nextJump;

    private void Start()
    {
        HPOrig = HP;

        respawn();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        jumping();
        if (isReloading)
        {
            return;
        }
            if (gameManager.instance.playerScript.weaponListStats[gameManager.instance.playerScript.selectedGun].trackedAmmo > 0)
            {
                StartCoroutine(shoot(weaponListStats[selectedGun]));
                return;
            }
            if (!isReloading)
            {
                StartCoroutine(reloadWeapon(weaponListStats[selectedGun]));
                return;
            }
        
        gunSelection();

    }
    IEnumerator shoot(RangedWeapons currentGun)
    {
        if (weaponListStats.Count > 0 && Input.GetButton("Fire1") && !isShooting)
        {
            
                isShooting = true;
                //currentAmmo = currentAmmo - chamber;
                currentGun.trackedAmmo = currentGun.trackedAmmo - currentGun.chamber;
                audioSource.PlayOneShot(gunFireSound, gunFireSoundAudVolume);
                gameManager.instance.updateAmmoCount();
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
                {
                    if (hit.collider.GetComponent<iDamage>() != null)
                    {
                        hit.collider.GetComponent<iDamage>().takeDamage(shootDmg);
                    }
                }
                yield return new WaitForSeconds(shootRate);
                isShooting = false;
            
        }

    }
    IEnumerator reloadWeapon(RangedWeapons stats)
    {
        isReloading = true;
        if(stats.trackedMaxAmmo >= stats.reloadCount)
        {
            //maxAmmo = maxAmmo - reloadCount;
            stats.trackedMaxAmmo = stats.trackedMaxAmmo - reloadCount;
            //currentAmmo = reloadCount;
            stats.trackedAmmo = stats.reloadCount;
        }else if(stats.trackedMaxAmmo <= stats.reloadCount)
        {
            //currentAmmo = maxAmmo;
            stats.trackedAmmo = stats.trackedMaxAmmo;
            stats.trackedMaxAmmo = stats.trackedMaxAmmo - stats.trackedMaxAmmo;
        }
        else
        {
            stats.trackedMaxAmmo = 0;
            stats.trackedAmmo = 0;
        }
        //play reload sound
        //play reload animation
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
    }
    void movement()
    {
        //resetting playerVelocity and jumpCount
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            jumpCount = 0;
            playerVelocity.y = 0;
        }
        //Player Movement and Sprint
        move = (transform.right * Input.GetAxis("Horizontal") + (transform.forward * Input.GetAxis("Vertical")));
        if (Input.GetButton("Sprint"))
        {
            playerSprinting = true;
            //remove stamina here
            controller.Move(move * Time.deltaTime * (sprintSpeed + playerSpeed));
        }
        else
        {
            playerSprinting = false;
            controller.Move(move * Time.deltaTime * playerSpeed);
        }


        controller.Move(playerVelocity * Time.deltaTime);
        StartCoroutine(playMovingNoises());
    }
    IEnumerator playMovingNoises()
    {
        if (move.magnitude > .3f && !playingMoveAudio && controller.isGrounded)
        {

            playingMoveAudio = true;
            //  if(on carpet tag){
            //        play carpet steps
            //  } else if(on wood tag){
            //        play wood steps
            //  } else if(on metal, etc tag){
            //        play those noises
            //  } else (default steps go here)
            audioSource.PlayOneShot(playerMoving[Random.Range(0, playerMoving.Length - 1)], playerMovingAudVolume);
            if (playerSprinting)
                yield return new WaitForSeconds(.3f);
            else
                yield return new WaitForSeconds(.6f);
            playingMoveAudio = false;

        }
    }
    public void takeDamage(int dmg)
    {
        HP -= dmg;
        audioSource.PlayOneShot(playerTookDamage[Random.Range(0, playerTookDamage.Length - 1)], playerTookDamageAudVolume);
        UpdatePlayerHud();
        StartCoroutine(gameManager.instance.playerDamage());
        if (HP <= 0)
        {
            gameManager.instance.playerDeadMenu.SetActive(true);
            gameManager.instance.cursorLockPause();
        }
    }
    void jumping()
    {
        //jumping requires stamina check
        if (Input.GetButtonDown("Jump") && jumpCount < jumpsMax)
        {
            audioSource.PlayOneShot(playerJumps[nextJump], playerJumpsAudVolume);
            nextJump++;
            if (nextJump > playerJumps.Length - 1)
            {
                nextJump = 0;
            }
            jumpCount++;
            playerVelocity.y = jumpHeight;
            //remove stamina here
        }
        playerVelocity.y -= gravityModifier * Time.deltaTime;
    }
    public void weaponPickup(RangedWeapons stats)
    {
        shootRate = stats.fireRate;
        shootDist = stats.fireDistance;
        shootDmg = stats.damage * chamber;
        chamber = stats.chamber;
        gameManager.instance.currentAmmo = stats.ammoCount;
        gameManager.instance.maximumAmmo = stats.maxAmmo;
        reloadCount = stats.reloadCount;
        reloadTime = stats.reloadTime;
        gunFireSound = stats.triggerSound;
        gunModel.GetComponent<MeshFilter>().sharedMesh = stats.designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = stats.designModel.GetComponent<MeshRenderer>().sharedMaterial;
        weaponListStats.Add(stats);
    }
    public void itemPickup(itemGrabs item, RangedWeapons currentWeapon)
    {
        grabbedPickup = true;
        shootRate = shootRate/item.fireRate;
        shootDist += item.fireDistance;
        shootDmg += item.damage;
        jumpsMax += item.addJumps;
        sprintSpeed += item.addSpeed;
        currentWeapon.trackedMaxAmmo += item.ammoCount;
        if(grabbedPickup)
            StartCoroutine(coolDown(item));
        if (HP < HPOrig && item.addHealth == 1)
        {
            HP = HPOrig;
        }
        else if (HP < HPOrig && item.addHealth == 2)
        {
            HP += 5;
            if (HP > HPOrig)
            {
                HP = HPOrig;
            }
        }
    }
    IEnumerator coolDown(itemGrabs item)
    {
        
        yield return new WaitForSeconds(10.00f);
        shootRate = shootRate*item.fireRate;
        shootDist -= item.fireDistance;
        shootDmg -= item.damage;
        jumpsMax -= item.addJumps;
        sprintSpeed -= item.addSpeed;
        grabbedPickup = false;
    }
    public void gunSelection()
    {
        if (weaponListStats.Count > 1)
        {
            if (!isReloading)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedGun < weaponListStats.Count - 1)
                {
                    selectedGun++;
                    weaponSwap();


                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedGun > 0)
                {
                    selectedGun--;
                    weaponSwap();

                }
            }
        }
    }
    public void weaponSwap()
    {
        shootRate = weaponListStats[selectedGun].fireRate;
        shootDist = weaponListStats[selectedGun].fireDistance;
        chamber = weaponListStats[selectedGun].chamber;
        shootDmg = weaponListStats[selectedGun].damage * chamber;
        gameManager.instance.currentAmmo = weaponListStats[selectedGun].ammoCount;
        gameManager.instance.maximumAmmo = weaponListStats[selectedGun].maxAmmo;
        reloadCount = weaponListStats[selectedGun].reloadCount;
        reloadTime = weaponListStats[selectedGun].reloadTime;
        gunFireSound = weaponListStats[selectedGun].triggerSound;

        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponListStats[selectedGun].designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponListStats[selectedGun].designModel.GetComponent<MeshRenderer>().sharedMaterial;
    }
    public void UpdatePlayerHud()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / (float)HPOrig;
        //add current ammo/ max ammo update
        //add stamina update
    }
    public void respawn()
    {
        controller.enabled = false;
        gameManager.instance.playerDeadMenu.SetActive(false);
        HP = HPOrig;
        UpdatePlayerHud();
        gameManager.instance.updateAmmoCount();
        transform.position = gameManager.instance.spawnPosition.transform.position;
        controller.enabled = true;
    }
}
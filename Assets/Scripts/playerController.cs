
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, iDamage
{
    [Header("---PowerUp Modifer---")]
    [SerializeField] float shootRateUp;
    [SerializeField] int shootDamageUp;
    [SerializeField] int shootDistanceUp;
    public bool isFireRateUp;
    public bool isDamageUp;
    public bool isRangeUp;
    public bool isShootDistanceUp;




    [Header("----Player Stats----")]
    [SerializeField] int HP;
    [SerializeField] float sprintSpeed;
    [SerializeField] CharacterController controller;
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravityModifier;
    [SerializeField] float stamina;
    [SerializeField] float currentStamina;
    [SerializeField] int poisonDmg;
    [SerializeField] float dotTickRate;
    //stamina required

    [SerializeField] int jumpsMax;
    private int jumpCount;

    [Header("---Weapon Stats---")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int shootDmg;
    [SerializeField] int currentAmmo;
    [SerializeField] int maxAmmo;
    [SerializeField] int chamber;
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
    [SerializeField] AudioClip reloadSound;
    [Range(0, 1)] [SerializeField] float reloadSoundAudVolume;



    [SerializeField] public List<RangedWeapons> weaponListStats = new List<RangedWeapons>();


    [Header("----- Gun Components -----")]
    [SerializeField] GameObject gunModel;

    int HPOrig;
    private Vector3 playerVelocity;
    Vector3 move;
    public bool isShooting;
    private bool isReloading = false;
    public bool playingMoveAudio;
    public bool playerSprinting;
    public bool grabbedPickup;
    public bool onCooldown;
    [SerializeField] public int selectedGun;
    private int nextJump;
    List<int> poisonStack = new List<int>();

    private void Start()
    {
        HPOrig = HP;
        currentStamina = stamina;

        gameManager.instance.updateAmmoCount(0, 0);
        UpdatePlayerHud();
        respawn();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        jumping();

        gunSelection();

        if (isReloading)
        {
            return;
        }
        if (weaponListStats.Count > 0)
        {

            if (weaponListStats[selectedGun].trackedAmmo > 0 && !isReloading)
            {
                    StartCoroutine(shoot(weaponListStats[selectedGun]));
            }
            else if (weaponListStats[selectedGun].trackedAmmo <= 0)
            {
                StartCoroutine(reloadWeapon(weaponListStats[selectedGun]));
            }
        }
    }
    IEnumerator shoot(RangedWeapons currentGun)
    {
        if (Input.GetButton("Fire1") && !isShooting)
        {
            isShooting = true;
            currentGun.trackedAmmo -= currentGun.chamber;
            audioSource.PlayOneShot(gunFireSound, gunFireSoundAudVolume);
            gameManager.instance.updateAmmoCount(currentGun.trackedAmmo, currentGun.trackedMaxAmmo);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
            {
                if (hit.collider.GetComponent<iDamage>() != null)
                    hit.collider.GetComponent<iDamage>().takeDamage(shootDmg);
                
            }
            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }
        
    }
    IEnumerator reloadWeapon(RangedWeapons stats)
    {
        isReloading = true;
        if (stats.trackedMaxAmmo - stats.ammoCount - stats.trackedAmmo >= 0)
        {
            stats.trackedMaxAmmo -= stats.ammoCount - stats.trackedAmmo;
            stats.trackedAmmo = stats.ammoCount;
            reloadTime = stats.reloadTime;
            audioSource.PlayOneShot(reloadSound, reloadSoundAudVolume);
        }
        else if (stats.trackedAmmo > 0)
        {
            stats.trackedAmmo = stats.trackedMaxAmmo;
            stats.trackedMaxAmmo = 0;
            reloadTime = stats.reloadTime;
            audioSource.PlayOneShot(reloadSound, reloadSoundAudVolume);
        }
        else
            reloadTime = 0;
        //play reload sound
        //play reload animation
        yield return new WaitForSeconds(reloadTime);
        gameManager.instance.updateAmmoCount(stats.trackedAmmo, stats.trackedMaxAmmo);
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
        if (Input.GetButton("Sprint") && currentStamina!=0)
        {
            playerSprinting = true;
            //remove stamina here
            currentStamina--;
            UpdatePlayerHud();
            controller.Move(move * Time.deltaTime * (sprintSpeed + playerSpeed));
        }
        else
        {
            playerSprinting = false;
            controller.Move(move * Time.deltaTime * playerSpeed);
            if(currentStamina <= 0 && !onCooldown)
            {
                StartCoroutine(sprintCooldown());
            }
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
        if (HP <= 0)
        {
            gameManager.instance.playerDeadMenu.SetActive(true);
            gameManager.instance.cursorLockPause();
        }
        else
        {
            StartCoroutine(gameManager.instance.playerDamage());
        }
    }
    void jumping()
    {
        //jumping requires stamina check
        if (Input.GetButtonDown("Jump") && jumpCount < jumpsMax && currentStamina!=0)
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
            currentStamina--;
            UpdatePlayerHud();
        }
        playerVelocity.y -= gravityModifier * Time.deltaTime;
    }
    public void weaponPickup(RangedWeapons stats)
    {
        shootDmg = stats.damage;
        shootRate = stats.fireRate;
        shootDist = stats.fireDistance;
        shootDmg = stats.damage;
        chamber = stats.chamber;
        stats.trackedAmmo = stats.ammoCount;
        stats.trackedMaxAmmo = stats.maxAmmo;
        reloadTime = stats.reloadTime;
        gunFireSound = stats.triggerSound;
        gunModel.GetComponent<MeshFilter>().sharedMesh = stats.designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = stats.designModel.GetComponent<MeshRenderer>().sharedMaterial;
        weaponListStats.Add(stats);
        gameManager.instance.updateAmmoCount(stats.trackedAmmo, stats.trackedMaxAmmo);
    }
    public void itemPickup(itemGrabs item)
    {
        grabbedPickup = true;
        //shootRate = shootRate / item.fireRate;
        shootDist += item.fireDistance;
        shootDmg += item.damage;
        jumpsMax += item.addJumps;
        sprintSpeed += item.addSpeed;
        maxAmmo += item.ammoCount;
        if (grabbedPickup)
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

        grabbedPickup = false;
    }
    IEnumerator coolDown(itemGrabs item)
    {
        if (item.fireRate == 1)
        {
            isFireRateUp = true;
            shootRate /= shootRateUp;
            yield return new WaitForSeconds(10.00f);
            isFireRateUp = false;
            shootRate *= shootRateUp;
            grabbedPickup = false;
        }
        else if (item.damage == 1)
        {
            isDamageUp = true;
            shootDmg += shootDamageUp;
            yield return new WaitForSeconds(10.00f);
            shootDmg -= shootDamageUp;
            grabbedPickup = false;
            isDamageUp = false;

        }
        else if (item.fireDistance == 1)
        {
            isShootDistanceUp = true;
            shootDist += shootDistanceUp;
            yield return new WaitForSeconds(10.00f);
            shootDist -= shootDistanceUp;
            grabbedPickup = false;
            isShootDistanceUp = false;

        }
        else
        {

            yield return new WaitForSeconds(10.00f);
            shootDist -= item.fireDistance;
            shootDmg -= item.damage;
            jumpsMax -= item.addJumps;
            sprintSpeed -= item.addSpeed;
            grabbedPickup = false;
        }
    }
    public void gunSelection()
    {
        if (weaponListStats.Count > 0)
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
    public void weaponSwap()
    {
        if (isFireRateUp)
        {
            shootRate = weaponListStats[selectedGun].fireRate / shootRateUp;
        }
        else if (isDamageUp)
        {
            shootDmg = weaponListStats[selectedGun].damage + shootDamageUp;
        }
        else if (isShootDistanceUp)
        {
            shootDist = weaponListStats[selectedGun].fireDistance + shootDistanceUp;
        }
        else
        {
            shootRate = weaponListStats[selectedGun].fireRate;
            shootDist = weaponListStats[selectedGun].fireDistance;
            shootDmg = weaponListStats[selectedGun].damage;
            

        }
        shootRate = weaponListStats[selectedGun].fireRate;
        shootDist = weaponListStats[selectedGun].fireDistance;
        chamber = weaponListStats[selectedGun].chamber;
        shootDmg = weaponListStats[selectedGun].damage * chamber;
        reloadTime = weaponListStats[selectedGun].reloadTime;
        gunFireSound = weaponListStats[selectedGun].triggerSound;

        gameManager.instance.updateAmmoCount(weaponListStats[selectedGun].trackedAmmo, weaponListStats[selectedGun].trackedMaxAmmo);
        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponListStats[selectedGun].designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponListStats[selectedGun].designModel.GetComponent<MeshRenderer>().sharedMaterial;
    }
    public void UpdatePlayerHud()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / (float)HPOrig;
        gameManager.instance.staminaDrain.fillAmount = currentStamina / stamina;
        //add current ammo/ max ammo update
        //add stamina update
    }
    public void respawn()
    {
        controller.enabled = false;
        gameManager.instance.playerDeadMenu.SetActive(false);
        HP = HPOrig;
        UpdatePlayerHud();
        if (gameManager.instance.playerScript.selectedGun > 0)
        {
            gameManager.instance.updateAmmoCount(weaponListStats[selectedGun].trackedAmmo, weaponListStats[selectedGun].trackedMaxAmmo) ;
        }
        
        transform.position = gameManager.instance.spawnPosition.transform.position;
        controller.enabled = true;
    }

    IEnumerator sprintCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1f);
        currentStamina = stamina;
        UpdatePlayerHud();
        onCooldown = false;

           

        
    }
    public void startDoT(int ticks)
    {
        if(poisonStack.Count <= 0)
        {
            poisonStack.Add(ticks);
            StartCoroutine(DoT());
        }
        else
            poisonStack.Add(ticks);
    }
    IEnumerator DoT()
    {
        while (poisonStack.Count > 0)
        {
            for(int i = 0; i < poisonStack.Count; i++)
            {
                poisonStack[i]--;
            }
            takeDamage(poisonDmg);
            poisonStack.RemoveAll(i => i == 0);

            yield return new WaitForSeconds(dotTickRate);
        }
    }
}
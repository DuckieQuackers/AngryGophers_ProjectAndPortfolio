using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, iDamage
{
    [Header("---PowerUp Modifer---")]
    [SerializeField] float shootRateUp;
    [SerializeField] int shootDamageUp;
    [SerializeField] int shootDistanceUp;
    [Header("----Player Stats----")]
    [SerializeField] int HP;
    [SerializeField] int ammoHeld;
    [SerializeField] float sprintSpeed;
    [SerializeField] CharacterController controller;
    [SerializeField] float playerSpeed;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravityModifier;
    [SerializeField] float stamina;
    [SerializeField] float currentStamina;

    [SerializeField] int jumpsMax;
    private int jumpCount;

    [Header("---Weapon Stats---")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int shootDmg;

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


    [SerializeField] List<RangedWeapons> weaponListStats = new List<RangedWeapons>();


    [Header("----- Gun Components -----")]
    [SerializeField] GameObject gunModel;

    int HPOrig;
    private Vector3 playerVelocity;
    Vector3 move;
    public bool isShooting;
    public bool playingMoveAudio;
    public bool playerSprinting;
    public bool grabbedPickup;
    public bool isFireRateUp;
    public bool isDamageUp;
    public bool isRangeUp;
    public bool isShootDistanceUp;
    [SerializeField] int selectedGun;
    private int nextJump;

    private void Start()
    {
        HPOrig = HP;
        respawn();
        currentStamina = stamina;
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        jumping();
        StartCoroutine(shoot());
        gunSelection();

    }
    //player shooting
    IEnumerator shoot()
    {
        if (weaponListStats.Count > 0 && Input.GetButton("Fire1") && !isShooting)
        {

            isShooting = true;
            audioSource.PlayOneShot(gunFireSound, gunFireSoundAudVolume);
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
            if (playerSprinting)
            {
                controller.Move(move * Time.deltaTime * (sprintSpeed + playerSpeed));
                currentStamina--;
            }
           
        }
        else if(currentStamina == 0)
        {
            playerSprinting = false;
            controller.Move(move * Time.deltaTime * playerSpeed);
            StartCoroutine(sprintCooldown());
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
        //jumping
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
        }
        playerVelocity.y -= gravityModifier * Time.deltaTime;
    }

    public void weaponPickup(RangedWeapons stats)
    {
        shootRate = stats.fireRate;
        shootDist = stats.fireDistance;
        shootDmg = stats.damage;
        gunFireSound = stats.triggerSound;
        gunModel.GetComponent<MeshFilter>().sharedMesh = stats.designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = stats.designModel.GetComponent<MeshRenderer>().sharedMaterial;
        weaponListStats.Add(stats);
    }

    public void itemPickup(itemGrabs item)
    {
        grabbedPickup = true;
        //shootRate = shootRate/item.fireRate;
        shootDist += item.fireDistance;
        shootDmg += item.damage;
        jumpsMax += item.addJumps;
        sprintSpeed += item.addSpeed;
        ammoHeld += item.ammoCount;
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
      if(item.fireRate == 1)
        {
            isFireRateUp = true;
            shootRate /= shootRateUp;
            yield return new WaitForSeconds(10.00f);
            isFireRateUp = false;
            shootRate *= shootRateUp;
            grabbedPickup = false;
        }
      else if(item.damage == 1)
        {
            isDamageUp = true;
            shootDmg += shootDamageUp;
            yield return new WaitForSeconds(10.00f);
            shootDmg -= shootDamageUp;
            grabbedPickup = false;
            isDamageUp = false;

        }
      else if(item.fireDistance == 1)
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
            //shootRate = shootRate*item.fireRate;
            shootDist -= item.fireDistance;
            shootDmg -= item.damage;
            jumpsMax -= item.addJumps;
            sprintSpeed -= item.addSpeed;
            grabbedPickup = false;

        }
        
    }

    public void gunSelection()
    {
        if (weaponListStats.Count > 1)
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
       

        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponListStats[selectedGun].designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponListStats[selectedGun].designModel.GetComponent<MeshRenderer>().sharedMaterial;
    }
    public void UpdatePlayerHud()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / (float)HPOrig;
    }
    public void respawn()
    {
        controller.enabled = false;
        gameManager.instance.playerDeadMenu.SetActive(false);
        HP = HPOrig;
        UpdatePlayerHud();
        transform.position = gameManager.instance.spawnPosition.transform.position;
        controller.enabled = true;
    }
    IEnumerator sprintCooldown()
    {
        yield return new WaitForSeconds(5f);
        if (currentStamina != stamina)
        {
            currentStamina += stamina;
            if(currentStamina > stamina)
            {
                currentStamina = stamina;
            }
        }
    }
}
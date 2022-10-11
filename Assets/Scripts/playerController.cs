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
    
    [SerializeField] int jumpsMax;
    private int jumpCount;
    
    [Header("---Weapon Stats---")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int shootDmg;


    [SerializeField] List<RangedWeapons> weaponListStats = new List<RangedWeapons>();


    [Header("----- Gun Components -----")]
    [SerializeField] GameObject gunModel;

    int HPOrig;
    private Vector3 playerVelocity;
    public bool isShooting;
    [SerializeField] int selectedGun;

    private void Start()
    {
        HPOrig = HP;
        respawn();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        StartCoroutine(shoot());
        gunSelection();

    }
    //player shooting
    IEnumerator shoot()
    {
        if (weaponListStats.Count > 0 && Input.GetButton("Fire1") && !isShooting)
        {
            
            isShooting = true;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
            {
                if (hit.collider.GetComponent<iDamage>() != null)
                {
                    hit.collider.GetComponent<iDamage>().takeDamage(shootDmg);
                }
            }
            Debug.Log("Shot");
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
        Vector3 move = (transform.right * Input.GetAxis("Horizontal") + (transform.forward * Input.GetAxis("Vertical")));
        if (Input.GetButton("Sprint"))
        {
            controller.Move(move * Time.deltaTime * sprintSpeed);
        }
        else
        {
            controller.Move(move * Time.deltaTime * playerSpeed);
        }

        //jumping
        if (Input.GetButtonDown("Jump") && jumpCount < jumpsMax)
        {
            jumpCount++;
            playerVelocity.y = jumpHeight;
        }
        playerVelocity.y -= gravityModifier *Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void takeDamage(int dmg)
    {
        HP -= dmg;
        UpdatePlayerHud();
        StartCoroutine(gameManager.instance.playerDamage());
        if(HP <= 0)
        {
            Debug.Log("Player died");
        }
    }

    public void weaponPickup(RangedWeapons stats)
    {
        shootRate = stats.fireRate;
        shootDist = stats.fireDistance;
        shootDmg = stats.damage;
        gunModel.GetComponent<MeshFilter>().sharedMesh = stats.designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = stats.designModel.GetComponent<MeshRenderer>().sharedMaterial;
        weaponListStats.Add(stats);
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
        shootRate = weaponListStats[selectedGun].fireRate;
        shootDist = weaponListStats[selectedGun].fireDistance;
        shootDmg = weaponListStats[selectedGun].damage;

        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponListStats[selectedGun].designModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponListStats[selectedGun].designModel.GetComponent<MeshRenderer>().sharedMaterial;
    }
    void UpdatePlayerHud()
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
}

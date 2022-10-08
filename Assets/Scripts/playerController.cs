using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;

    [Header("----- Player Stats -----")]
    [Range(0, 10)][SerializeField] int HP;
    [Range(1, 5)] [SerializeField] float playerSpeed;
    [Range(8, 15)][SerializeField] float jumpHeight;
    [Range(15, 35)][SerializeField] float gravityValue;
    [Range(1, 3)][SerializeField] int jumpsMax;


    [Header("----- Gun Stats -----")]
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;
    [SerializeField] int shootDmg;
    [SerializeField] GameObject gunModel;
    [SerializeField] List<gunStats> gunStat = new List<gunStats>();

  

    

    private Vector3 playerVelocity;
    private int timesJumped;
    bool isShooting;
    int selectGun;
    int HPOrig;

    private void Start()
    {
        HPOrig = HP;
        respawn();
    }

    void Update()
    {
        movement();
        StartCoroutine(shoot());
        gunSelect();
    }
    void movement()
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            timesJumped = 0;
        }
        //first person
        Vector3 move = (transform.right * Input.GetAxis("Horizontal")) +
                       (transform.forward * Input.GetAxis("Vertical"));

        controller.Move(move * Time.deltaTime * playerSpeed);



        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && timesJumped < jumpsMax)
        {
            timesJumped++;
            playerVelocity.y = jumpHeight;
        }

        playerVelocity.y -= gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
    IEnumerator shoot()
    {
        if (gunStat.Count >0 && Input.GetButton("Shoot")  && !isShooting)
        {

            isShooting = true;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
            {
                if (hit.collider.GetComponent<IDamage>() != null)
                {
                    hit.collider.GetComponent<IDamage>().takeDamage(shootDmg);
                }

                //Instantiate(cube, hit.point, transform.rotation);
            }

            Debug.Log("Shoot!");
            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }
        
    }
    public void gunPickup(gunStats stats)
    {
        shootRate = stats.shootRate;
        shootDist = stats.shootDist;
        shootDmg = stats.shootDamage;
        gunModel.GetComponent<MeshFilter>().sharedMesh = stats.gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = stats.gunModel.GetComponent<MeshRenderer>().sharedMaterial;

        gunStat.Add(stats);
    }
    void gunSelect()
    {
        if (gunStat.Count > 1)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectGun < gunStat.Count - 1)
            {
                selectGun++;
                changeGun();
            }

            else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectGun > 0)
            {
                selectGun--;
                changeGun();
            }
        }
    }
    void changeGun()
    {
        shootRate = gunStat[selectGun].shootRate;
        shootDist = gunStat[selectGun].shootDist;
        shootDmg = gunStat[selectGun].shootDamage;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunStat[selectGun].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunStat[selectGun].gunModel.GetComponent<MeshRenderer>().sharedMaterial;

    }
    public void takeDamage(int dmg)
    {
        HP -= dmg;
        UpdatePlayerHud();
        StartCoroutine(gameManager.instance.playerDamage());

        if (HP <= 0)
        {
           gameManager.instance.playerDeadMenu.SetActive(true);
            gameManager.instance.cursorLockPause();
        }

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


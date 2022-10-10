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
    private Vector3 playerVelocity;
    [SerializeField] int jumpsMax;
    private int jumpCount;
    public bool isShooting;
    [Header("---Weapon Stats---")]
    [SerializeField] float shootRate;
    [SerializeField] float shootDist;
    [SerializeField] int shootDmg;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        StartCoroutine(shoot());

    }
    //player shooting
    IEnumerator shoot()
    {
        if (Input.GetButton("Fire1") && !isShooting)
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
        if(HP <= 0)
        {
            Debug.Log("Player died");
        }
    }
}

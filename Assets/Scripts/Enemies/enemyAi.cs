using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, iDamage
{
    [Header ("----- Components -----")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected AudioSource aud;
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject eyes;
    [SerializeField] protected GameObject drop;
    [SerializeField] Color shade;
    [SerializeField] List<GameObject> dropPool;
    [SerializeField] protected Collider hitBox;

    [Header("----- Enemy combat -----")]
    [Range(1, 50)][SerializeField] protected int hp;
    [SerializeField] bool isImportant;
    [SerializeField] bool forceDrop;
    [Range(.01f, 10)] [SerializeField] protected float attackRate;

    [Header("----- Movement/patrol stats -----")]
    [Range(1, 5)] [SerializeField] int facePlayerSpeed;
    [Range(1,50)][SerializeField] int sightDis;
    [Range(10, 90)] [SerializeField] float viewAngle;
    [Range(1, 10)] [SerializeField] int speedChase;
    [Range(0,10)] [SerializeField] int roamDis;
    [SerializeField] float animLerp;

    [Header("----- Audio -----")]
    [SerializeField] protected AudioClip hurtAud;
    [Range(0,1)] [SerializeField] protected float hurtVol;
    [SerializeField] AudioClip agroAud;
    [Range(0,1)] [SerializeField] float agroVol;
    [SerializeField] protected AudioClip attackAud;
    [Range(0,1)] [SerializeField] protected float attackVol;
    [SerializeField] protected AudioClip deathAud;
    [Range(0,1)] [SerializeField] protected float deathVol;
    [SerializeField] AudioClip scuttleAud;
    [Range(0,1)] [SerializeField] float scuttleVol;

    bool agro;
    bool playerInRange;
    bool lineOfSight;
    protected bool isAttacking;
    bool canShoot = true;
    float angle;
    float speedOriginal;
    float stoppingDis;
    Vector3 playerDir;
    Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        if (isImportant)
            gameManager.instance.enemySpawn();

        origin = transform.position;
        speedOriginal = agent.speed;
        stoppingDis = agent.stoppingDistance;
        model.material.color = shade;
        if(dropPool.Count > 0)
            drop = dropPool[Random.Range(0, dropPool.Count - 1)];
        roam();
    }

    // Update is called once per frame
    void Update()
    {
        if (hp > 0)
        {
            anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.magnitude / speedChase, Time.deltaTime * animLerp));

            playerDir = gameManager.instance.player.transform.position - eyes.transform.position;
            angle = Vector3.Angle(playerDir, transform.forward);

            lineOfSight = canSeePlayer();

            if (playerInRange || lineOfSight)
            {
                if (!agro)
                {
                    aud.PlayOneShot(agroAud, agroVol);
                    agent.speed = speedChase;
                    agro = true;
                }

                agent.stoppingDistance = stoppingDis;

                agent.SetDestination(gameManager.instance.player.transform.position);

                if (agent.remainingDistance < agent.stoppingDistance)
                    facePlayer();

                if (!isAttacking && canShoot)
                    StartCoroutine(attack());
            }
            else if (agent.remainingDistance < 0.1 && agent.destination != gameManager.instance.player.transform.position)
            {
                agro = false;
                roam();
            }
        }
    }

    void roam()
    {
        agent.stoppingDistance = 0;
        agent.speed = speedOriginal;

        Vector3 randomDir = Random.insideUnitSphere * roamDis;
        randomDir += origin;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, 1, 1);
        NavMeshPath path = new NavMeshPath();

        agent.CalculatePath(hit.position, path);
        agent.SetPath(path);
    }

    bool canSeePlayer()
    {
        RaycastHit hit;

        if (Physics.Raycast(eyes.transform.position, playerDir, out hit, sightDis))
        {
            if (hit.collider.CompareTag("Player") && angle <= viewAngle)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    void facePlayer()
    {
        playerDir.y = 0;
        Quaternion rotation = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * facePlayerSpeed);
    }

    public void takeDamage(int dmg)
    {
        hp -= dmg;

        if (hp <= 0)
        {
            anim.SetBool("Dead", true);
            aud.PlayOneShot(deathAud, deathVol);
            agent.enabled = false;
            hitBox.enabled = false;

            if(drop != null && forceDrop)
                Instantiate(drop, transform.position, transform.rotation);
            else
            {
                int temp = Random.Range(0, 2);
                if(temp == 0 && drop != null)
                    Instantiate(drop, transform.position, transform.rotation);
            }

            Destroy(gameObject, 10);

            if (isImportant)
                gameManager.instance.checkEnemyTotal();
        }
        else
        {

            agent.SetDestination(gameManager.instance.player.transform.position);

            StartCoroutine(flashDamage());
            aud.PlayOneShot(hurtAud, hurtVol);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            playerInRange= false;
    }

    public virtual IEnumerator attack()
    {
        isAttacking = true;
        Instantiate(bullet, eyes.transform.position, transform.rotation);
        aud.PlayOneShot(attackAud, attackVol);

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

    protected IEnumerator flashDamage()
    {
        model.material.color = Color.red;
        float returnSpeed = agent.speed;
        agent.speed = 0;
        canShoot = false;

        anim.SetTrigger("Hurt");
        yield return new WaitForSeconds(.6f);

        model.material.color = shade;
        agent.speed = returnSpeed;
        canShoot = true;
    }
}
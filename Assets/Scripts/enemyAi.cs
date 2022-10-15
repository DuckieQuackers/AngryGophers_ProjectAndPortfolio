using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAi : MonoBehaviour, iDamage
{
    [Header ("----- Components -----")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] GameObject bullet;
    public GameObject eyes;

    [Header("----- Enemy combat -----")]
    [Range(1, 50)][SerializeField] int hp;
    [Range(.01f, 10)] [SerializeField] float attackRate;

    [Header("----- Movement/patrol stats -----")]
    [Range(1, 5)] [SerializeField] int facePlayerSpeed;
    [Range(1,50)][SerializeField] int sightDis;
    [Range(10, 90)] [SerializeField] float viewAngle;
    [Range(1, 10)] [SerializeField] int speedChase;
    [Range(0,10)] [SerializeField] int roamDis;

    bool playerInRange;
    bool lineOfSight;
    bool isShooting;
    float angle;
    float speedOriginal;
    Vector3 playerDir;
    Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        gameManager.instance.enemySpawn();
        origin = transform.position;
        speedOriginal = agent.speed;
        roam();
    }

    // Update is called once per frame
    void Update()
    {

        playerDir = gameManager.instance.player.transform.position - eyes.transform.position;
        angle = Vector3.Angle(playerDir, transform.forward);

        lineOfSight = canSeePlayer();

        if (playerInRange || lineOfSight)
        {
            agent.speed = speedChase;

            agent.SetDestination(gameManager.instance.player.transform.position);

            if (agent.remainingDistance < agent.stoppingDistance)
                facePlayer();

            if (!isShooting)
                StartCoroutine(attack());
        }
        else if(agent.remainingDistance < 0.1 && agent.destination != gameManager.instance.player.transform.position)
        {
            roam();
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
        agent.SetDestination(gameManager.instance.player.transform.position);
        StartCoroutine(flashDamage());

        if (hp <= 0)
        {
            Destroy(gameObject);
            gameManager.instance.checkEnemyTotal();
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
        isShooting = true;
        Instantiate(bullet, eyes.transform.position, transform.rotation);
        yield return new WaitForSeconds(attackRate);
        isShooting = false;
    }

    IEnumerator flashDamage()
    {
        model.material.color = Color.red;
        agent.speed = 0;

        yield return new WaitForSeconds(0.2f);

        model.material.color = Color.white;
        agent.speed = speedOriginal;
    }
}

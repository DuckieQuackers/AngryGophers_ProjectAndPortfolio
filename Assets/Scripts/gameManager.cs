using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public int enemyNumber;

    [Header("----- Player Stuff -----")]
    public GameObject player;

    public playerController playerScript;
    public GameObject spawnPosition;

    [Header("----- UI -----")]
    public GameObject pauseMenu;
    public GameObject playerDeadMenu;
    public GameObject winMenu;
    public GameObject menuCurrentlyOpen;
    public GameObject playerDamageFlash;
    public Image playerHPBar;
    public TextMeshProUGUI enemyCountText;

    public bool isPaused;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        spawnPosition = GameObject.FindGameObjectWithTag("Spawn Position");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !playerDeadMenu.activeSelf && !winMenu.activeSelf)
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);

            if (isPaused)
            {
                cursorLockPause();
            }
            else
            {
                cursorUnlockUnpause();
            }
        }
    }

    public void cursorLockPause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void cursorUnlockUnpause()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public IEnumerator playerDamage()
    {
        playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        playerDamageFlash.SetActive(false);
    }

    public void checkEnemyTotal()
    {
        enemyNumber--;
        updateGameGoal();

        if (enemyNumber <= 0)
        {
            winMenu.SetActive(true);
            cursorLockPause();
        }
    }

    public void enemySpawn()
    {
        enemyNumber++;
        updateGameGoal();
    }

    public void updateGameGoal()
    {

        enemyCountText.text = "Enemies left: " + enemyNumber.ToString("F0");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AlienGameController : MonoBehaviour
{
    private Controls controls;

    public Transform UFO = null;

    public UFOBullet[] Bullets = null;

    public GameObject EnemyPrefab = null;
    public Transform EnemyParentTransform = null;

    public GameObject Boss = null;
    public SpriteRenderer BossSprite = null;

    public GameObject NonUFOUI = null;
    public GameObject PauseMessage = null;
    public GameObject GameOverMessage = null;
    public GameObject Retry = null;
    public GameObject Background = null;

    public float UFOMoveSpeed = 1;

    public AudioSource audioSource = null;
    public AudioClip bossDamageSound = null;
    public AudioClip bossDeathSound = null;
    public AudioClip laserShotSound = null;
    public AudioClip enemyDestroyedSound = null;

    private float enemiesKilled = 0;
    private float bossDamage = 0;

    private bool bossIsRed = false;
    private Color32 Red = new Color32(255, 0, 0, 255);
    private Color32 White = new Color32(255, 255, 255, 255);
    private float redTimer = 0;

    private Coroutine bossIsDead = null;

    private bool gameIsPaused = false;

    private const float _lengthToFlickerRed = .1f;

    private const float _maxLeftPos = -300;
    private const float _maxRightPos = 300;

    private const float _enemyCount = 150;
    private const float _enemyPerRow = 10;
    private const float _enemyXOffset = 55;
    private const float _enemyYOffset = 55;

    private const float _bossHealth = 325;

    void Awake()
    {
        controls = new Controls();
        controls.AlienMiniGame.Enable();
        controls.AlienMiniGame.Shoot.started += OnShoot;
        controls.AlienMiniGame.RemoteViewing.started += ToggleRemoteViewing;
        controls.AlienMiniGame.Pause.started += PauseGame;

        int enemiesInRow = 0;
        int rowCount = 0;
        for (int i = 0; i < _enemyCount; i++)
        {
            GameObject enemy = Instantiate(EnemyPrefab);
            enemy.transform.parent = EnemyParentTransform;
            enemy.transform.position = EnemyParentTransform.position;
            enemy.transform.position += new Vector3(_enemyXOffset * enemiesInRow, -_enemyYOffset * rowCount, 0);
            if (enemiesInRow++ == _enemyPerRow - 1)
            {
                enemiesInRow = 0;
                rowCount++;
            }
        }
    }

    void Update()
    {
        Vector2 move = controls.AlienMiniGame.Movement.ReadValue<Vector2>();
        if (move.x > 0)
        {
            UFO.position += new Vector3(UFOMoveSpeed * Time.deltaTime, 0, 0);
            if (UFO.position.x >= _maxRightPos) UFO.position = new Vector3(_maxRightPos, UFO.transform.position.y, UFO.transform.position.z);
        }
        else if (move.x < 0)
        {
            UFO.position -= new Vector3(UFOMoveSpeed * Time.deltaTime, 0, 0);
            if (UFO.position.x <= _maxLeftPos) UFO.position = new Vector3(_maxLeftPos, UFO.transform.position.y, UFO.transform.position.z);
        }

        if (Boss.activeSelf)
        {
            if (bossIsRed)
            {
                BossSprite.color = Red;
                bossIsRed = false;
                redTimer = 0;
            }
            else
            {
                redTimer += Time.deltaTime;
                if (redTimer >= _lengthToFlickerRed)
                {
                    BossSprite.color = White;
                }
            }
            
            bossIsRed = false;
        }
    }

    public void KillEnemy()
    {
        audioSource.PlayOneShot(enemyDestroyedSound);
        if (Boss.activeSelf) return;
        enemiesKilled++;
        Debug.Log("Enemies Killed: " + enemiesKilled);
        if (enemiesKilled >= _enemyCount)
        {
            Debug.Log("Boss Time!");
            Boss.SetActive(true);
        }
    }

    public void DamageBoss()
    {
        if (bossIsDead != null) return;

        bossDamage++;

        Debug.Log("Boss Damage Dealt " + bossDamage);
        
        if (bossDamage == _bossHealth)
        {
            bossIsDead = StartCoroutine(BossDeath());
        }
        else
        {
            bossIsRed = true;
            audioSource.PlayOneShot(bossDamageSound);
        }
    }

    private IEnumerator BossDeath()
    {
        audioSource.PlayOneShot(bossDeathSound);
        SpriteRenderer enemySprite = BossSprite.GetComponent<SpriteRenderer>();
        float timer = 0;
        Color32 red = new Color32(255, 0, 0, 255);
        Color32 noAlpha = new Color32(255, 0, 0, 0);
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            enemySprite.color = Color32.Lerp(red, noAlpha, timer / 1.5f);
            yield return null;
        }

        NonUFOUI.SetActive(true);
        NonUFOUI.GetComponentInChildren<EndingManager>().StartScene();
        gameObject.SetActive(false);
    }

    private void OnShoot(InputAction.CallbackContext ctx)
    {
        foreach(UFOBullet bullet in Bullets)
        {
            if (!bullet.BeingShot)
            {
                bullet.transform.position = UFO.position;
                bullet.transform.position += new Vector3(0, 25, 0);
                bullet.BeingShot = true;
                audioSource.PlayOneShot(laserShotSound);
                return;
            }
        }
    }

    private void ToggleRemoteViewing(InputAction.CallbackContext ctx)
    {
        Debug.Log("Toggled Remote Viewing");
    }

    public void PauseGame(InputAction.CallbackContext ctx)
    {
        if (GameOverMessage.activeSelf) return;

        if (gameIsPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Retry.SetActive(false);
            PauseMessage.SetActive(false);
            Background.SetActive(false);
            Time.timeScale = 1;
            gameIsPaused = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Retry.SetActive(true);
            PauseMessage.SetActive(true);
            Background.SetActive(true);
            Time.timeScale = 0;
            gameIsPaused = true;
        }
    }

    public void GameOver()
    {
        Debug.Log("You lost :(");

        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Retry.SetActive(true);
        Background.SetActive(true);
        GameOverMessage.SetActive(true);
    }

    public void ReloadScene()
    {
        Time.timeScale = 1; 
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }
}

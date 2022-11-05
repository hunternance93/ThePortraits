using UnityEngine;

public class UFOBoss : MonoBehaviour
{
    private const float _bossSpeed = 30;
    private const float _minHeight = 30;
    private const float _maxBounceHeight = 60;
    private const float _spawnTimerReductionRate = .5f;
    private const float _minSpawnTimer = 2;

    public GameObject ChargerEnemyPrefab = null;
    public Transform ChargerHolder = null;

    private const float _minChargerX = -300;
    private const float _maxChargerX = 300;

    private bool goingDown = true;

    private float timer = -5;
    private float spawnEnemyEvery = 7;

    void Update()
    {

        timer += Time.deltaTime;
        if (timer >= spawnEnemyEvery)
        {
            Debug.Log("Spawning enemy");
            GameObject newCharger = Instantiate(ChargerEnemyPrefab);
            newCharger.transform.parent = ChargerHolder;
            float xPos;
            if(Random.Range(0, 2) == 1) xPos = Random.Range(70, _maxChargerX);
            else xPos = Random.Range(_minChargerX, -70);
            newCharger.transform.position = new Vector3(xPos, ChargerHolder.transform.position.y, ChargerHolder.transform.position.z);

            timer = 0;
            spawnEnemyEvery -= _spawnTimerReductionRate;
            if (spawnEnemyEvery < _minSpawnTimer) spawnEnemyEvery = _minSpawnTimer;
        }

        if (goingDown)
        {
            transform.position -= new Vector3(0, _bossSpeed * Time.deltaTime, 0);
            if (transform.position.y <= _minHeight) goingDown = false;
        }
        else
        {
            transform.position += new Vector3(0, _bossSpeed * Time.deltaTime, 0);
            if (transform.position.y >= _maxBounceHeight) goingDown = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UFOEnemy : MonoBehaviour
{
    public SpriteRenderer EnemyImage = null;
    public Sprite AirisuSprite = null;
    public Sprite PosutaSprite = null;
    public Sprite MikaraSprite = null;
    public Sprite GuuhiSprite = null;

    private const float _enemySpeed = 75;
    private const float _distanceToMoveBeforeNext = 100;
    private const float _distanceToMoveDown = 20;
    private const float _increasedSpeedPerFloor = 2.5f;
    private const float _gameOverHeight = -120;
    private bool movingRight = true;
    private bool movingDown = false;
    private float currentDistanceToSide = 0;
    private float currentDistanceDown = 0;

    private float currentSpeed = _enemySpeed;

    private void Start()
    {
        SetSprite();
        gameObject.AddComponent(typeof(PolygonCollider2D));
    }

    private void SetSprite()
    {
        switch (Random.Range(1, 6))
        {
            case 1:
                EnemyImage.sprite = AirisuSprite;
                break;
            case 2:
                EnemyImage.sprite = PosutaSprite;
                break;
            case 3:
                EnemyImage.sprite = MikaraSprite;
                break;
            case 4:
                EnemyImage.sprite = GuuhiSprite;
                break;
            default:
                return;
        }
    }

    void Update()
    {
        int right = movingRight ? 1 : -1;
        int down = movingDown ? -1 : 0;

        transform.position += new Vector3(right * currentSpeed * Time.deltaTime, down * currentSpeed * Time.deltaTime, 0);
        currentDistanceToSide += currentSpeed * Time.deltaTime;
        if (movingDown) currentDistanceDown += currentSpeed * Time.deltaTime;
        if (currentDistanceToSide >= _distanceToMoveBeforeNext)
        {
            movingRight = !movingRight;
            movingDown = true;
            currentDistanceToSide = 0;
            currentSpeed += _increasedSpeedPerFloor;
        }
        if (currentDistanceDown >= _distanceToMoveDown)
        {
            movingDown = false;
            currentDistanceDown = 0;
        }
        if (transform.position.y <= _gameOverHeight) GameObject.Find("AlienGame").GetComponent<AlienGameController>().GameOver();
    }
}

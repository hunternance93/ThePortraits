using UnityEngine;

public class UFOChargerEnemy : MonoBehaviour
{
    public SpriteRenderer EnemyImage = null;
    public Sprite AirisuSprite = null;
    public Sprite PosutaSprite = null;
    public Sprite MikaraSprite = null;
    public Sprite GuuhiSprite = null;

    private const float _enemySpeed = 90;
    private const float _gameOverHeight = -130;

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
        transform.position -= new Vector3(0, _enemySpeed * Time.deltaTime, 0);

        if (transform.position.y <= _gameOverHeight) GameObject.Find("AlienGame").GetComponent<AlienGameController>().GameOver();
    }
}

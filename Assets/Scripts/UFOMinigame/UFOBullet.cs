using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOBullet : MonoBehaviour
{
    [HideInInspector] public bool BeingShot = false;

    private const float _maxHeight = 300;
    private const float _shootSpeed = 250;

    private AlienGameController AGC = null;

    private void Start()
    {
        AGC = GetComponentInParent<AlienGameController>();
    }

    void Update()
    {
        if (BeingShot)
        {
            transform.position += new Vector3(0, _shootSpeed * Time.deltaTime, 0);
            if (transform.position.y >= _maxHeight) EndShooting();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!other.gameObject.name.Contains("Overseer"))
            {
                //TODO: Sound Effect
                other.enabled = false;
                
                StartCoroutine(FadeOutAndDestroyEnemy(other.gameObject));
                EndShooting();
                AGC.KillEnemy();
            }
            else
            {
                EndShooting();
                AGC.DamageBoss();
            }
        }
    }

    private IEnumerator FadeOutAndDestroyEnemy(GameObject enemy)
    {
        SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
        float timer = 0;
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 noAlpha = new Color32(255, 255, 255, 0);
        while (timer < .25f)
        {
            timer += Time.deltaTime;
            enemySprite.color = Color32.Lerp(white, noAlpha, timer / .25f);
            yield return null;
        }
        Destroy(enemy);    
    }

    private void EndShooting()
    {
        BeingShot = false;
        transform.position = new Vector3(0, -500, 0);
    }
}

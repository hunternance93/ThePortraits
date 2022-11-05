using UnityEngine;
using UnityEngine.UI;

public class FlickerImage : MonoBehaviour
{
    private Image image = null;
    private const float _flickerTime = .5f;

    private float timer = 0;

    private void Start()
    {
        if (image == null) image = GetComponent<Image>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= _flickerTime)
        {
            image.enabled = !image.enabled;
            timer = 0;
        }
    }
}

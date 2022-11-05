using UnityEngine;

public class StartAudioAtRandomPoint : MonoBehaviour
{
    private void Awake()
    {
        AudioSource aud = GetComponent<AudioSource>();
        aud.time = UnityEngine.Random.Range(0, aud.clip.length);
    }
}

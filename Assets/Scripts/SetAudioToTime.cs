using UnityEngine;

public class SetAudioToTime : MonoBehaviour
{
    [SerializeField] private float startTime = 0;

    private void Awake()
    {
        GetComponent<AudioSource>().time = startTime;
    }
}

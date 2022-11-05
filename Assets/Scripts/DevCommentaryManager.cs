using UnityEngine;

public class DevCommentaryManager : MonoBehaviour
{
    public GameObject DevCommentaryPrefab;

    public float alternateVolumeLevel = -1;

    private const float _reduceAllVolume = .7f;

    void Start()
    {
        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.DevCommentary)
        {
            foreach (AudioSource audioSource in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
            {
                if (alternateVolumeLevel == -1) alternateVolumeLevel = _reduceAllVolume; 
                audioSource.volume *= alternateVolumeLevel;
            }

            Instantiate(DevCommentaryPrefab);
        }
    }
}

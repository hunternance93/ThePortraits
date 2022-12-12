using System.Collections;
using UnityEngine;

public class CreditsHandler : MonoBehaviour
{
    public GameObject Credits = null;

    void Start()
    {
        StartCoroutine(CreditsStartRoutine());
    }

    private IEnumerator CreditsStartRoutine()
    {
        yield return new WaitForSecondsRealtime(.5f);
        Credits.SetActive(true);
    }
}

using UnityEngine;

public class EnableDisableInteractable : MonoBehaviour, IInteractable
{
    public GameObject[] ObjectsToToggle = null;
    public string FindObjectNamed = "";

    private AudioSource aud = null;

    private void Start()
    {
        if (!string.IsNullOrEmpty(FindObjectNamed)) {
            ObjectsToToggle = new GameObject[1];
            ObjectsToToggle[0] = GameObject.Find(FindObjectNamed);
        }
        aud = GetComponent<AudioSource>();
    }

    public void Interacted()
    {
        if (aud != null) aud.Play();
        foreach(GameObject go in ObjectsToToggle)
        {
            if (go != null) go.SetActive(!go.activeSelf);
        }
    }
}

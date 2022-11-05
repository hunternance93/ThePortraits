using UnityEngine;

//For objects you only want in editor as a reference
public class DestroyOnStart : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject);
    }
}

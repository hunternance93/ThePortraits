using UnityEngine;

public class DestructibleInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Whether or not player needs an item in inventory to destroy this object")]
    [SerializeField] private bool requiresItem = false;
    [Tooltip("Whether or not the item is consumed on use")]
    [SerializeField] private bool removesItem = false;
    [Tooltip("Name of item required for player to destroy this object")]
    [SerializeField] private string item = "";
    [Tooltip("Message displayed if player does not have an item required to destroy object")]
    [SerializeField] private string failureMessage = "";
    [Tooltip("(Optional) Message displayed if player does not have the item required to destroy object")]
    [SerializeField] private string successMessage = "";
    [Tooltip("Object that will be destroyed from this interaction")]
    [SerializeField] private GameObject[] objectsToDestroy = null;
    [Tooltip("Object that will replace the destroyed object (debris, a particle effect, etc.)")]
    [SerializeField] private GameObject objectReplacement = null;
    [SerializeField] private float cameraShakeIntensity = 0;
    [SerializeField] private float cameraShakeDuration = 0;
    [SerializeField] private AudioSource[] audioToPlay = null;

    public void Interacted()
    {
        if (requiresItem && item != "")
        {
            if (GameManager.instance.Player.InventoryContains(item))
            {
                foreach(GameObject go in objectsToDestroy) Destroy(go);
                if (objectReplacement != null) objectReplacement.SetActive(true);
                if (!string.IsNullOrEmpty(successMessage)) GameManager.instance.DisplayMessage(successMessage);
                if (removesItem) GameManager.instance.Player.RemoveItem(item);
                HandleCameraShake();
                foreach (AudioSource aud in audioToPlay) {
                    if (!aud.isPlaying) aud.Play();
                }
            }
            else
            {
                GameManager.instance.DisplayMessage(failureMessage);
            }
        }
        else
        {
            foreach (GameObject go in objectsToDestroy) Destroy(go);
            if (objectReplacement != null) objectReplacement.SetActive(true);
            if (!string.IsNullOrEmpty(successMessage)) GameManager.instance.DisplayMessage(successMessage);
            HandleCameraShake();
            foreach (AudioSource aud in audioToPlay)
            {
                if (!aud.isPlaying) aud.Play();
            }
        }
    }

    private void HandleCameraShake()
    {
        if (cameraShakeDuration > 0 && cameraShakeIntensity > 0)
        {
            GameManager.instance.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);
        }
    }
}

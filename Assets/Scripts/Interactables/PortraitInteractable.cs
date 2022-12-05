using UnityEngine;

public class PortraitInteractable : MonoBehaviour, IInteractable
{
    public void Interacted()
    {
        PortraitManager.instance.HandlePortraitInteract();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GongInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private float cameraShakeIntensity = .5f;
    [SerializeField] private float cameraShakeDuration = 2.5f;
    [Tooltip("A sound effect that will play when this is interacted with")]
    [SerializeField] private AudioSource soundEffect = null;
    [Tooltip("List of heads affected by the gong.")]
    [SerializeField] private EnemyAI[] stunnableEnemies;
    [Tooltip("Message to be displayed if player clicks gong a second time")]
    [SerializeField] private string messageDisplayedAfterFirstInteract = "";

    private bool hasBeenRung = false;

    private void Start()
    {
        if (soundEffect == null && GetComponent<AudioSource>() != null) soundEffect = GetComponent<AudioSource>();
    }
    public void Interacted()
    {
        if (!hasBeenRung) {

            if (GameManager.instance.Player.InventoryContains("gong mallet"))
            {
                hasBeenRung = true;

                if (soundEffect != null) soundEffect.Play();
                GameManager.instance.RingGong(cameraShakeIntensity, cameraShakeDuration);
                StunEnemies();
                GameManager.instance.Player.RemoveItem("gong mallet");
            }
            else
            {
                GameManager.instance.DisplayMessage("I could ring this with that mallet in the corner of the room.");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(messageDisplayedAfterFirstInteract)) GameManager.instance.DisplayMessage(messageDisplayedAfterFirstInteract);
        }
    }

    private void StunEnemies()
    {
        foreach (EnemyAI enemy in stunnableEnemies)
        {
            if (!enemy.IsDisabledStory()) enemy.SetState(EnemyAI.EnemyState.Stunned);
        }
    }
}
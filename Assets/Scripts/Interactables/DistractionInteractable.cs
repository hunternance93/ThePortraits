using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Possible TODO: Add option for enemies to be distracted by proximity rather than manually assigned
public class DistractionInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("The enemy that will be affected by this distraction")]
    [SerializeField] private EnemyAI enemy = null;
    [Tooltip("The exact spot the enemy should go to observe a distraction")]
    [SerializeField] private Transform locationOfDistraction = null;
    [Tooltip("Where the enemy's gaze should land once they reach locationOfDistraction")]
    [SerializeField] private Transform distractionLookAtLocation = null;
    [Tooltip("How long the enemy should be distracted")]
    [SerializeField] private float distractionLength = 60;
    [Tooltip("A sound effect that will play when this is interacted with")]
    [SerializeField] private AudioSource soundEffect = null;
    [Tooltip("A cooldown if necessary so you can't spam the distraction, zero for none")]
    [SerializeField] private float cooldown;
    [SerializeField] private string cooldownMessage = "It's too risky to touch the bell now.";
        
    private float nextInteraction;

    private void Start()
    {
        if (locationOfDistraction == null) locationOfDistraction = transform;
        if (distractionLookAtLocation == null) distractionLookAtLocation = transform;
        if (soundEffect == null && GetComponent<AudioSource>() != null) soundEffect = GetComponent<AudioSource>();
        nextInteraction = 0;
    }

    public void Interacted()
    {
        if (!enemy.IsDisabledStory() && (enemy.GetState() == EnemyAI.EnemyState.Distracted || enemy.GetState() == EnemyAI.EnemyState.Chasing)) {
            GameManager.instance.DisplayMessage(cooldownMessage);
        }
        else if (Vector3.Distance(enemy.transform.position, locationOfDistraction.position) < 1)
        {
            GameManager.instance.DisplayMessage("I shouldn't ring it when it is so close.");
        }
        else if (Time.time > nextInteraction)
        {
            enemy.InvestigateDistraction(locationOfDistraction.position, distractionLookAtLocation.position, distractionLength);
            if (soundEffect != null) soundEffect.Play();
            nextInteraction = Time.time + cooldown;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("A unique name for the item being collected")]
    [SerializeField] private string item = "";
    [Tooltip("(Optional) A special message to display when collecting the item")]
    [SerializeField] private string customMessage = "";
    [Tooltip("(Optional) If there is an enemy here, it will aggro to the player when item is collected")]
    [SerializeField] private EnemyAI forcedEnemyAggro = null;
    [Tooltip("(Optional) If there are any audio sources here they will play when item is collected")]
    [SerializeField] private AudioSource[] audioToPlayOnLoot = null;
    [Tooltip("If this item should be saved even if player reloads scene / dies without reaching checkpoint")]
    [SerializeField] private bool saveItemOnCollect = false;
    [SerializeField] private bool makeKaedeSayHeavy = false;

    // Note this logic only runs on objects that have something "renderable". If possible: link this script to objects that contain a mesh (child objects do not count)
    // Otherwise, this logic will need to move to the "Update" method which would result in unnecessary calls every frame.
    private void OnWillRenderObject()
    {
        if (saveItemOnCollect && GameManager.instance.Player.InventoryContains(item) && gameObject != null)
            Destroy(gameObject);
    }

    public void Interacted()
    {
        GameManager.instance.Player.AddItem(item);
        if (string.IsNullOrEmpty(customMessage))
        {
            GameManager.instance.DisplayMessage("I collect the " + item + ".", GameManager.instance.defaultItemMessageLength);
        }
        else
        {
            GameManager.instance.DisplayMessage(customMessage, GameManager.instance.defaultItemMessageLength);
        }
        if (forcedEnemyAggro != null)
        {
            if (!forcedEnemyAggro.IsDisabledStory())
            {
                forcedEnemyAggro.lastKnownTargetPosToBeUpdated = GameManager.instance.Player.transform.position;
                forcedEnemyAggro.lastKnownTargetPos = GameManager.instance.Player.transform.position;
                forcedEnemyAggro.ForceChasing = true;
                forcedEnemyAggro.SetState(EnemyAI.EnemyState.Chasing);
            }
        }
        if (audioToPlayOnLoot != null)
        {
            foreach(AudioSource aud in audioToPlayOnLoot)
            {
                aud.Play();
            }
        }
        if (saveItemOnCollect) GameManager.instance.SaveGameManager.SaveInventory();
        if (item.ToLower().Contains("coin"))
        {
            if (!GameManager.instance.Player.PlayerHasMoreThanOneCoin())
            {
                KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.WhatIsThis);
            }
        }
        else if (makeKaedeSayHeavy) KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.Heavy);
        Destroy(gameObject);
    }
}

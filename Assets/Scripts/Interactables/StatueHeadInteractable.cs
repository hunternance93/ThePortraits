using System.Collections;
using UnityEngine;

public class StatueHeadInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string statueHeadName = "missing statue head";
    [Tooltip("Message displayed when interacted")]
    [SerializeField] private string message = "";
    [SerializeField] private string messageAfterMove = "The statue moved after I added that creepy head";
    [Tooltip("(Optional) How long message should display for")]
    [SerializeField] private float messageLength = -1;
    [SerializeField] private float distanceToMoveStatue = -3;
    [SerializeField] private float timeToMoveStatue = 5;
    [SerializeField] private AudioSource[] soundsToPlayOnMove = null;
    [SerializeField] private GameObject[] objectsToEnable = null;

    private bool hasMoved = false;

    public void Interacted()
    {
        if (!hasMoved && GameManager.instance.Player.InventoryContains(statueHeadName))
        {
            foreach(GameObject go in objectsToEnable) go.SetActive(true);
            foreach (AudioSource audio in soundsToPlayOnMove) audio.Play();
            StartCoroutine(MoveStatue());
            GameManager.instance.Player.RemoveItem(statueHeadName);
        }
        else
        {
            if (!hasMoved)
            {
                if (messageLength < 0) GameManager.instance.DisplayMessage(message);
                else GameManager.instance.DisplayMessage(message, messageLength);
            }
            else
            {
                GameManager.instance.DisplayMessage(messageAfterMove);
            }
        }
    }

    private IEnumerator MoveStatue()
    {
        hasMoved = true;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new Vector3(startPos.x, startPos.y, startPos.z + distanceToMoveStatue);
        float timer = 0;
        while (timer < timeToMoveStatue)
        {
            timer += Time.deltaTime;
            transform.localPosition = new Vector3(targetPos.x, targetPos.y, Mathf.Lerp(startPos.z, targetPos.z, timer / timeToMoveStatue));
            yield return null;
        }
        transform.localPosition = targetPos;
    }
}

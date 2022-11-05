using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Name of the scene to load")]
    [SerializeField] private string sceneToLoad = "";
    [Tooltip("If it requires an item in inventory to open")]
    [SerializeField] private bool requiresItem = false;
    [Tooltip("Item required to open door")]
    [SerializeField] private string item = "";
    [Tooltip("Message displayed if player does not have item to open door")]
    [SerializeField] private string failureMessage = "";
    [Tooltip("How long failure messages should display")]
    [SerializeField] private float messageLength = 1.5f;
    [Tooltip("If there should be a fade out before loading")]
    [SerializeField] private bool fadeOut = false;
    [Tooltip("How long the fadeout should be")]
    [SerializeField] private float fadeOutLength = .5f;
    [Tooltip("Image to fade")]
    [SerializeField] private CanvasFadeOut canvasFadeOut = null;
    [Tooltip("All enemies in this list must be stunned to open the door, if no enemies need to be stunned leave empty")]
    [SerializeField] private EnemyAI[] enemiesThatMustBeStunned = null;
    [Tooltip("Fail message if there are enemies that need to be stunned before opening it")]
    [SerializeField] private string failMessageIfEnemiesNotStunned = "";
    [Tooltip("Items to remove on load to clear up space in inventory")]
    [SerializeField] private string[] itemsToRemoveFromInventoryOnSceneLoad = null;

    private bool isLoading = false;

    public void Interacted()
    {
        if (isLoading) return;
        if (requiresItem)
        {
            if (GameManager.instance.Player.InventoryContains(item))
            {
                if (enemiesThatMustBeStunned != null)
                {
                    foreach (EnemyAI enemy in enemiesThatMustBeStunned)
                    {
                        if (enemy.GetState() != EnemyAI.EnemyState.Stunned)
                        {
                            GameManager.instance.DisplayMessage(string.IsNullOrEmpty(failMessageIfEnemiesNotStunned) ? "It's not safe to open the door without dealing with those monsters first" : failMessageIfEnemiesNotStunned, messageLength);
                            return;
                        }
                    }
                }
                LoadScene();
            }
            else
            {
                GameManager.instance.DisplayMessage(string.IsNullOrEmpty(failureMessage) ? "You need " + item + " to open." : failureMessage, messageLength);
            }
        }
        else
        {
            if (enemiesThatMustBeStunned != null)
            {
                foreach (EnemyAI enemy in enemiesThatMustBeStunned)
                {
                    if (enemy.GetState() != EnemyAI.EnemyState.Stunned)
                    {
                        GameManager.instance.DisplayMessage(string.IsNullOrEmpty(failureMessage) ? "It's not safe to open the door without dealing with those monsters first." : failMessageIfEnemiesNotStunned, messageLength);
                        return;
                    }
                }
            }
            LoadScene();
        }
    }

    private void LoadScene()
    {
        isLoading = true;
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        AudioManager.instance.StopPlayerMovementAudio();
        foreach (string item in itemsToRemoveFromInventoryOnSceneLoad)
        {
            if (!string.IsNullOrEmpty(item)) GameManager.instance.Player.RemoveItem(item);
        }
        GameManager.instance.SaveGameManager.SaveGameScene(sceneToLoad);
        if (!fadeOut)
        {
            GameManager.instance.UpdatePlaytime();
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (GameManager.instance.CurrentGameMode != GameManager.GameMode.Hardcore) GameManager.instance.DisplaySaveText();
        GameManager.instance.DisplayLoadingText();
        GameManager.instance.DisableTipAndInspect();
        StartCoroutine(canvasFadeOut.FadeOut(fadeOutLength));
        yield return new WaitForSeconds(fadeOutLength);
        canvasFadeOut.gameObject.SetActive(true);
        GameManager.instance.UpdatePlaytime();
        GameManager.instance.SaveGameManager.SaveGameScene(sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}

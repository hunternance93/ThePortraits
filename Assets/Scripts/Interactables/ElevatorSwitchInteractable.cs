using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorSwitchInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("A sound effect that will play when this is interacted with")]
    [SerializeField] private AudioSource soundEffect = null;
    private Animation anim;
    [Tooltip("Used to apply an invisible wall so player doesn't leave elevator")]
    [SerializeField] private GameObject[] gameObjectsToActivate = null;
    [SerializeField] private bool onlyActivateOnce = true;
    [SerializeField] private string sceneToLoad = "";
    [SerializeField] private float delayBeforeLoadScene = 10;
    [SerializeField] private float elevatorSpeed = 3;
    [SerializeField] private GameObject[] objectsToMove = null;
    [SerializeField] private float delayBeforeElevatorMove = 3;
    [SerializeField] private float fadeOutLength = .5f;
    [SerializeField] private CanvasFadeOut canvasFadeOut = null;
    [SerializeField] private Animator elevatorDoorAnim = null;
    [SerializeField] private float elevatorDoorSoundDelay = 0;
    [SerializeField] private AudioSource elevatorDoorSound = null;

    private void Start()
    {
        anim = GetComponent<Animation>();

        if (soundEffect == null && GetComponent<AudioSource>() != null) soundEffect = GetComponent<AudioSource>();
    }

    public void Interacted()
    {
        if (onlyActivateOnce)
        {
            foreach (Collider col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }
        anim.Play();
        if (soundEffect != null) soundEffect.Play();
        if (objectsToMove != null) StartCoroutine(ElevatorMovement());
        foreach (GameObject go in gameObjectsToActivate)
        {
            go.SetActive(true);
        }

        elevatorDoorAnim.SetBool("IsOpening", false);
        StartCoroutine(WaitAndThenPlayAudio());
    }

    private IEnumerator WaitAndThenPlayAudio()
    {
        yield return new WaitForSeconds(elevatorDoorSoundDelay);
        elevatorDoorSound.Play();
    }

    private IEnumerator ElevatorMovement()
    {
        yield return new WaitForSeconds(delayBeforeElevatorMove);
        if (!string.IsNullOrEmpty(sceneToLoad)) StartCoroutine(FadeOutAndLoad());
        while (true)
        {
            foreach (GameObject go in objectsToMove)
            {
                go.transform.position -= new Vector3(0, elevatorSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        yield return new WaitForSeconds(delayBeforeLoadScene);
        canvasFadeOut.gameObject.SetActive(true);
        StartCoroutine(canvasFadeOut.FadeOut(fadeOutLength));
        if (GameManager.instance.CurrentGameMode != GameManager.GameMode.Hardcore) GameManager.instance.DisplaySaveText();
        GameManager.instance.DisplayLoadingText();
        yield return new WaitForSeconds(fadeOutLength);
        GameManager.instance.DisableTipAndInspect();
        GameManager.instance.UpdatePlaytime();
        LoadScene();
    }

    private void LoadScene()
    {
        GameManager.instance.SaveGameManager.SaveGameScene(sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
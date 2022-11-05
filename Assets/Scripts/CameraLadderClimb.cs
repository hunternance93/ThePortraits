using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraLadderClimb : MonoBehaviour
{
    [SerializeField] private Vector3 headMovementPerStep = new Vector3(0, .35f, .04f);
    [SerializeField] private float numberOfSteps = 3;
    [SerializeField] private float stepLength = .2f;
    [SerializeField] private float lengthBetweenSteps = .1f;
    [SerializeField] private CanvasFadeOut fade = null;
    [SerializeField] private Transform spawnPointAfterLadder;
    [SerializeField] private AudioListener ladderCamAudioListener;

    private Vector3 initPos;

    private void OnEnable()
    {
        StartCoroutine(CameraLadderClimbAnim());
    }

    private IEnumerator CameraLadderClimbAnim()
    {
        ladderCamAudioListener.enabled = true;
        initPos = transform.position;

        GameManager.instance.mainCamera.gameObject.SetActive(false);
        GameManager.instance.Player.gameObject.transform.SetPositionAndRotation(spawnPointAfterLadder.position, GameManager.instance.Player.gameObject.transform.rotation);

        yield return StartCoroutine(fade.FadeIn());

        for (int i = 0; i < numberOfSteps; i++)
        {
            float timer = 0;
            Vector3 startPos = transform.position;
            Vector3 tar = startPos + headMovementPerStep;
            while (timer < stepLength)
            {
                transform.position = Vector3.Lerp(startPos, tar, timer/stepLength);
                timer += Time.deltaTime;
                yield return null;
            }
            transform.position = tar;

            if (numberOfSteps -1 != i) yield return new WaitForSeconds(lengthBetweenSteps);
        }

        yield return StartCoroutine(fade.FadeOut());
        StartCoroutine(fade.FadeIn());
        ladderCamAudioListener.enabled = false;
        GameManager.instance.Player.gameObject.SetActive(true);
        GameManager.instance.mainCamera.gameObject.SetActive(true);
        GameManager.instance.Player.FPSController.PreventInteractableFor(1);

        gameObject.transform.SetPositionAndRotation(initPos, transform.rotation);
        yield return new WaitForSeconds(fade.FadeLength);
        GameManager.instance.Player.CanSightJack = true;
        gameObject.SetActive(false);
    }
}

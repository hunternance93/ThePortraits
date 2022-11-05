using Cinemachine;
using System.Collections;
using UnityEngine;

public class InGameEndingCutscene : MonoBehaviour
{
    public CinemachineVirtualCamera FinalSceneCam = null;
    public CinemachineVirtualCamera WalkCam = null;
    public Transform MeioTransform = null;
    public Transform CameraPOV = null;
    public GameObject CinematicKaede = null;
    public Color32 TrueColor;
    public Color32 NormalColor;
    public Light HeirloomGlow = null;
    public float StartingIntensity = 0;
    public float FinalIntensity = 100;
    public float StartingRange = 0;
    public float FinalRange = 100;
    public float LengthOfShine = 10;
    public AudioSource[] audioToQuiet = null;

    public IEnumerator EndOfGameCutscene()
    {
        if (GameManager.instance.Player.PCS.StunCount < 1 && (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore || GameManager.instance.CurrentGameMode == GameManager.GameMode.Normal))
        {
        }

        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        AudioManager.instance.StopPlayerMovementAudio();
        WalkCam.LookAt = MeioTransform;
        FinalSceneCam.LookAt = MeioTransform;
        FinalSceneCam.Priority = 999;
        yield return new WaitForSeconds(3);
        CinematicKaede.SetActive(true);
        HeirloomGlow.color = GameManager.instance.Player.InventoryContains("True Family Heirloom") ? TrueColor : NormalColor;
        yield return StartCoroutine(GameManager.instance.Player.PCS.ForceMeioSightjack(CameraPOV));
        HeirloomGlow.intensity = StartingIntensity;
        yield return new WaitForSeconds(1.5f);
        float timer = 0;
        while (timer < LengthOfShine)
        {
            HeirloomGlow.intensity = Mathf.Lerp(StartingIntensity, FinalIntensity, timer / LengthOfShine);
            HeirloomGlow.range = Mathf.Lerp(StartingRange, FinalRange, timer / LengthOfShine);
            timer += Time.deltaTime;
            yield return null;
        }
        if (GameManager.instance.Player.InventoryContains("True Family Heirloom"))
        {
            GameManager.instance.Player.TrueHeirloomFlash.SetPartiallyTransparent(false);
            yield return StartCoroutine(GameManager.instance.Player.TrueHeirloomFlash.FadeIn(1.5f));
        }
        else
        {
            GameManager.instance.Player.NormalHeirloomFlash.SetPartiallyTransparent(false);
            yield return StartCoroutine(GameManager.instance.Player.NormalHeirloomFlash.FadeIn(1.5f, true));
        }

        timer = 0;
        while (timer < 3)
        {
            foreach(AudioSource aud in audioToQuiet)
            {
                aud.volume = Mathf.Lerp(1, 0, timer / 3);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }
}

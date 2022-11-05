using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCameraSwitcher : MonoBehaviour
{
    public GameObject[] GhostPOVs;
    public CinemachineVirtualCamera SightjackCamera;
    public CinemachineVirtualCamera walkCamera;
    public float SightjackDelay = 0.3f;
    public float InitSightjackDelay = 0.5f;

    [SerializeField] private SkinnedMeshRenderer[] playerSkinnedMeshRenderers = null;
    [SerializeField] private SkinnedMeshRenderer[] crouchedSkinnedMeshRenderers = null;
    [SerializeField] private AudioListener playerAudioListener = null;
    public GameObject BlinkObject = null;
    public float BlinkObjectTargetHeight = 160;
    public float interstitialLength = 1.5f;

    public GameObject StunAbilityOverlay = null;
    public GameObject AccessibleStunAbilityOverlay = null;
    
    public GameObject CurrentStunAbilityOverlay = null;
    
    
    public AudioSource OverseerSightjackNoise = null;

    private GameObject CurrentSightjackOverlay = null;
    
    private int currentGhostCam = 0;

    private Quaternion originalRotation;

    private float blinkObjInitHeight;

    private Coroutine sightjackRoutine = null;
    private Coroutine interstitialRoutine = null;

    [HideInInspector] public int StunCount = 0;
    [HideInInspector] public bool CurrentSightjackIsOverseer = false;

    private void Start()
    {
        blinkObjInitHeight = BlinkObject.transform.position.y;
        SetBothMeshRenderers(false);
    }

    private void Awake()
    {
        GameManager.instance.controls.PlayerControl.Sightjack.performed += SightjackOnperformed;
        GameManager.instance.controls.Sightjacking.ExitSightjacking.performed += ExitSightjackingOnperformed;
        GameManager.instance.controls.Sightjacking.NextGhostCamera.performed += NextGhostCameraOnperformed;
        GameManager.instance.controls.Sightjacking.ScrollGhostCamera.performed += ScrollGhostView;

        GameManager.instance.controls.Sightjacking.SetGhostCamera1.performed += context => SetGhostCamera(1);
        GameManager.instance.controls.Sightjacking.SetGhostCamera2.performed += context => SetGhostCamera(2);
        GameManager.instance.controls.Sightjacking.SetGhostCamera3.performed += context => SetGhostCamera(3);
        GameManager.instance.controls.Sightjacking.SetGhostCamera4.performed += context => SetGhostCamera(4);
        GameManager.instance.controls.Sightjacking.SetGhostCamera5.performed += context => SetGhostCamera(5);
        GameManager.instance.controls.Sightjacking.SetGhostCamera6.performed += context => SetGhostCamera(6);
        GameManager.instance.controls.Sightjacking.SetGhostCamera7.performed += context => SetGhostCamera(7);
        GameManager.instance.controls.Sightjacking.SetGhostCamera8.performed += context => SetGhostCamera(8);
        GameManager.instance.controls.Sightjacking.SetGhostCamera9.performed += context => SetGhostCamera(9);
        GameManager.instance.controls.Sightjacking.SetGhostCamera10.performed += context => SetGhostCamera(10);

        GameManager.instance.controls.Sightjacking.StunEnemy.performed += context => StunTarget();
    }

    private void SetCurrentSightjackOverlay()
    {
        CurrentSightjackOverlay = GameManager.instance.GetReduceFlashing() ? GameManager.instance.AccessibleSightjackOverlay : GameManager.instance.SightjackOverlay;
    }
    
    private void SetCurrentStunAbilityOverlay()
    {
        CurrentStunAbilityOverlay = GameManager.instance.GetReduceFlashing() ? AccessibleStunAbilityOverlay : StunAbilityOverlay;
    }

    private void SetGhostCamera(int cameraNum)
    {
        if (GhostPOVs.Length < cameraNum || currentGhostCam == cameraNum - 1 || GameManager.instance.GameEnding.IsGameOver) return;
        SetGhostCam(cameraNum - 1);
        AudioManager.instance.PlaySightJackChange();
        sightjackRoutine = StartCoroutine(SightjackCameraChange());
    }

    private void ScrollGhostView(InputAction.CallbackContext obj)
    {
        float val = obj.ReadValue<float>();
        if (val < 0) NextGhostCameraOnperformed(obj);
        else PrevGhostCameraOnperformed(obj);
    }

    private void NextGhostCameraOnperformed(InputAction.CallbackContext obj)
    {
        if (GhostPOVs.Length > 1 && !GameManager.instance.GameEnding.IsGameOver)
        {
            IncrementGhostCam();
            AudioManager.instance.PlaySightJackChange();
            sightjackRoutine = StartCoroutine(SightjackCameraChange());
        }
    }

    private void PrevGhostCameraOnperformed(InputAction.CallbackContext obj)
    {
        if (GhostPOVs.Length > 1 && !GameManager.instance.GameEnding.IsGameOver)
        {
            DecrementGhostCam();
            AudioManager.instance.PlaySightJackChange();
            sightjackRoutine = StartCoroutine(SightjackCameraChange());
        }
    }

    private bool SightjackingActionPossible()
    {
        if (Time.timeScale == 0 || !GameManager.instance.Player.CanSightJack) return false;
        if (sightjackRoutine != null) return false;
        if (interstitialRoutine != null) return false;
        if (!GameManager.instance.Player.FPSController.IsGrounded()) return false;
        return true;
    }

    private void ExitSightjackingOnperformed(InputAction.CallbackContext obj)
    {
        if (!SightjackingActionPossible()) return;

        sightjackRoutine = StartCoroutine(EndSightjacking());
    }

    private void SightjackOnperformed(InputAction.CallbackContext obj)
    {
        if (!SightjackingActionPossible() || GhostPOVs.Length == 0) return;

        AudioManager.instance.StopPlayerMovementAudio();

        try {
            EnemyAI enemy = GhostPOVs[currentGhostCam].GetComponentInParent<EnemyAI>();
            if (enemy == null || enemy.AnglerEnemy) GameManager.instance.SetSightJacking(false, 0, true);
            else GameManager.instance.SetSightJacking(enemy.GetState() == EnemyAI.EnemyState.Stunned, InitSightjackDelay);

            if (enemy == null) OverseerSightjackNoise.Play();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            GameManager.instance.SetSightJacking();
        }

        sightjackRoutine = StartCoroutine(SightjackInitial());
        SetPlayerMeshRenderers(true);
    }

    public void InstantlyEndSightjacking()
    {
        if (GhostPOVs.Length <= 0) return;
        StopAllCoroutines();
        sightjackRoutine = null;
        interstitialRoutine = null;
        BlinkObject.transform.position = new Vector3(BlinkObject.transform.position.x, blinkObjInitHeight, BlinkObject.transform.position.z);
        BlinkObject.SetActive(false);
        if (OverseerSightjackNoise != null) OverseerSightjackNoise.Stop();
        if (CurrentStunAbilityOverlay != null) CurrentStunAbilityOverlay.SetActive(false);
        GameManager.instance.EndSightJacking(true);
        if (CurrentSightjackOverlay != null)
        {
            CurrentSightjackOverlay.SetActive(false);
        }
        DisableAllStunnedOverlays();
        SightjackCamera.Priority = 5;
        foreach(GameObject enemyPOV in GhostPOVs)
        {
            enemyPOV.GetComponent<AudioListener>().enabled = false;
        }
        SetPlayerMeshRenderers(false);
        playerAudioListener.enabled = true;
        GameManager.instance.Player.CanSightJack = true;
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
    }

    public void SetGhostPOVs(GameObject[] newCams)
    {
        if (GameManager.instance.CurrentGameMode != GameManager.GameMode.Story && GameManager.instance.CurrentGameMode != GameManager.GameMode.DevCommentary)
        {
            GhostPOVs = newCams;
        }
        else
        {
            List<GameObject> tempCams = new List<GameObject>();
            EnemyAI tempEnemy;
            foreach(GameObject go in newCams)
            {
                tempEnemy = go.GetComponentInParent<EnemyAI>();
                if (!(tempEnemy != null && tempEnemy.IsDisabledStory())) tempCams.Add(go);
            }
            if (tempCams.Count > 0)
            {
                GhostPOVs = tempCams.ToArray();
            }
            else GhostPOVs = new GameObject[0];
        }
    }

    public void ResetGhostCamIndex()
    {
        currentGhostCam = 0;
    }

    public void ShowVision(GameObject visionCam, float extraBuildUpToVision = 0, bool empoweredHeirloomScene = false)
    {
        interstitialRoutine = StartCoroutine(BrieflySwitchTo(visionCam, extraBuildUpToVision, empoweredHeirloomScene));
    }

    private IEnumerator BrieflySwitchTo(GameObject cam, float extraBuildupTime = 0, bool empoweredHeirloomScene = false)
    {
        AudioManager.instance.StopPlayerMovementAudio();
        GameManager.instance.Player.isSightJacking = true;
        GameManager.instance.Player.FPSController.TransitionToWalkingCamera();
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        if (extraBuildupTime > 0)
        {
            GameManager.instance.DisplayMessage("My head... I'm getting another vision.", extraBuildupTime - 2f);
            KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.MyHead);
            yield return new WaitForSeconds(extraBuildupTime);
        }
        AudioManager.instance.PlayHeartBeat();

        GameManager.instance.DisableTipAndInspect();
        cam.SetActive(true);
        yield return new WaitForSeconds(interstitialLength);
        cam.SetActive(false);
        GameManager.instance.Player.isSightJacking = false;
        if (empoweredHeirloomScene)
        {
            yield return new WaitForSeconds(.5f);
            if (GameManager.instance.Player.InventoryContains("True Family Heirloom"))
            {
                yield return StartCoroutine(GameManager.instance.Player.TrueHeirloomFlash.FlashScreen(.5f));
            }
            else
            {
                yield return StartCoroutine(GameManager.instance.Player.NormalHeirloomFlash.FlashScreen(.5f));
            }
            yield return new WaitForSeconds(1);
            GameManager.instance.DisplayMessage("The heirloom is glowing brightly... now that I am far enough away from the town, maybe I can channel its powers to help me escape?", 6);
            yield return new WaitForSeconds(5);
            GameManager.instance.DisplayProTip("Tip: Unleash the power of the heirloom on an enemy by pressing Space while gazing through their eyes.", 15);
        }
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
        interstitialRoutine = null;
    }

    public IEnumerator ForceMeioSightjack(Transform target)
    {
        GameManager.instance.SetSightJacking(false, 0, true);
        SetCurrentSightjackOverlay();
        CurrentSightjackOverlay.SetActive(true);
        yield return null;
        SightjackCamera.Priority = 1000;
        yield return new WaitForSeconds(InitSightjackDelay);
        CurrentSightjackOverlay.SetActive(false);
        SightjackCamera.Follow = target.transform;
        playerAudioListener.enabled = false;
        GameManager.instance.SetSightJackingVisuals();
        //GameManager.instance.SetOverseerEffects();
        target.GetComponent<AudioListener>().enabled = true;
    }

    public IEnumerator ForceSightjack(float length, GameObject pov, float extraBuildUpTime = 0)
    {
        AudioManager.instance.StopPlayerMovementAudio();
        GameManager.instance.Player.CanSightJack = false;
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());

        if (extraBuildUpTime > 0)
        {
            GameManager.instance.DisplayMessage("Damnit, it is happening again...", extraBuildUpTime - 1);
            KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.HeadacheGroan);
            yield return new WaitForSeconds(extraBuildUpTime);
        }

        GameManager.instance.SetSightJacking();
        SetCurrentSightjackOverlay();
        CurrentSightjackOverlay.SetActive(true);

        originalRotation = CinemachineCore.Instance.GetActiveBrain(0).CurrentCameraState.FinalOrientation;

        GameManager.instance.Player.FPSController.TransitionToWalkingCamera();
        yield return null;
        SightjackCamera.Priority = 50;
        yield return new WaitForSeconds(InitSightjackDelay);
        CurrentSightjackOverlay.SetActive(false);

        SetPlayerMeshRenderers(true);

        SightjackCamera.Follow = pov.transform;
        playerAudioListener.enabled = false;

        GameManager.instance.SetSightJackingVisuals();

        pov.GetComponent<AudioListener>().enabled = true;
        yield return new WaitForSeconds(length);

        GameManager.instance.EndSightJacking();
        SetCurrentSightjackOverlay();
        CurrentSightjackOverlay.SetActive(true);

        SightjackCamera.Priority = 5;
        
        yield return new WaitForSeconds(InitSightjackDelay);
        walkCamera.GetCinemachineComponent<CinemachinePOV>().ForceCameraPosition(Vector3.zero, originalRotation);
        yield return null;

        CurrentSightjackOverlay.SetActive(false);

        SetPlayerMeshRenderers(false);

        pov.GetComponent<AudioListener>().enabled = false;
        playerAudioListener.enabled = true;

        GameManager.instance.Player.CanSightJack = true;
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
    }

    private IEnumerator SightjackCameraChange()
    {
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        bool blinkObjWasActive = BlinkObject.activeSelf;
        BlinkObject.SetActive(true);

        Vector3 initPos = new Vector3(BlinkObject.transform.position.x, blinkObjInitHeight, BlinkObject.transform.position.z);
        Vector3 targetPos = new Vector3(BlinkObject.transform.position.x, BlinkObjectTargetHeight, BlinkObject.transform.position.z);

        float timer = 0;
        EnemyAI enemy = GhostPOVs[currentGhostCam].GetComponentInParent<EnemyAI>();

        if (!blinkObjWasActive)
        {
            BlinkObject.transform.position = initPos;

            do
            {
                timer += Time.deltaTime;
                BlinkObject.transform.position = Vector3.Lerp(initPos, targetPos, timer / (SightjackDelay / 2));
                yield return null;
            } while (timer < SightjackDelay / 2);

        }
        BlinkObject.transform.position = targetPos;

        DisableGhostCams();
        SightjackCamera.Follow = GhostPOVs[currentGhostCam].transform;
        GhostPOVs[currentGhostCam].GetComponent<AudioListener>().enabled = true;


        DisableAllStunnedOverlays();

        GameManager.instance.SetSightJackingVisuals();

        if (enemy == null)
        {
            CurrentSightjackIsOverseer = true;
            GameManager.instance.SetOverseerEffects();
        }
        else
        {
            CurrentSightjackIsOverseer = false;
            GameManager.instance.EndOverseerEffects();
        }

        if (OverseerSightjackNoise != null && OverseerSightjackNoise.isPlaying) OverseerSightjackNoise.Stop();

        if (enemy != null && enemy.GetState() == EnemyAI.EnemyState.Stunned)
        {
            AudioManager.instance.PlaySightJackAmbience(true, 0);
            if (GameManager.instance.StunnedOverlay != null)
            {
                GameManager.instance.StunnedOverlay[currentGhostCam % GameManager.instance.StunnedOverlay.Length].SetActive(true);
            }
        }
        else if (enemy != null)
        {
            AudioManager.instance.PlaySightJackAmbience();
        }
        else
        {
            OverseerSightjackNoise.Play();
            AudioManager.instance.PauseSightJackAmbience();
            AudioManager.instance.PauseStunnedSightJackAmbience();
        }

        if (enemy == null || !enemy.IsCurrentlySleeping)
        {
            timer = 0;
            do
            {
                timer += Time.deltaTime;
                BlinkObject.transform.position = Vector3.Lerp(targetPos, initPos, timer / (SightjackDelay / 2));
                yield return null;

            } while (timer < SightjackDelay / 2);

            BlinkObject.SetActive(false);
        }
        else
        {
            //Attempted implementation of having the sleeping enemy open its eyes briefly when sightjacked but I don't really like it in this state but figured I'd keep the code here for later.

            /*EnemyAI tempAI = GhostCams[currentGhostCam].GetComponentInParent<EnemyAI>();
            tempAI.IsCurrentlySleeping = false;
            yield return null;
            if (tempAI.GetState() != EnemyAI.EnemyState.Searching)
            {
                timer = 0;
                do
                {
                    timer += Time.deltaTime;
                    BlinkObject.transform.position = Vector3.Lerp(targetPos, initPos, timer / (SightjackDelay / 2));
                    yield return null;

                } while (BlinkObject.transform.position.y < blinkObjInitHeight);

                BlinkObject.SetActive(false);
            }
            else {
                timer = 0;
                do
                {
                    timer += Time.deltaTime;
                    BlinkObject.transform.position = Vector3.Lerp(targetPos, initPos, timer / (SightjackDelay / 2));
                    yield return null;

                } while (timer < SightjackDelay / 3);

                Vector3 midOpenPos = BlinkObject.transform.position;

                yield return new WaitForSeconds(SightjackDelay);

                timer = 0;
                do
                {
                    timer += Time.deltaTime;
                    BlinkObject.transform.position = Vector3.Lerp(midOpenPos, targetPos, timer / (SightjackDelay / 2));
                    yield return null;
                } while (BlinkObject.transform.position.y > BlinkObjectTargetHeight);
            }
            if (tempAI.GetState() == EnemyAI.EnemyState.Searching) tempAI.IsCurrentlySleeping = true;*/
        }

        sightjackRoutine = null;
        GameManager.instance.SwitchInput(GameManager.instance.controls.Sightjacking.Get());
    }

    private IEnumerator SightjackInitial()
    {
        SetCurrentSightjackOverlay();
        CurrentSightjackOverlay.SetActive(true);
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());

        originalRotation = CinemachineCore.Instance.GetActiveBrain(0).CurrentCameraState.FinalOrientation;

        GameManager.instance.Player.FPSController.TransitionToWalkingCamera();
        yield return null;
        SightjackCamera.Priority = 50;
        yield return new WaitForSeconds(InitSightjackDelay);
        CurrentSightjackOverlay.SetActive(false);

        EnemyAI enemy = GhostPOVs[currentGhostCam].GetComponentInParent<EnemyAI>();

        if (enemy != null && enemy.GetState() == EnemyAI.EnemyState.Stunned)
        {
            if (GameManager.instance.StunnedOverlay != null && GameManager.instance.StunnedOverlay.Length != 0) GameManager.instance.StunnedOverlay[currentGhostCam % GameManager.instance.StunnedOverlay.Length].SetActive(true);
        }
        else
        {
            if (GameManager.instance.StunnedOverlay != null && GameManager.instance.StunnedOverlay.Length != 0) GameManager.instance.StunnedOverlay[currentGhostCam % GameManager.instance.StunnedOverlay.Length].SetActive(false);
        }

        if (enemy != null && enemy.AnglerEnemy) GameManager.instance.SetAnglerEffects();

        GameManager.instance.SetSightJackingVisuals();

        if (enemy == null)
        {
            CurrentSightjackIsOverseer = true;
            GameManager.instance.SetOverseerEffects();
        }
        else
        {
            CurrentSightjackIsOverseer = false;
            GameManager.instance.EndOverseerEffects();
        }

        playerAudioListener.enabled = false;
        DisableGhostCams();
        SightjackCamera.Follow = GhostPOVs[currentGhostCam].transform;


        GhostPOVs[currentGhostCam].GetComponent<AudioListener>().enabled = true;
        if (enemy != null && enemy.IsCurrentlySleeping)
        {
            BlinkObject.SetActive(true);
            BlinkObject.transform.position = new Vector3(BlinkObject.transform.position.x, BlinkObjectTargetHeight, BlinkObject.transform.position.y);
        }

        sightjackRoutine = null;

        GameManager.instance.SwitchInput(GameManager.instance.controls.Sightjacking.Get());
    }

    public void ForceEndSightjacking()
    {
        StopAllCoroutines();
        sightjackRoutine = StartCoroutine(EndSightjacking());
    }

    public void SetBothMeshRenderers(bool state)
    {
        foreach (SkinnedMeshRenderer renderer in playerSkinnedMeshRenderers)
        {
            renderer.enabled = state;
        }
        foreach (SkinnedMeshRenderer renderer in crouchedSkinnedMeshRenderers)
        {
            renderer.enabled = state;
        }
    }

    public void SetPlayerMeshRenderers(bool state)
    {
        SkinnedMeshRenderer[] renderers = GameManager.instance.Player.FPSController.IsCrouched() ? crouchedSkinnedMeshRenderers : playerSkinnedMeshRenderers;

        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            renderer.enabled = state;
        }
    }

    private IEnumerator EndSightjacking()
    {
        BlinkObject.SetActive(false);
        GameManager.instance.EndSightJacking();
        DisableGhostCams();

        SightjackCamera.Priority = 5;

        SetPlayerMeshRenderers(false);
        playerAudioListener.enabled = true;

        SetCurrentSightjackOverlay();
        CurrentSightjackOverlay.SetActive(true);

        AudioManager.instance.PauseSightJackAmbience();
        AudioManager.instance.PauseStunnedSightJackAmbience();
        if (OverseerSightjackNoise != null) OverseerSightjackNoise.Stop();


        if (GameManager.instance.StunnedOverlay != null)
        {
            foreach (GameObject overlay in GameManager.instance.StunnedOverlay)
            {
                overlay.SetActive(false);
            }
        }

        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        yield return null;
        walkCamera.GetCinemachineComponent<CinemachinePOV>().ForceCameraPosition(Vector3.zero, originalRotation);
        yield return new WaitForSeconds(SightjackDelay);

        CurrentSightjackOverlay.SetActive(false);

        //Bug where this was still happening at wrong times sometime, so adding this extra call too
        AudioManager.instance.PauseStunnedSightJackAmbience();
        sightjackRoutine = null;
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
    }
    private void IncrementGhostCam()
    {
        currentGhostCam = (currentGhostCam + 1) % GhostPOVs.Length;
    }

    private void DecrementGhostCam()
    {
        currentGhostCam--;
        if (currentGhostCam < 0) currentGhostCam = GhostPOVs.Length - 1;
    }

    private void SetGhostCam(int value)
    {
        currentGhostCam = value;
    }

    private void DisableGhostCams()
    {
        foreach (GameObject pov in GhostPOVs)
        {
            pov.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void StunTarget()
    {
        if(GameManager.instance.Player.CanStun())
        {
            EnemyAI enemyTar = GhostPOVs[currentGhostCam].GetComponentInParent<EnemyAI>();
            //TODO: Maybe make it so a special effect occurs if you attempt to stun giant head
            if (enemyTar != null)
            {
                StartCoroutine(StunRoutine(enemyTar));
            }
            else
            {
                GameManager.instance.DisplayMessage("It is too powerful for me to use the heirloom on it.");
            }
        } 
    }

    private IEnumerator StunRoutine(EnemyAI enemyTar)
    {
        StunCount++;

        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        if (!GameManager.instance.Player.HasOverlaySwappedColor && GameManager.instance.Player.InventoryContains("True Family Heirloom")) GameManager.instance.Player.SwapOverlayColor();
        SetCurrentStunAbilityOverlay();
        CurrentStunAbilityOverlay.SetActive(true);
        AudioManager.instance.PlayStunAbilityNoise();
        GameManager.instance.Player.PutStunOnCooldown();
        yield return new WaitForSeconds(.8f);
        CurrentStunAbilityOverlay.SetActive(false);

        enemyTar.SetState(EnemyAI.EnemyState.Stunned);
        SwapStunToNormal(true);
        GameManager.instance.SwitchInput(GameManager.instance.controls.Sightjacking.Get());
    }

    public void EnemyNoLongerStunned(EnemyAI enemy)
    {
        if (GameManager.instance.Player.isSightJacking)
        {
            EnemyAI currentEnemy = GhostPOVs[currentGhostCam].GetComponentInParent<EnemyAI>();
            if (currentEnemy != null && currentEnemy == enemy)
            {
                SwapStunToNormal();
            }
        }
    }

    private void SwapStunToNormal(bool reverse = false)
    {
        if (!reverse)
        {
            DisableAllStunnedOverlays();
            AudioManager.instance.PlaySightJackAmbience();
        }
        else
        {
            if (GameManager.instance.StunnedOverlay != null && GameManager.instance.StunnedOverlay.Length != 0) GameManager.instance.StunnedOverlay[currentGhostCam % GameManager.instance.StunnedOverlay.Length].SetActive(true);
            AudioManager.instance.PlaySightJackAmbience(true);
        }
    }

    private void DisableAllStunnedOverlays()
    {
        if (GameManager.instance.StunnedOverlay != null)
        {
            foreach (GameObject overlay in GameManager.instance.StunnedOverlay)
            {
                overlay.SetActive(false);
            }
        }
    }
}

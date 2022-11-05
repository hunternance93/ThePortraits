using System.Collections;
using UnityEngine;
using static FloorSoundController;

//Class used to manage non-spatial audio
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;
    [SerializeField] private AudioSource sightJackStart = null;
    [SerializeField] private AudioSource sightJackAmbience = null;
    [SerializeField] private AudioSource pausedStunnedSightJackAmbience = null;
    [SerializeField] private GameObject sightJackChangeParent = null;
    //[SerializeField] private AudioSource cicadaAmbience = null;
    [SerializeField] private AudioSource crouchWalking = null;
    [SerializeField] private AudioSource walking = null;
    [SerializeField] private AudioSource running = null;
    [SerializeField] private AudioSource openJournal = null;
    [SerializeField] private AudioSource turnPage = null;
    [SerializeField] private AudioSource deathNoise = null;
    [SerializeField] private AudioSource deathNoiseDentist = null;
    [SerializeField] private AudioSource heartBeat = null;
    [SerializeField] private AudioSource discordantOrchestra = null;
    [SerializeField] private AudioSource itemCollect = null;
    [SerializeField] private AudioSource stunAbilityNoise = null;
    [SerializeField] private AudioSource hoverOverUI = null;
    [SerializeField] private AudioSource selectUI = null;

    [SerializeField] private AudioClip dirtWalking = null;
    [SerializeField] private AudioClip dirtCrouching = null;
    [SerializeField] private AudioClip dirtRunning = null;
    [SerializeField] private AudioClip tileWalking = null;
    [SerializeField] private AudioClip tileCrouching = null;
    [SerializeField] private AudioClip tileRunning = null;
    [SerializeField] private AudioClip woodWalking = null;
    [SerializeField] private AudioClip woodCrouching = null;
    [SerializeField] private AudioClip woodRunning = null;
    [SerializeField] private AudioClip tatamiWalking = null;
    [SerializeField] private AudioClip tatamiCrouching = null;
    [SerializeField] private AudioClip tatamiRunning = null;
    [SerializeField] private AudioClip metalWalking = null;
    [SerializeField] private AudioClip metalCrouching = null;
    [SerializeField] private AudioClip metalRunning = null;

    [SerializeField] private float lengthOfTimeToPlayDeathNoise = 3;

    private int lastSightjackChangeSound = -1;
    private const int NumberOfSightjackChangeSounds = 3;

    private void Awake()
    {
        instance = this;
    }

    public void PlayItemCollect()
    {
        itemCollect.Play();
    }

    public void PlayDeath()
    {
        if (!deathNoise.isPlaying) deathNoise.Play();
        if (!deathNoiseDentist.isPlaying)
        {
            StartCoroutine(PlayDeathNoiseForLength(UnityEngine.Random.Range(0, deathNoiseDentist.clip.length - lengthOfTimeToPlayDeathNoise)));
        }
    }

    public void StopDeathSounds()
    {
        deathNoise.Stop();
        deathNoiseDentist.Stop();
    }


    public IEnumerator PlayDeathNoiseForLength(float startTime)
    {
        deathNoiseDentist.time = startTime;
        deathNoiseDentist.Play();
        yield return new WaitForSecondsRealtime((lengthOfTimeToPlayDeathNoise * 2)/3);
        float time = 0;
        float startVol = deathNoiseDentist.volume;
        while (time < (lengthOfTimeToPlayDeathNoise/3))
        {
            time += Time.unscaledDeltaTime;
            deathNoiseDentist.volume = Mathf.Lerp(startVol, 0, time / (lengthOfTimeToPlayDeathNoise / 3));
            yield return null;
        }
        deathNoiseDentist.Stop();
        deathNoiseDentist.volume = startVol;
    }

    public void PlayStunAbilityNoise()
    {
        stunAbilityNoise.Play();
    }

    public void PlayHeartBeat()
    {
        heartBeat.Play();
    }

    public void PlayDiscordantOrchestra()
    {
        //In case player falls off, I don't want it to replay
        if (!PlayerPrefs.HasKey("HasPlayedRooftopMusic"))
        {
            PlayerPrefs.SetString("HasPlayedRooftopMusic", "True");
            StartCoroutine(WaitAndPlay(discordantOrchestra, 1.5f));
        }
    }

    private IEnumerator WaitAndPlay(AudioSource aud, float delay)
    {
        yield return new WaitForSeconds(delay);
        aud.Play();
    }

    public void PlayOpenJournal()
    {
        openJournal.Play();
    }

    public void PlayTurnPage()
    {
        turnPage.Play();
    }

    public void PlayHoverOverUI()
    {
        return;
        hoverOverUI.PlayOneShot(hoverOverUI.clip);
    }

    public void PlaySelectUI()
    {
        return;
        selectUI.PlayOneShot(selectUI.clip);
    }

    public void PlaySightJackStart()
    {
        sightJackStart.PlayOneShot(sightJackStart.clip);
    }

    //Plays one of the sightjack change sounds randomly, but not the last one it played
    public void PlaySightJackChange()
    {
        int soundIndex;
        do
        {
            soundIndex = Random.Range(0, NumberOfSightjackChangeSounds);
        } while (soundIndex == lastSightjackChangeSound);
        lastSightjackChangeSound = soundIndex;
        AudioSource sound = sightJackChangeParent.GetComponentsInChildren<AudioSource>()[soundIndex];
        sound.PlayOneShot(sound.clip);
    }

    public void PlaySightJackAmbience(bool stunnedSightJack = false, float ambienceDelay = 0)
    {
        if (stunnedSightJack)
        {
            sightJackAmbience.Pause();
            if (pausedStunnedSightJackAmbience.isPlaying) return;
            StartCoroutine(PlayStunnedSightJackAmbience(ambienceDelay));
        }
        else
        {
            pausedStunnedSightJackAmbience.Pause();
            if (sightJackAmbience.isPlaying) return;
            sightJackAmbience.Play();
        }
    }

    public void PauseSightJackAmbience()
    {
        sightJackAmbience.Stop();
    }

    public IEnumerator PlayStunnedSightJackAmbience(float ambienceDelay)
    {
        yield return new WaitForSeconds(ambienceDelay);
        pausedStunnedSightJackAmbience.Play();
    }

    public void PauseStunnedSightJackAmbience()
    {
        pausedStunnedSightJackAmbience.Stop();
    }

    public void PlayCrouchAudio()
    {
        crouchWalking.Play();
        walking.Pause();
        running.Pause();
    }

    public void PlayWalkingAudio()
    {
        crouchWalking.Pause();
        walking.Play();
        running.Pause();
    }

    public void PlayRunningAudio()
    {
        crouchWalking.Pause();
        walking.Pause();
        running.Play();
    }

    public bool IsPlayingMovementAudio()
    {
        return crouchWalking.isPlaying || walking.isPlaying || running.isPlaying;
    }

    public void SetFootStepAudioClips(FloorSoundType fst)
    {
        bool crouchWasPlaying = crouchWalking.isPlaying;
        bool walkingWasPlaying = walking.isPlaying;
        bool runningWasPlaying = running.isPlaying;
        switch (fst)
        {
            case FloorSoundType.Normal:
            case FloorSoundType.Water:
                crouchWalking.clip = dirtCrouching;
                walking.clip = dirtWalking;
                running.clip = dirtRunning;
                break;
            case FloorSoundType.Tatami:
                crouchWalking.clip = tatamiCrouching;
                walking.clip = tatamiWalking;
                running.clip = tatamiRunning;
                break;
            case FloorSoundType.Metal:
                crouchWalking.clip = metalCrouching;
                walking.clip = metalWalking;
                running.clip = metalRunning;
                break;
            case FloorSoundType.Tile:
                crouchWalking.clip = tileCrouching;
                walking.clip = tileWalking;
                running.clip = tileRunning;
                break;
            case FloorSoundType.Wood:
                crouchWalking.clip = woodCrouching;
                walking.clip = woodWalking;
                running.clip = woodRunning;
                break;
        }
        if (crouchWasPlaying) crouchWalking.Play();
        if (walkingWasPlaying) walking.Play();
        if (runningWasPlaying) running.Play();
    }

    public void SetCrouchVolume(float vol)
    {
        crouchWalking.volume = vol;
    }

    public void SetWalkingVolume(float vol)
    {
        walking.volume = vol;
    }

    public void SetRunningVolume(float vol)
    {
        running.volume = vol;
    }

    public void StopPlayerMovementAudio()
    {
        crouchWalking.Pause();
        walking.Pause();
        running.Pause();
    }

    public void StopAllAudio()
    {
        foreach (AudioSource audioSource in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            audioSource.Stop();
        }
    }
}

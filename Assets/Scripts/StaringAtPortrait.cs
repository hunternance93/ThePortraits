using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class StaringAtPortrait : MonoBehaviour
{
    public AudioSource ScarySounds = null;

    public PostProcessProfile VolProfile;
    private Vignette vig;
    private ChromaticAberration _CA;

    [HideInInspector] public bool LookingAtPortrait = false;

    private const float MAX_VIG = .55f;
    private const float MIN_VIG = .2f;
    private const float MAX_CA = 1f;
    private const float MIN_CA = .4f;
    private const float TIME_TO_REACH_EFFECTS = .5f;
    private const float LENGTH_OF_PULSE = 1;
    private const float MIN_VOLUME = .7f;
    private const float MAX_VOLUME = 1;

    private float vignetteLevel = 0;
    private float chromLevel = 0;
    private float timer = 0;

    private Coroutine beginEffectsRoutine = null;
    private Coroutine endEffectsRoutine = null;
    private Coroutine pulseEffectsRoutine = null;

    private void Start()
    {
        VolProfile.TryGetSettings<Vignette>(out vig);
        VolProfile.TryGetSettings<ChromaticAberration>(out _CA);
    }

    public void SetStaring(bool isStaring)
    {
        LookingAtPortrait = isStaring;
        if (pulseEffectsRoutine != null) return;
        if (LookingAtPortrait)
        {
            if (beginEffectsRoutine == null)
            {
                if (endEffectsRoutine != null)
                {
                    StopCoroutine(endEffectsRoutine);
                    endEffectsRoutine = null;
                }
                beginEffectsRoutine = StartCoroutine(BeginEffects());
            }
        }
        else
        {
            if (endEffectsRoutine == null)
            {
                if (beginEffectsRoutine != null)
                {
                    StopCoroutine(beginEffectsRoutine);
                    beginEffectsRoutine = null;
                }
                endEffectsRoutine = StartCoroutine(EndEffects());
            }
        }
    }
    
    private IEnumerator BeginEffects()
    {
        if (vig.intensity.value <= MIN_VIG)
        {
            timer = Mathf.Lerp(0, TIME_TO_REACH_EFFECTS, vig.intensity.value / MIN_VIG);
            while (timer < TIME_TO_REACH_EFFECTS)
            {
                vig.intensity.value = Mathf.Lerp(0, MIN_VIG, timer / TIME_TO_REACH_EFFECTS);
                _CA.intensity.value = Mathf.Lerp(0, MIN_CA, timer / TIME_TO_REACH_EFFECTS);
                ScarySounds.volume = Mathf.Lerp(0, MIN_VOLUME, timer / TIME_TO_REACH_EFFECTS);
                timer += Time.deltaTime;
                yield return null;
            }
        }
            beginEffectsRoutine = null;
            pulseEffectsRoutine = StartCoroutine(PulseEffects());
    }

    private IEnumerator PulseEffects()
    {
        while (LookingAtPortrait)
        {
            float timer = 0;
            while (timer < LENGTH_OF_PULSE /2)
            {
                vig.intensity.value = Mathf.Lerp(MIN_VIG, MAX_VIG, timer / (LENGTH_OF_PULSE / 2));
                _CA.intensity.value = Mathf.Lerp(MIN_CA, MAX_CA, timer / (LENGTH_OF_PULSE / 2));
                ScarySounds.volume = Mathf.Lerp(MIN_VOLUME, MAX_VOLUME, timer / (LENGTH_OF_PULSE / 2));
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0;
            while (timer < LENGTH_OF_PULSE / 2)
            {
                vig.intensity.value = Mathf.Lerp(MAX_VIG, MIN_VIG, timer / (LENGTH_OF_PULSE / 2));
                _CA.intensity.value = Mathf.Lerp(MAX_CA, MIN_CA, timer / (LENGTH_OF_PULSE / 2));
                ScarySounds.volume = Mathf.Lerp(MAX_VOLUME, MIN_VOLUME, timer / (LENGTH_OF_PULSE / 2));
                timer += Time.deltaTime;
                yield return null;
            }
        }
        pulseEffectsRoutine = null;
        beginEffectsRoutine = null; //just in case idk
        if (endEffectsRoutine == null) endEffectsRoutine = StartCoroutine(EndEffects());
    }

    private IEnumerator EndEffects()
    {
        timer = Mathf.Lerp(TIME_TO_REACH_EFFECTS, 0, vig.intensity.value / MIN_VIG);
        while (timer < TIME_TO_REACH_EFFECTS)
        {
            vig.intensity.value = Mathf.Lerp(MIN_VIG, 0, timer / TIME_TO_REACH_EFFECTS);
            _CA.intensity.value = Mathf.Lerp(MIN_CA, 0, timer / TIME_TO_REACH_EFFECTS);
            ScarySounds.volume = Mathf.Lerp(MIN_VOLUME, 0, timer / TIME_TO_REACH_EFFECTS);
            timer += Time.deltaTime;
            yield return null;
        }

        vig.intensity.value = 0;
        _CA.intensity.value = 0;
        ScarySounds.volume = 0;
        endEffectsRoutine = null;
    }

    public void InstantlyStopAllEffects()
    {
        StopAllCoroutines();
        pulseEffectsRoutine = null;
        beginEffectsRoutine = null;
        endEffectsRoutine = null;
        vig.intensity.value = 0;
        _CA.intensity.value = 0;
        ScarySounds.volume = 0;
        LookingAtPortrait = false;
    }
}

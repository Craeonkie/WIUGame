using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;
    public bool loop;
    public bool isBGM;
    [Range(0, 1)] public float spatialBlend;
    public float minDistance;
    public float maxDistance;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public AnimationCurve customRolloffCurve;
    [Range(0, 256)] public int priority = 128;

    [Range(0f, 1f)] public float baseVolume = 1f;

    public UnityEvent AudioFinishedEvent;

    public AudioSource source;
    private float finalVolume;
    private float savedGroupVolume;
    private float savedMasterVolume;
    public float audioLerpTime = 0.5f;
    private MonoBehaviour coroutineRunner;
    //private Coroutine trackingCoroutine;
    private Coroutine audioEventCoroutine;

    public void Init(GameObject AudioObject)
    {
        // Create an AudioSource on the AudioManager GameObject
        source = AudioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.priority = priority;

        source.spatialBlend = spatialBlend;
        source.minDistance = Mathf.Abs(minDistance);
        source.maxDistance = Mathf.Abs(maxDistance);
        source.rolloffMode = rolloffMode;

        

        if (rolloffMode == AudioRolloffMode.Custom)
        {
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloffCurve);
        }

        source.volume = baseVolume;
        finalVolume = baseVolume;

        source.Play();
        source.Pause();
        source.time = 0f;

        if (AudioObject.GetComponent<AudioMonobehaviour>() == null)
        {
            AudioObject.AddComponent<AudioMonobehaviour>();
        }

        //need to use monobehaviour to run a corotine
        coroutineRunner = AudioObject.GetComponent<AudioMonobehaviour>();
    }

    public void Play()
    {
        if (source != null)
        {
            source.volume = baseVolume * savedMasterVolume * savedGroupVolume;

            source.Play();

            //StopTracking();
            //trackingCoroutine = coroutineRunner.StartCoroutine(TrackAudioCompletion());
        }

    }

    public void PlayLerp()
    {
        if (source != null)
        {
            if (audioEventCoroutine != null)
                coroutineRunner.StopCoroutine(audioEventCoroutine);

            finalVolume = 0f;
            source.volume = 0f;

            audioEventCoroutine = coroutineRunner.StartCoroutine(PlayLerpVolume(audioLerpTime));

            source.Play();
        }
    }

    public void PlayLerpCustom(float lerpTime)
    {
        if (source != null)
        {
            if (audioEventCoroutine != null)
                coroutineRunner.StopCoroutine(audioEventCoroutine);

            audioEventCoroutine = coroutineRunner.StartCoroutine(PlayLerpVolume(lerpTime));

            source.Play();
        }
    }

    private IEnumerator PlayLerpVolume(float lerpTime)
    {
        float elapsedTime = 0f;
        float startVolume = finalVolume;

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            finalVolume = Mathf.Lerp(startVolume, baseVolume, elapsedTime / lerpTime);
            source.volume = finalVolume * savedMasterVolume * savedGroupVolume;

            yield return null;
        }
    }

    public void PlayClipAtPoint(Vector3 position)
    {
        if (source != null)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }

    //private void StopTracking()
    //{
    //    if (trackingCoroutine != null && coroutineRunner != null)
    //    {
    //        coroutineRunner.StopCoroutine(trackingCoroutine);
    //        trackingCoroutine = null;
    //    }
    //}

    public void PlayOneShot()
    {
        if (source != null)
        {
            source.PlayOneShot(clip);

            //coroutineRunner.StartCoroutine(TrackOneShotCompletion(clip.length));
        }
    }

    public void Pause()
    {
        if (source != null)
            source.Pause();
    }

    public void Reset()
    {
        if (source != null)
            source.time = 0f;
    }

    public void Stop()
    {
        if (source != null)
            source.Stop();
    }

    public void StopLerp()
    {
        if (source != null)
        {
            if (audioEventCoroutine != null)
                coroutineRunner.StopCoroutine(audioEventCoroutine);

            audioEventCoroutine = coroutineRunner.StartCoroutine(StopLerpVolume(audioLerpTime));
        }
    }

    public void StopLerpCustom(float lerpTime)
    {
        if (source != null)
        {
            if (audioEventCoroutine != null)
                coroutineRunner.StopCoroutine(audioEventCoroutine);

            audioEventCoroutine = coroutineRunner.StartCoroutine(StopLerpVolume(lerpTime));
        }
    }

    public IEnumerator StopLerpVolume(float lerpTime)
    {
        float elapsedTime = 0f;
        float startVolume = finalVolume;
        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            finalVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / lerpTime);
            source.volume = finalVolume * savedMasterVolume * savedGroupVolume;
            yield return null;
        }

        source.Stop();
    }

    public void Resume()
    {
        if (source != null)
        {
            if (source.time > 0f)
                source.Play();
        }
    }

    public void ApplySettings(float master, float bgm, float sfx, float pitch)
    {
        if (source == null) return;

        float groupVolume = isBGM ? bgm : sfx;
        source.volume = finalVolume * master * groupVolume;
        source.pitch = pitch;

        savedGroupVolume = groupVolume;
        savedMasterVolume = master;
    }

    public bool IsPlaying()
    {
        return source.isPlaying;
    }

    private IEnumerator TrackAudioCompletion()
    {
        // wait until audio is not playing
        while (source.isPlaying)
        {
            yield return null;
        }

        AudioFinishedEvent?.Invoke();
        //trackingCoroutine = null;
    }

    private IEnumerator TrackOneShotCompletion(float duration)
    {
        //wait for that duration
        yield return new WaitForSeconds(duration);

        AudioFinishedEvent?.Invoke();
    }
}
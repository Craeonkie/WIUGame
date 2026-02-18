using UnityEngine;

public class ObjectWith3DAudio : MonoBehaviour
{
    public bool _playOnStart;

    public new Audio audio = new Audio();

    public void OnEnable()
    {
        AudioLibrary.UpdateAudio += ApplySettings;
    }

    public void OnDisable()
    {
        AudioLibrary.UpdateAudio -= ApplySettings;
    }

    private void Start()
    {
        audio.Init(this.gameObject);

        audio.Play();

        if (!_playOnStart)
        {
            audio.Stop();
            audio.Reset();
        }

        UpdateSourceAudio();
    }

    public void UpdateSourceAudio()
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.UpdateSpecificAudioSettings(audio);
        }
    }

    public void Play()
    {
        if (audio != null)
        {
            audio.Play();

            //StopTracking();
            //trackingCoroutine = coroutineRunner.StartCoroutine(TrackAudioCompletion());
        }

    }

    public void Pause()
    {
        if (audio != null)
        {
            audio.Pause();
        }
    }

    public void PlayClipAtPoint(Vector3 position)
    {
        if (audio != null)
        {
            AudioSource.PlayClipAtPoint(audio.clip, position);
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
        if (audio != null)
        {
            audio.PlayOneShot();

            //coroutineRunner.StartCoroutine(TrackOneShotCompletion(clip.length));
        }
    }

    public void Reset()
    {
        if (audio != null)
            audio.Reset();
    }

    public void Stop()
    {
        if (audio != null)
            audio.Stop();
    }

    public void Resume()
    {
        if (audio != null)
        {
            audio.Play();
        }
    }

    public void ApplySettings(float master, float bgm, float sfx, float pitch)
    {
        if (audio == null) return;

        audio.ApplySettings(master, bgm, sfx, pitch);
    }
}

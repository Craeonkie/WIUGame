using UnityEngine;
using UnityEngine.Events;

public class ObjectWith3DAudio : MonoBehaviour
{
    public bool _playOnStart;

    public Audio audio3D = new Audio();

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
        audio3D.Init(this.gameObject);

        audio3D.Play();

        if (!_playOnStart)
        {
            audio3D.Stop();
            audio3D.Reset();
        }

        UpdateSourceAudio();
    }

    public void UpdateSourceAudio()
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.UpdateSpecificAudioSettings(audio3D);
        }
    }

    public void Play()
    {
        if (audio3D != null)
        {
            audio3D.Play();

            //StopTracking();
            //trackingCoroutine = coroutineRunner.StartCoroutine(TrackAudioCompletion());
        }

    }

    public void Pause()
    {
        if (audio3D != null)
        {
            audio3D.Pause();
        }
    }

    public void PlayClipAtPoint(Vector3 position)
    {
        if (audio3D != null)
        {
            AudioSource.PlayClipAtPoint(audio3D.clip, position);
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
        if (audio3D != null)
        {
            audio3D.PlayOneShot();

            //coroutineRunner.StartCoroutine(TrackOneShotCompletion(clip.length));
        }
    }

    public void Reset()
    {
        if (audio3D != null)
            audio3D.Reset();
    }

    public void Stop()
    {
        if (audio3D != null)
            audio3D.Stop();
    }

    public void Resume()
    {
        if (audio3D != null)
        {
            audio3D.Play();
        }
    }

    public void ApplySettings(float master, float bgm, float sfx, float pitch)
    {
        if (audio3D == null) return;

        audio3D.ApplySettings(master, bgm, sfx, pitch);
    }
}
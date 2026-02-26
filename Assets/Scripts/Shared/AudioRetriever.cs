using UnityEngine;

public class AudioRetriever : MonoBehaviour
{
    public void PlaySound(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PlaySound(name);
        }
    }
    
    public void PlaySoundLerp(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PlaySoundLerp(name);
        }
    }

    public void PauseSound(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PauseSound(name);
        }
    }

    public void PlayOneShot(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PlayOneShot(name);
        }
    }

    public void PlayOneShotRandom2(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            int random = Random.Range(1, 3);

            AudioLibrary.Instance.PlayOneShot(name + random);
        }
    }

    public void PlayOneShotRandom3(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            int random = Random.Range(1, 4);

            AudioLibrary.Instance.PlayOneShot(name + random);
        }
    }

    public void PlayOneShotRandom4(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            int random = Random.Range(1, 5);

            AudioLibrary.Instance.PlayOneShot(name + random);
        }
    }

    public void PlayOneShotRandom5(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            int random = Random.Range(1, 6);

            AudioLibrary.Instance.PlayOneShot(name + random);
        }
    }

    public GameObject Play3DAtTransform(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            return AudioLibrary.Instance.PlaySoundAtPointCustom(name, transform.position);
        }

        return null;
    }

    public void Play3DAtTransformNoReturnObject(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PlaySoundAtPointCustom(name, transform.position);
        }
    }

    public void Play3DAtTransformAndMakeItAChild(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.PlaySoundAtPointCustom(name, transform);
        }
    }

    public void StopSound(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.StopSound(name);
        }
    }

    public void StopSoundLerp(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.StopSoundLerp(name);
        }
    }

    public void ResetSound(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.ResetSound(name);
        }
    }

    public void StopAllSounds()
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.StopAllSounds();
        }
    }

    public void ResetAllSounds()
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.ResetAllSounds();
        }
    }

    public void ResumeAllSounds()
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.ResumeAllSounds();
        }
    }
}
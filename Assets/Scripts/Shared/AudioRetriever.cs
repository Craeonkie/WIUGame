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
            GameObject audioObject = AudioLibrary.Instance.PlaySoundAtPointCustom(name, transform.position);

            if (audioObject != null)
            {
                audioObject.transform.parent = transform;
                audioObject.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void StopSound(string name)
    {
        if (AudioLibrary.Instance != null)
        {
            AudioLibrary.Instance.StopSound(name);
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
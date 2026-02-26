using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class AudioLibrary : MonoBehaviour
{
    [Header("Audio Library Singleton")]
    public static AudioLibrary Instance { get; private set; }

    [Header("Audio Library (ScriptableObject)")]
    [SerializeField] private AudioManager _audioManager;

    [Header("Audio")]
    [SerializeField] private List<Audio> _audios = new List<Audio>();

    [Header("3D Audio Prefab")]
    [SerializeField] private GameObject _objectWith3DAudioPrefab;

    public static System.Action<float, float, float, float> UpdateAudio;

    public float _masterVolume;
    public float _sfxVolume;
    public float _bgmVolume;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_audioManager == null)
        {
            Debug.LogError("No AudioManager assigned to AudioLibrary!");
            return;
        }

        foreach (var audio in _audios)
        {
            Debug.Log(" audio");

            GameObject newAudioObject = new GameObject();
            newAudioObject.transform.SetParent(transform);
            newAudioObject.name = audio.name;

            audio.Init(newAudioObject);
        }

        UpdateAudioSettings();
    }

    public void UpdateAudioSettings()
    {
        if (_audioManager != null)
        {
            foreach (var audio in _audios)
            {
                audio.ApplySettings(_audioManager.masterVolume, _audioManager.bgmVolume, _audioManager.sfxVolume, _audioManager.globalPitch);
            }

            _masterVolume = _audioManager.masterVolume;
            _bgmVolume = _audioManager.bgmVolume;
            _sfxVolume = _audioManager.sfxVolume;
            UpdateAudio?.Invoke(_audioManager.masterVolume, _audioManager.bgmVolume, _audioManager.sfxVolume, _audioManager.globalPitch);
        }
    }

    public void UpdateSpecificAudioSettings(Audio specificAudio)
    {
        if (_audioManager != null && specificAudio != null)
        {
            specificAudio.ApplySettings(_audioManager.masterVolume, _audioManager.bgmVolume, _audioManager.sfxVolume, _audioManager.globalPitch);
        }
    }

    public void UpdateSpecificAudioSettings(ObjectWith3DAudio specificAudio)
    {
        if (_audioManager != null && specificAudio != null)
        {
            specificAudio.ApplySettings(_audioManager.masterVolume, _audioManager.bgmVolume, _audioManager.sfxVolume, _audioManager.globalPitch);
        }
    }

    public void PlaySound(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.Play();
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void PlaySoundLerp(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.PlayLerp();
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void PlaySoundLerp(string name, float lerpTime)
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.PlayLerpCustom(lerpTime);
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void PlaySoundAtPoint(string name, Vector3 position)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.PlayClipAtPoint(position);
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public GameObject PlaySoundAtPointCustom(string name, Vector3 position)
    {
        if (_audioManager == null) return null;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                var obj = Instantiate(_objectWith3DAudioPrefab, position, Quaternion.identity);
                var audioComponent = obj.GetComponent<ObjectWith3DAudio>();
                audioComponent.audio3D = audio;
                audioComponent._playOnStart = true;
                Destroy(obj, audio.clip.length);

                return obj;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);

        return null;
    }

    public void PlaySoundAtPointCustom(string name, Transform transform)
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                var obj = Instantiate(_objectWith3DAudioPrefab, transform.position, Quaternion.identity, transform);
                var audioComponent = obj.GetComponent<ObjectWith3DAudio>();
                audioComponent.audio3D = audio;
                audioComponent._playOnStart = true;
                Destroy(obj, audio.clip.length);

                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void PlayOneShot(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.PlayOneShot();
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void StopSound(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.Stop();
                return;
            }
        }
    }

    public void StopSoundLerp(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.StopLerp();
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void StopSoundLerp(string name, float lerpTime)
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.StopLerpCustom(lerpTime);
                return;
            }
        }
        Debug.LogWarning("Audio not found in library: " + name);
    }

    public void PauseSound(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.Pause();
                return;
            }
        }
    }

    public void ResetSound(string name)
    {
        if (_audioManager == null) return;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                audio.Reset();
                return;
            }
        }
    }

    public void StopAllSounds()
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            audio.Stop();
        }
    }

    public void ResetAllSounds()
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            audio.Stop();
            audio.Reset();
        }
    }

    public void ResumeAllSounds()
    {
        if (_audioManager == null) return;
        foreach (var audio in _audios)
        {
            audio.Resume();
        }
    }

    public bool IsPlaying(string name)
    {
        if (_audioManager == null) return false;

        foreach (var audio in _audios)
        {
            if (audio.name == name)
            {
                return audio.IsPlaying();
            }
        }
        return false;
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "AudioManager", menuName = "Audio/AudioManager")]
public class AudioManager : ScriptableObject
{
    [Header("Global Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(-3f, 3f)] public float globalPitch = 1f;
}
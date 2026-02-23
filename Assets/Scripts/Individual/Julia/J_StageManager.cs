using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[System.Serializable]
public class J_Cutscene
{
    public PlayableDirector director;
    public bool wasPlayed;
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;

    public void Play()
    {
        if (director != null)
        {
            director.Play();
            wasPlayed = true;
            OnCutsceneStart?.Invoke();
        }
    }

    public void Stop()
    {
        if (director != null)
        {
            director.Stop();
            OnCutsceneEnd?.Invoke();
        }
    }
}

public class J_StageManager : MonoBehaviour
{
    public static J_StageManager Instance;

    [SerializeField] private J_Cutscene[] _cutscenes;
    [SerializeField] private bool _playOnAwake;
    private int _currentCutsceneIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _currentCutsceneIndex = 0;

        // Subscribe to all cutscene director events
        foreach (var cutscene in _cutscenes)
        {
            if (cutscene.director != null)
            {
                cutscene.director.stopped += (director) => OnTimelineStopped(director, cutscene);
                cutscene.director.played += (director) => OnTimelinePlayed(director, cutscene);
            }
        }

        if (_playOnAwake && _cutscenes.Length > 0)
        {
            PlayCutscene(0);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        foreach (var cutscene in _cutscenes)
        {
            if (cutscene.director != null)
            {
                cutscene.director.stopped -= (director) => OnTimelineStopped(director, cutscene);
                cutscene.director.played -= (director) => OnTimelinePlayed(director, cutscene);
            }
        }
    }

    private void OnTimelinePlayed(PlayableDirector director, J_Cutscene cutscene)
    {
        Debug.Log($"Timeline Started: {director.name}");
        cutscene.OnCutsceneStart?.Invoke();
    }

    private void OnTimelineStopped(PlayableDirector director, J_Cutscene cutscene)
    {
        Debug.Log($"Timeline Ended: {director.name}");
        cutscene.OnCutsceneEnd?.Invoke();
    }

    

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < _cutscenes.Length)
        {
            _currentCutsceneIndex = index;
            _cutscenes[index].Play();
        }
    }

    public void PlayNextCutscene()
    {
        _currentCutsceneIndex++;
        if (_currentCutsceneIndex < _cutscenes.Length)
        {
            _cutscenes[_currentCutsceneIndex].Play();
        }
    }

    public void StopCurrentCutscene()
    {
        if (_currentCutsceneIndex >= 0 && _currentCutsceneIndex < _cutscenes.Length)
        {
            _cutscenes[_currentCutsceneIndex].Stop();
        }
    }
}
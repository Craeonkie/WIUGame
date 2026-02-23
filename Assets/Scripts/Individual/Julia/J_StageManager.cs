using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class J_Cutscene
{
    public CinemachineCamera cutsceneCamera;
    public bool wasPlayed;
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;
}

public class J_StageManager : MonoBehaviour
{
    public static J_StageManager Instance;
    [SerializeField] private J_Cutscene[] _cutscenes;
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
    }
}

using TMPro;
using UnityEngine;

public class J_BossBehaviour : Entity
{
    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _phaseText;

    [System.Serializable]
    struct Phase
    {
        public string name;
        public float healthThreshold;
        public System.Action OnTransition;
    }

    public enum STATE { 
        OBSERVING,
        PREPARING,
        ATTACKING,
        EXHAUSTED
    }

    [Header("Boss Phases")]
    [SerializeField] Phase[] _bossPhases;
    private STATE _currentState = STATE.OBSERVING;
    private int _currentPhaseIndex;
    private Phase _currentPhase;

    private void Start()
    {
        base.Start();
    }

    private void Update()
    {
        base.Update();
    }

    private void UpdateObservingState()
    {

    }

    private void ChangeStates()
    {

    }

    private void ChangePhases()
    {
        if (_currentPhaseIndex + 1 == _bossPhases.Length)
            return;

        // Change to next phase
        if (_currentHP <= (_currentPhase.healthThreshold * _maxHP))
        {
            _currentPhase.OnTransition?.Invoke();
            _currentPhaseIndex += 1;
            _currentPhase = _bossPhases[_currentPhaseIndex];
        }
    }
}

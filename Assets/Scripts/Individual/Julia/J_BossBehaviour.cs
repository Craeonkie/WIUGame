using TMPro;
using UnityEngine;
using static UnityEditor.ShaderData;

public class J_BossBehaviour : Entity
{
    [System.Serializable]
    struct Phase
    {
        public string name;
        [Range(0f, 1f)] public float healthThreshold;
        public float attackSpeedMultiplier;
        public float attackDamage;
    }

    public enum STATE { 
        IDLE,
        OBSERVING,
        PREPARING,
        ATTACKING,
        EXHAUSTED
    }

    public enum HAND {
        LEFT,
        RIGHT
    }

    [Header("Components")]
    [SerializeField] private GameObject _playerTarget;
    [SerializeField] private Animator _animator;

    [Header("Additional Boss Data")]
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackCooldown;
    private float _currentAttackDamage;

    [Header("Boss Phases")]
    [SerializeField] Phase[] _phases;
    private int _currentPhaseIndex;
    public System.Action<int> OnPhaseEnter;

    [Header("Boss States")]
    private STATE _currentState;
    private HAND _attackingHand;
    private int _leftHandFrequency;
    private int _rightHandFrequency;

    [SerializeField] private float _idleDuration;
    [SerializeField] private float _readyDuration;
    private float _currentStateTimer;



    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _phaseText;

    protected override void Start()
    {
        base.Start();

        // Get the first phase and state
        _currentState = STATE.OBSERVING;
        _currentPhaseIndex = 0;

        _leftHandFrequency = 0;
        _rightHandFrequency = 0;
    }

    protected override void Update()
    {
        base.Update();

        CheckStateTransition();
    }

    private void FixedUpdate()
    {
        // Perform FSM actions
        switch (_currentState)
        {
            case STATE.IDLE:
                Idle();
                break;
            case STATE.OBSERVING:
                Observe();
                break;
            case STATE.PREPARING:
                Prepare();
                break;
            case STATE.ATTACKING:
                Attack();
                break;
            case STATE.EXHAUSTED:
                Exhausted();
                break;
        }
    }



    public override void TakeDamage(float damageTaken)
    {
        if (isInvincible)
            return;

        _currentHP -= damageTaken;
        float healthPercent = _currentHP / _myEntityData._maxHP;

        CheckPhaseTransition(healthPercent);
    }

    private void CheckPhaseTransition(float healthPercent)
    {
        // Check if should die
        if (_currentHP <= 0)
        {
            Die();
            return;
        }

        // Check if we should advance to next phase
        for (int i = _currentPhaseIndex + 1; i < _phases.Length; i++)
        {
            if (healthPercent <= _phases[i].healthThreshold)
            {
                EnterPhase(i);
                break;
            }
        }
    }

    private void EnterPhase(int phaseIndex)
    {
        if (phaseIndex >= _phases.Length) 
            return;

        _currentPhaseIndex = phaseIndex;
        Phase phase = _phases[phaseIndex];

        Debug.Log($"Boss entering phase: {phase.name}");
        _phaseText.text = phase.ToString();

        // Apply phase modifiers
        _attackCooldown /= phase.attackSpeedMultiplier;
        _currentAttackDamage = phase.attackDamage;

        InvokePhaseEnterEvent(phaseIndex);
    }

    private void InvokePhaseEnterEvent(int index)
    {
        // TODO: Event should differ based on which phase it is

        OnPhaseEnter?.Invoke(index);
    }




    private void CheckStateTransition()
    {
        switch (_currentState)
        {
            case STATE.IDLE:

                // Enter observing state
                if (_currentStateTimer <= 0f)
                {
                    EnterState(STATE.OBSERVING);
                }


                break;
            case STATE.OBSERVING:
                
                // Enter preparing state
                if (_currentStateTimer <= 0f)
                {
                    EnterState(STATE.PREPARING);
                }

                break;
            case STATE.PREPARING:

                // Enter attacking state
                if (_currentStateTimer <= 0f)
                {
                    EnterState(STATE.ATTACKING);
                }

                break;
            case STATE.ATTACKING:

                // Enter exhausted state
                if (_currentStateTimer <= 0f)
                {
                    EnterState(STATE.EXHAUSTED);
                }

                break;
            case STATE.EXHAUSTED:

                // Enter idle state
                if (_currentStateTimer <= 0f)
                {
                    EnterState(STATE.IDLE);
                }

                break;
        }
    }

    private void EnterState(STATE nextState)
    {
        switch (nextState)
        {
            case STATE.IDLE:


                break;
            case STATE.OBSERVING:
                break;
            case STATE.PREPARING:
                // TODO: Randomise attack here based on frequency + weightage
                if (_leftHandFrequency == 2)
                {
                    _attackingHand = HAND.RIGHT;
                    _rightHandFrequency++;
                    _leftHandFrequency = 0;
                }
                else if (_rightHandFrequency == 2)
                {
                    _attackingHand = HAND.LEFT;
                    _leftHandFrequency++;
                    _rightHandFrequency = 0;
                }
                else
                {
                    // Weigh
                    int rand = Random.Range(0, 100);

                    //if (rand <= 50 - )
                }

                // Set to preparing for animator
                _animator.SetBool("Preparing", true);



                break;
            case STATE.ATTACKING:
                break;
            case STATE.EXHAUSTED:
                break;
        }
    }

    private void Idle()
    {

    }

    private void Observe()
    {
        _attackCooldown -= Time.deltaTime;

        // TODO: Head-aim rig should look at player and consider options

        
    }

    private void Prepare()
    {
        // TODO: Keep aiming for the player
    }

    private void Attack()
    {
        // Trigger attack
    }

    private void Exhausted()
    {
        // Rest
    }

    private void SpawnBugs()
    {
        // TODO: SPAWN BUG PREFABS INSIDE AREA
    }
}

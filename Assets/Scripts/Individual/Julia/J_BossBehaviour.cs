using TMPro;
using UnityEngine;

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
    [SerializeField] private float _attackSpeed; // Applied to the fist swinging down, use for adjusting animation speed
    private float _currentAttackDamage;

    [Header("Boss Phases")]
    [SerializeField] Phase[] _phases;
    private int _currentPhaseIndex;
    public System.Action<int> OnPhaseEnter;

    [Header("Boss States")]
    private STATE _currentState;
    private HAND _attackingHand;
    [SerializeField] int _timesBeforeExhausted;
    [SerializeField] int _hitsBeforeExhausted; // Only for 2nd and 3rd phase
    private bool _canChangeState;
    private int _currentTimesAttacked;
    private int _leftHandFrequency;
    private int _rightHandFrequency;


    [SerializeField] private float _idleDuration;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private float _readyDuration;
    [SerializeField] private float _exhaustedDuration;
    private float _currentStateTimer;



    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _phaseText;

    protected override void Start()
    {
        base.Start();

        // Get the first phase and state
        _currentState = STATE.IDLE;
        _currentPhaseIndex = 0;
        EnterState(_currentState);
        EnterPhase(_currentPhaseIndex);

        _canChangeState = true;

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
        _attackSpeed /= phase.attackSpeedMultiplier;
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
        if (!_canChangeState)
            return;

        _currentStateTimer -= Time.deltaTime;

        switch (_currentState)
        {
            case STATE.IDLE:

                // Enter observing state
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
                if (_currentTimesAttacked >= _hitsBeforeExhausted)
                {
                    EnterState(STATE.EXHAUSTED);
                }
                else
                {
                    EnterState(STATE.IDLE);
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

                // Set the duration
                _currentStateTimer = _attackCooldown;
                _animator.SetBool("Tired", false);
                _animator.SetBool("Ready", true);

                isInvincible = true;

                break;
            case STATE.PREPARING:

                // Set the duration
                _currentStateTimer = _readyDuration;


                // Decide an attack based on the frequency and chance
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
                    float leftWeight = 100f / (_leftHandFrequency + 1);
                    float rightWeight = 100f / (_rightHandFrequency + 1);

                    float rand = Random.Range(0f, leftWeight + rightWeight);

                    _attackingHand = rand < leftWeight ? HAND.LEFT : HAND.RIGHT;

                    if (_attackingHand == HAND.LEFT)
                        _leftHandFrequency++;
                    else
                        _rightHandFrequency++;

                }

                // Set to preparing for animator
                _animator.SetBool("Ready", false);
                _animator.SetBool("Preparing", true);
                _animator.SetInteger("Hand", (int)_attackingHand);

                break;
            case STATE.ATTACKING:

                Debug.Log("state attacking");

                _currentStateTimer = 0f;
                _currentTimesAttacked++;

                // Trigger attack
                _animator.SetBool("Preparing", false);
                _animator.SetTrigger("Attack");

                _canChangeState = false;

                break;
            case STATE.EXHAUSTED:

                // Set the duration
                _currentStateTimer = _exhaustedDuration;
                _currentTimesAttacked = 0;

                _animator.SetBool("Tired", true);

                isInvincible = false;

                break;
        }

        _currentState = nextState;
        _stateText.text = _currentState.ToString();
    }

    private void Idle()
    {
        _currentStateTimer -= Time.deltaTime;
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

    public void AllowStateTransition() => _canChangeState = true;
}

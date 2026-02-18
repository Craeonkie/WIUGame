using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
    [SerializeField] private SphereCollider[] _fistColliders; // Use this list to enable and disable respectively
    [SerializeField] private CinemachineImpulseSource[] _sources;
    [SerializeField] private GameObject[] _shockwaveAffectedGameObjects; // Use this list to trigger the shockwave
    [SerializeField] private TwoBoneIKConstraint _leftArmRig; // Set weight to 1 / 0 respectively
    [SerializeField] private Transform _leftArmTarget;

    private Vector3 _originalLeftTargetPosition;
    private float _leftTargetRigWeight = 0f;


    [Header("Additional Boss Data")]
    [SerializeField] private LayerMask _layersToCheck;
    [SerializeField] private float _attackSpeed; // Applied to the fist swinging down, use for adjusting animation speed
    [SerializeField] private float _maxShockwaveDistance; // Applied to shockwave, maximum distance before shockwave dies down
    [SerializeField] private float _shockwaveIntensity; // Applied to shockwave, how intense the shockwave is
    [SerializeField] private float _shockwaveTravelSpeed; // Applied to shockwave, how fast the shockwave travels
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

    private void OnEnable()
    {
        J_PlayerController.OnMove += UpdateRigTargetPosition;
    }

    private void OnDisable()
    {
        J_PlayerController.OnMove -= UpdateRigTargetPosition;
    }

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

        Debug.Log("Left arm rig: " + _leftArmRig.weight);

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

    private void LateUpdate()
    {
        //_leftArmRig.weight = _leftTargetRigWeight;

        //if (_leftTargetRigWeight > 0f)
        //{
        //    Vector3 animatedPosition = _leftArmTarget.position;
        //    _leftArmTarget.position = new Vector3(
        //        _playerTarget.transform.position.x,
        //        animatedPosition.y,              // Y comes from animation
        //        _playerTarget.transform.position.z
        //    );
        //}
    }


    private void CheckAttackColldiers()
    {
        for (int i = 0; i < 2; ++i)
        {
            if (!_fistColliders[i].enabled)
                continue;

            Vector3 worldCenter = _fistColliders[i].transform.TransformPoint(_fistColliders[i].center);
            float scaleFactor = Mathf.Max(_fistColliders[i].transform.lossyScale.x, _fistColliders[i].transform.lossyScale.y, _fistColliders[i].transform.lossyScale.z);
            float actualWorldRadius = _fistColliders[i].radius * scaleFactor;


            // I'm just setting it manually to player layer because a modular script of this doesn't exist yet
            Collider[] hitColliders = Physics.OverlapSphere(worldCenter, actualWorldRadius, _layersToCheck);

            for (int j = 0; j < hitColliders.Length; j++)
            {
                // Check for tag
                if (hitColliders[j].gameObject.CompareTag("Player"))
                {
                    // Deal damage to the player
                    hitColliders[j].gameObject.GetComponent<Entity>().TakeDamage(_currentAttackDamage);
                    Debug.Log("Player instantly died!");
                }
                else if (hitColliders[j].gameObject.CompareTag("ShockwaveAffetcted"))
                {
                    // Disable this collider
                    _fistColliders[i].enabled = false;

                    // Call all materials with the shockwave material and invoke the shockwave
                    // Manually set the start position of the shockwave
                    for (int k = 0; k < _shockwaveAffectedGameObjects.Length; ++k)
                    {
                        // Get the material of this game object
                        Material shockwaveMat = _shockwaveAffectedGameObjects[k].GetComponent<Renderer>().material;
                        shockwaveMat.SetVector("_RadiusCenter", transform.position - _shockwaveAffectedGameObjects[k].transform.position);
                    }

                    // Start shockwave coroutine
                    StartCoroutine(StartShockwave());
                }


                // TODO: Play audio here
                //if (SlashSound)
                //{
                //    AudioManager.Instance.PlayOneShot("slashHit1", damageable.transform.position);
                //}
                //else
                //{
                //    AudioManager.Instance.PlayOneShot("punchImpact", damageable.transform.position);
                //}

                // Generate impulse
                _sources[i].GenerateImpulse(Camera.main.transform.forward);
            }
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

                _leftTargetRigWeight = 0f;

                // Disable colldiers
                for (int i = 0; i < _fistColliders.Length; ++i)
                {
                    _fistColliders[i].enabled = false;
                    _fistColliders[i].isTrigger = true;
                }

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

                //_leftTargetRigWeight = 0f;

                // Enable colliders
                for (int i = 0; i < 2; ++i)
                {
                    _fistColliders[i].enabled = true;
                    _fistColliders[i].isTrigger = false;
                }

                isInvincible = false;

                break;
        }

        _currentState = nextState;
        _stateText.text = _currentState.ToString();
    }

    //private void LeaveState(STATE prevState)
    //{
    //    switch ()
    //}



    private void Idle()
    {
        //_currentStateTimer -= Time.deltaTime;
    }

    private void Prepare()
    {
        // TODO: Keep aiming for the player
        //_leftArmTarget =
    }

    private void Attack()
    {
        // Check for player
        CheckAttackColldiers();
    }

    private void Exhausted()
    {
        
    }

    private void SpawnBugs()
    {
        // TODO: SPAWN BUG PREFABS INSIDE AREA
    }

    public void AllowStateTransition() => _canChangeState = true;



    private void UpdateRigTargetPosition(Vector3 position)
    {
        // magic number speed for now
        var nextPosition = position;
        nextPosition.y = _originalLeftTargetPosition.y; 
        _leftArmTarget.position = Vector3.Lerp(_leftArmTarget.position, position, 1f);
    }

    public void StartTracking()
    {
        //_originalLeftTargetPosition = _leftArmTarget.position;
        //_leftTargetRigWeight = 1f;

        //Debug.Log("Left arm rig: " + _leftArmRig.weight);
    }


    
    private IEnumerator StartShockwave()
    {
        float currentDistance = 0f;
        float currentShockwaveIntensity = _shockwaveIntensity;

        // Continue shockwave until it reaches maximum distance
        while (currentDistance <= _maxShockwaveDistance)
        {
            for (int i = 0; i < _shockwaveAffectedGameObjects.Length; ++i)
            {
                Material mat = _shockwaveAffectedGameObjects[i].GetComponent<Renderer>().material;
                mat.SetFloat("_Intensity", currentShockwaveIntensity);
                mat.SetFloat("_Offset", currentDistance);
            }

            currentShockwaveIntensity = Mathf.Lerp(_shockwaveIntensity, 0f, (currentDistance / _maxShockwaveDistance));
            currentDistance += _shockwaveTravelSpeed * Time.deltaTime;

            // Draw debug ray to visualise where the shockwave is meant to be travelling
            //DrawDebugCircle()
            
            yield return null;
        }
    }



    // HELPER DEBUG FUNCTION, NOT MINE, WILL REMOVE LATER
    public static void DrawDebugCircle(Vector3 center, float radius, Color color, int segments)
    {
        // Ensure minimum segments
        if (segments == 0) segments = 50;

        float angleStep = 360f / segments;
        Vector3 lastPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = center.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = center.y; // For a horizontal circle in the XZ plane, set this to center.y and use 'z' below
            float z = center.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);

            Vector3 currentPoint = new Vector3(x, y, z);

            if (i > 0)
            {
                Debug.DrawLine(lastPoint, currentPoint, color);
            }
            lastPoint = currentPoint;
        }
    }


    private void OnDrawGizmos()
    {
        for (int i = 0; i < 2; ++i)
        {
            if (_fistColliders[i].enabled)
            {
                SphereCollider collider = _fistColliders[i];
                Vector3 worldCenter = collider.transform.TransformPoint(collider.center);

                float scaleFactor = Mathf.Max(collider.transform.lossyScale.x,
                                            collider.transform.lossyScale.y,
                                            collider.transform.lossyScale.z);

                float actualWorldRadius = collider.radius * scaleFactor;

                Gizmos.DrawWireSphere(worldCenter, actualWorldRadius);
            }
        }
    }
}

using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class J_BossBehaviour : Entity
{
    [System.Serializable]
    struct Phase
    {
        public string name;
        public bool hasDuration;
        public float phaseTimer;
        [Range(0f, 1f)] public float healthThreshold;
        public float attackSpeedMultiplier;
        public float attackDamage;
    }

    public enum STATE
    {
        IDLE,
        PREPARING,
        ATTACKING,
        THROWINGPILLOWS,
        HIT,
        EXHAUSTED
    }

    public enum HAND
    {
        LEFT,
        RIGHT,
        BOTH
    }

    [Header("Components")]
    [SerializeField] private GameObject _playerTarget;
    [SerializeField] private Animator _animator;
    [SerializeField] private SphereCollider[] _fistColliders; // Use this list to enable and disable respectively
    [SerializeField] private CinemachineImpulseSource[] _sources; // Impulse sources, add to the fists
    [SerializeField] private GameObject[] _shockwaveAffectedGameObjects; // Use this list to trigger the shockwave
    [SerializeField] private GameObject[] _shockwavePlanes;
    //[SerializeField] private TwoBoneIKConstraint _leftArmRig; // Set weight to 1 / 0 respectively
    //[SerializeField] private Transform _leftArmTarget;

    //private Vector3 _originalLeftTargetPosition;
    //private float _leftTargetRigWeight = 0f;


    [Header("Additional Boss Data")]
    [SerializeField] private LayerMask _layersToCheck;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private int _hitsRequired;
    [SerializeField] private float _attackSpeed; // Applied to the fist swinging down, use for adjusting animation speed
    [SerializeField] private float _shockwaveDamage;
    [SerializeField] private float _maxShockwaveDistance; // Applied to shockwave, maximum distance before shockwave dies down
    [SerializeField] private float _shockwaveIntensity; // Applied to shockwave, how intense the shockwave is
    [SerializeField] private float _shockwaveBandWidth;
    [SerializeField] private float _shockwaveTravelSpeed; // Applied to shockwave, how fast the shockwave travels
    private float _currentAttackDamage;

    [Header("Boss Phases")]
    [SerializeField] Phase[] _phases;
    private int _currentPhaseIndex;
    public static System.Action<int> OnPhaseEnter;
    private float _currentPhaseTimer;

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

    private MaterialPropertyBlock _mpb;

    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _phaseText;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
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

        //Debug.Log("Left arm rig: " + _leftArmRig.weight);


        // Decrease phase timer for phases that have duration
        if (_phases[_currentPhaseIndex].hasDuration)
        {
            _currentPhaseTimer -= Time.deltaTime;

            // Transition to next state
            if (_currentPhaseTimer <= 0f && _currentPhaseIndex < _phases.Length)
            {
                _currentPhaseIndex++;
                EnterPhase(_currentPhaseIndex);
            }
        }

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
            case STATE.HIT:
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


    private void CheckAttackColliders()
    {
        for (int i = 0; i < 2; ++i)
        {
            if (!_fistColliders[i].enabled)
                continue;


            Vector3 worldCenter = _fistColliders[i].transform.TransformPoint(_fistColliders[i].center);
            float scaleFactor = Mathf.Max(_fistColliders[i].transform.lossyScale.x, _fistColliders[i].transform.lossyScale.y, _fistColliders[i].transform.lossyScale.z);
            float actualWorldRadius = _fistColliders[i].radius * scaleFactor;

            // I'm just setting it manually to player layer because a modular script of this doesn't exist yet
            Collider[] hitColliders = Physics.OverlapSphere(worldCenter, actualWorldRadius, _playerLayer);
            for (int j = 0; j < hitColliders.Length; ++j)
            {
                hitColliders[j].GetComponent<Entity>().TakeDamage(float.MaxValue);
            }
           

            MaterialPropertyBlock handMpb = new MaterialPropertyBlock();
            Renderer planeR = _shockwavePlanes[i].GetComponent<Renderer>();
            Vector3 localCenter = planeR.transform.InverseTransformPoint(worldCenter);
            localCenter.y = 0f;

            handMpb.SetVector("_RadiusCenter", localCenter);
            planeR.SetPropertyBlock(handMpb);

            // Disable this collider
            _fistColliders[i].enabled = false;

            StartCoroutine(StartShockwave(worldCenter, planeR, handMpb));

            // TODO: Play audio here
            //if (SlashSound)
            //{
            //    AudioManager.Instance.PlayOneShot("slashHit1", damageable.transform.position);
            //}
            //else
            //{
            //    AudioManager.Instance.PlayOneShot("punchImpact", damageable.transform.position);
            //}

            //    // Generate impulse
            //    _sources[i].GenerateImpulse(Camera.main.transform.forward);
            //}
        }
    }

    private IEnumerator StartShockwave(Vector3 startPos, Renderer planeR, MaterialPropertyBlock mpb)
    {
        float currentDistance = 0f;
        float currentShockwaveIntensity = _shockwaveIntensity;
        bool collidedWith = false;

        while (currentDistance <= _maxShockwaveDistance)
        {
            // Update the new intensity and offset
            mpb.SetFloat("_Intensity", currentShockwaveIntensity);
            mpb.SetFloat("_Offset", currentDistance);
            planeR.SetPropertyBlock(mpb);

            // Check for actual collisions
            // Outer radius --> distance multiplied by the scale of the plane
            // Inner radius --> Outer radius minus the total width of the band
            Vector3 planeWorldScale = _shockwavePlanes[0].transform.lossyScale;
            float outerRadius = (currentDistance * planeWorldScale.x);
            float innerRadius = Mathf.Max(0f, (currentDistance * planeWorldScale.x) - 1f);

            DrawDebugCircle(startPos, outerRadius, Color.red, 36);
            DrawDebugCircle(startPos, innerRadius, Color.blue, 36);

            if (!collidedWith)
            {
                // There is only one player
                Collider[] hits = Physics.OverlapSphere(startPos, outerRadius, _playerLayer);

                for (int i = 0; i < hits.Length; ++i)
                {
                    if (!hits[i].TryGetComponent(out GroundChecker groundCheck))
                        continue;

                    // Check if grounded first
                    if (!groundCheck.IsGrounded())
                        break;

                    // Check distance from center of shockwave and check if hit the shockwave
                    if ((startPos - hits[i].gameObject.transform.position).magnitude > innerRadius)
                    {
                        hits[i].GetComponent<Entity>().TakeDamage(_shockwaveDamage);
                        Debug.Log("Player was hit by the shockwave!");
                        collidedWith = true;
                        break;
                    }
                }
            }

            // Increase the shockwave intensity
            currentShockwaveIntensity = Mathf.Lerp(_shockwaveIntensity, 0f, (currentDistance / _maxShockwaveDistance));
            currentDistance += _shockwaveTravelSpeed * Time.deltaTime;

            yield return null;
        }

        // Reset back to 0
        mpb.SetVector("_RadiusCenter", Vector3.zero);
        mpb.SetFloat("_Intensity", 0f);
        mpb.SetFloat("_Offset", 0f);
    }


    public override void TakeDamage(float damageTaken)
    {
        Debug.Log("went here");

        if (isInvincible)
            return;

        Debug.Log("took damage" + damageTaken);

        _currentHP -= damageTaken;
        float healthPercent = _currentHP / _maxHP;

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

        // Go to idle animation and reset stuff
        if (!IsInCurrentAnimationState("Idle") && _currentPhaseIndex > 0)
        {
            _animator.SetBool("Ready", false);
            _animator.SetBool("Preparing", false);
            _animator.SetBool("Tired", false);

            _leftHandFrequency = 0;
            _rightHandFrequency = 0;

            _animator.SetTrigger("Reset");

            EnterState(STATE.IDLE);
        }

        _currentPhaseIndex = phaseIndex;
        Phase phase = _phases[phaseIndex];

        Debug.Log($"Boss entering phase: {phase.name}");
        _phaseText.text = phase.ToString();

        // Apply phase modifiers
        _attackSpeed /= phase.attackSpeedMultiplier;
        _currentAttackDamage = phase.attackDamage;

        if (phase.hasDuration)
            _currentPhaseTimer = phase.phaseTimer;
        else
            _currentPhaseTimer = 0f;

        InvokePhaseEnterEvent(phaseIndex);
    }

    private void InvokePhaseEnterEvent(int index)
    {
        // TODO: Event should differ based on which phase it is
        OnPhaseEnter?.Invoke(index);

        // Phase 2
        if (index == 1)
        {
            J_SpawnManager.Instance.UpdateItemLimit("Bug", 8);
            J_SpawnManager.Instance.UpdateItemLimit("ThrowableBug", 8);
            J_SpawnManager.Instance.SpawnContinuously("Bug", 10f);
        }
        else if (index == 2)
        {
            J_SpawnManager.Instance.UpdateItemLimit("Bug", 15);
            J_SpawnManager.Instance.UpdateItemLimit("ThrowableBug", 15);
            J_SpawnManager.Instance.SpawnContinuously("Bug", 10f);
        }
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

                //_leftTargetRigWeight = 0f;

                // Disable colliders
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

                // Activate both planes
                for (int i = 0; i < _shockwavePlanes.Length; ++i)
                {
                    _shockwavePlanes[i].SetActive(true);
                }

                // Check the phase
                if (_currentPhaseIndex == 1)
                {
                    _attackingHand = HAND.BOTH;
                }
                else
                {
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
                }

                // Disable whichever plane wasn't active
                // Switch off the other plane
                //if (_attackingHand == HAND.LEFT)
                //    _shockwavePlanes[(int)HAND.RIGHT].SetActive(false);
                //else if (_attackingHand == HAND.RIGHT)
                //    _shockwavePlanes[(int)HAND.LEFT].SetActive(false);

                // Set to preparing for animator
                _animator.SetBool("Ready", false);
                _animator.SetBool("Preparing", true);
                _animator.SetInteger("Hand", (int)_attackingHand);

                break;
            case STATE.ATTACKING:

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
            case STATE.HIT:

                // Set

                break;
        }

        _currentState = nextState;
        _stateText.text = _currentState.ToString();
    }

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
        CheckAttackColliders();
    }

    private void Exhausted()
    {

    }

    public void AllowStateTransition() => _canChangeState = true;



    //private void UpdateRigTargetPosition(Vector3 position)
    //{
    //    // magic number speed for now
    //    var nextPosition = position;
    //    nextPosition.y = _originalLeftTargetPosition.y;
    //    _leftArmTarget.position = Vector3.Lerp(_leftArmTarget.position, position, 1f);
    //}

    //public void StartTracking()
    //{
    //    //_originalLeftTargetPosition = _leftArmTarget.position;
    //    //_leftTargetRigWeight = 1f;

    //    //Debug.Log("Left arm rig: " + _leftArmRig.weight);
    //}



    public void EnableColldier(int index)
    {
        if (index < 0 || index >= _fistColliders.Length)
        {
            Debug.Log("Invalid index was passed into EnableColldier!");
            return;
        }

        _fistColliders[index].enabled = true;
    }

    private void DisableAllColliders()
    {
        for (int i = 0; i < _fistColliders.Length; ++i)
        {
            _fistColliders[i].enabled = false;
        }
    }


    private IEnumerator DrawPoint(Vector3 pos)
    {
        float timer = 0f;

        while (timer < 5f)
        {
            Debug.DrawLine(pos, pos + (Vector3.up * 500f), Color.purple);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private bool IsInCurrentAnimationState(string stateName)
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private bool IsCurrentAnimationReadyForNextStep(string name)
    {
        // Check if the current animation has played enough to transition
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.normalizedTime >= 0.7f && stateInfo.IsName(name); // Adjust based on when you want to allow transitions
    }

    private bool IsCurrentAnimationEnded(string name)
    {
        // Check if the current animation has played enough to transition
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.normalizedTime >= 0.95f && stateInfo.IsName(name); // Adjust based on when you want to allow transitions
    }



    // DEBUG
    [ContextMenu("Start Next Phase")]
    public void StartNextPhase()
    {
        if (_currentPhaseIndex >= _phases.Length)
            return;

        _currentPhaseIndex++;
        EnterPhase(_currentPhaseIndex);
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
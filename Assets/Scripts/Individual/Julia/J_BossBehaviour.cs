using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class J_BossBehaviour : Entity
{
    [System.Serializable]
    struct Phase
    {
        public string name;
        public bool hasDuration;
        public float phaseTimer;
        [Range(0f, 1f)] public float healthThreshold;
        public float durationBeforeSlamDown;
        public UnityEvent OnPhaseTransition;
    }

    public enum STATE
    {
        IDLE,
        PREPARING,
        ATTACKING,
        THROWINGPILLOWS,
        RIPPINGPILLOWS,
        STEALINGPILLOWS,
        HIT,
        CONFUSED,
        EXHAUSTED
    }

    public enum HAND
    {
        LEFT,
        RIGHT,
        BOTH
    }

    public const string ANIMATOR_IDLE_BOOL = "Ready";
    public const string ANIMATOR_PREPARING_BOOL = "Preparing";
    public const string ANIMATOR_EXHAUSTED_BOOL = "Tired";
    public const string ANIMATOR_CONFUSED_BOOL = "Confused";
    public const string ANIMATOR_CONFUSE_TRIGGER = "Confuse";
    public const string ANIMATOR_HAND_VALUE = "Hand";
    public const string ANIMATOR_ATTACK_TRIGGER = "Attack";
    public const string ANIMATOR_RESET_TRIGGER = "Reset";
    public const string ANIMATOR_HIT_TRIGGER = "Hit";
    public const string ANIMATOR_THROW_TRIGGER = "Throw";
    public const string ANIMATOR_RIP_TRIGGER = "Rip";

    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SphereCollider[] _fistColliders; // Use this list to enable and disable respectively
    [SerializeField] private BoxCollider _headCollider; // Use this list to enable and disable respectively
    [SerializeField] private CinemachineImpulseSource[] _sources; // Impulse sources, add to the fists
    [SerializeField] private GameObject[] _shockwavePlanes;
    [SerializeField] private GameObject _fakePillow;

    [Header("Additional Boss Data")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _shockwaveDamage;
    [SerializeField] private float _maxShockwaveDistance; // Applied to shockwave, maximum distance before shockwave dies down
    [SerializeField] private float _shockwaveIntensity; // Applied to shockwave, how intense the shockwave is
    [SerializeField] private float _shockwaveBandWidth;
    [SerializeField] private float _shockwaveTravelSpeed; // Applied to shockwave, how fast the shockwave travels

    [Header("Boss Phases")]
    [SerializeField] Phase[] _phases;
    private int _currentPhaseIndex;
    public static System.Action<int> OnPhaseEnter;
    private float _currentPhaseTimer;

    [Header("Boss States")]
    private STATE _currentState;
    private HAND _attackingHand;
    [SerializeField] int _hitsBeforeExhausted; // Only for 2nd and 3rd phase
    private int _currentTimesHit;
    private int _leftHandFrequency;
    private int _rightHandFrequency;
    private bool _canChangeState;

    [SerializeField] private float _idleDuration;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private float _confusedDuration;
    [SerializeField] private float _exhaustedDuration;
    private float _readyingDuration;
    private float _currentStateTimer;

    [Header("Pillow Ripping Settings")]
    [SerializeField] private float _intervalBetweenRips;
    [SerializeField] private float _durationBeforeRipPillow;
    [SerializeField] private float _maximumForwardForce;
    [SerializeField] private float _horizontalForce;
    private float _currentRipPillowTimer;

    [Header("Pillow Stealing Settings")]
    [SerializeField] private float _durationBeforeStealPillow;
    private float _currentStealPillowTimer;
    private J_Pillow _currentPillow;
    public static System.Action OnStealPillow;

    [Header("Pillow Throwing Settings")]
    [SerializeField] private BoxCollider _throwRangeCollider;
    [SerializeField] private float _throwSpeed; 
    [SerializeField] private int _maxNumberOfPillowsInScene;
    private Vector3 _throwDestination;
    private int _currentNumberOfPillowsInScene;


    [Header("Confusion State Settings")]
    [SerializeField] private float _transportSpeed;
    private IEnumerator _transportPlayerCoroutine;
    public static System.Action <CapsuleCollider> OnTransportPlayer;


    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _phaseText;

    private void OnEnable()
    {
        J_CarryItem.OnCarry += CheckPillowToBeStolen;
        J_BossStateTrigger.OnShoulderTriggered += TriggerConfusionState;
    }

    private void OnDisable()
    {
        J_CarryItem.OnCarry -= CheckPillowToBeStolen;
        J_BossStateTrigger.OnShoulderTriggered -= TriggerConfusionState;
    }

    protected override void Start()
    {
        base.Start();

        _canChangeState = true;
        _currentPillow = null;

        // Get the first phase and state
        _currentState = STATE.IDLE;
        _currentPhaseIndex = 0;
        EnterState(_currentState);
        EnterPhase(_currentPhaseIndex);


        _leftHandFrequency = 0;
        _rightHandFrequency = 0;
    }

    protected override void Update()
    {
        base.Update();

        // Decrease phase timer for phases that have duration
        if (_phases[_currentPhaseIndex].hasDuration)
        {
            _currentPhaseTimer -= Time.deltaTime;

            // Transition to next state
            if (_currentPhaseTimer <= 0f && _currentPhaseIndex < _phases.Length - 1)
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
            case STATE.CONFUSED:
                break;
            case STATE.RIPPINGPILLOWS:
                break;
            case STATE.THROWINGPILLOWS:
                break;
        }
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

            // Find player
            Collider[] hitColliders = Physics.OverlapSphere(worldCenter, actualWorldRadius, _playerLayer);
            for (int j = 0; j < hitColliders.Length; ++j)
            {
                if (hitColliders[j].gameObject.CompareTag("PlayerTag"))
                {
                    hitColliders[j].GetComponent<Entity>().TakeDamage(float.MaxValue, 0.0f);
                    break;
                }
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
                        hits[i].GetComponent<Entity>().TakeDamage(_shockwaveDamage, 0.0f);
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


    public override void TakeDamage(float damageTaken, float invincibility = 0f)
    {
        if (isInvincible)
        {
            Debug.Log("is invincible");

            // Increase number of attacks ONLY for phase 2 AND not hit already
            if (_currentPhaseIndex == 1 && _currentState != STATE.HIT)
                _currentTimesHit++;

            // Enter hit state
            EnterState(STATE.HIT);
        }
        else
        {
            // Take damage
            Debug.Log("Monster took damage: " + damageTaken);

            _currentHP -= damageTaken;
            float healthPercent = _currentHP / _maxHP;
            OnHealthChanged?.Invoke(_currentHP, _maxHP);

            CheckPhaseTransition(healthPercent);
        }
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

    public override void Die()
    {
        OnDie?.Invoke();
        J_GameManager.Instance.SetCurrentScene(J_GameManager.MONSTER_SCENE);
    }

    public void EndFight()
    {
        SceneLoader.Instance.LoadScene(J_GameManager.END_SCENE);
    }


    private void EnterPhase(int phaseIndex)
    {
        if (phaseIndex >= _phases.Length)
            return;

        // Go to idle animation and reset other booleans
        if (!IsInCurrentAnimationState("Idle") && _currentPhaseIndex > 0)
        {
            _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
            _animator.SetBool(ANIMATOR_PREPARING_BOOL, false);
            _animator.SetBool(ANIMATOR_EXHAUSTED_BOOL, false);
            _animator.SetBool(ANIMATOR_CONFUSED_BOOL, false);

            _currentTimesHit = 0;
            _leftHandFrequency = 0;
            _rightHandFrequency = 0;

            _animator.SetTrigger("Reset");

            EnterState(STATE.IDLE);
        }

        // Set values
        _currentPhaseIndex = phaseIndex;
        Phase phase = _phases[phaseIndex];

        _readyingDuration = _phases[_currentPhaseIndex].durationBeforeSlamDown;

        Debug.Log($"Boss entering phase: {phase.name}");
        _phaseText.text = phase.name.ToString();

        if (phase.hasDuration)
            _currentPhaseTimer = phase.phaseTimer;
        else
            _currentPhaseTimer = 0f;

        phase.OnPhaseTransition?.Invoke();

        //InvokePhaseEnterEvent(phaseIndex);
    }

    public void InvokePhaseEnterEvent(int index)
    {
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
            J_CarryItem.Enable();
        }
    }

    private void Restart()
    {
        // TODO: Restart stage
    }




    private void CheckStateTransition()
    {
        if (!_canChangeState)
            return;

        //Debug.Log("Entering state transition");

        _currentStateTimer -= Time.deltaTime;

        switch (_currentState)
        {
            case STATE.IDLE:

                // Check for phase
                if (_currentStateTimer <= 0f)
                {
                    if (_currentPhaseIndex < 2)
                    {
                        // Enter observing state
                        EnterState(STATE.PREPARING);
                    }
                    else
                    {
                        // Increment timer only if pillow exists
                        if (_currentPillow != null)
                            _currentStealPillowTimer += Time.deltaTime;

                        // Go straight to ripping pillows
                        if (_currentStealPillowTimer >= _durationBeforeStealPillow)
                        {
                            OnStealPillow?.Invoke();
                            _currentStealPillowTimer = 0f;
                            _currentPillow = null;
                            _currentNumberOfPillowsInScene--;
                            EnterState(STATE.RIPPINGPILLOWS);
                        }
                        else
                        {
                            // Random chance of doing slam attack, throwing pillow or ripping pillow
                            List<STATE> options = new List<STATE>();

                            if (_currentNumberOfPillowsInScene < _maxNumberOfPillowsInScene) options.Add(STATE.THROWINGPILLOWS);
                            if (_currentRipPillowTimer >= _durationBeforeRipPillow) options.Add(STATE.RIPPINGPILLOWS);
                            options.Add(STATE.PREPARING);

                            int chosenOption = Random.Range(0, options.Count);

                            EnterState(options[chosenOption]);
                        }
                    }
                }

                break;
            case STATE.PREPARING:

                // Enter attacking state
                if (_currentStateTimer <= 0f)
                    EnterState(STATE.ATTACKING);

                break;
            case STATE.ATTACKING:

                // Enter idle state
                EnterState(STATE.IDLE);

                break;
            case STATE.HIT:
                
                // Check the number of times hit
                if (_currentTimesHit >= _hitsBeforeExhausted)
                    EnterState(STATE.EXHAUSTED);

                break;
            case STATE.CONFUSED:

                // Enter idle state
                if (_currentStateTimer <= 0f)
                {
                    EndConfusionState();
                    EnterState(STATE.IDLE);
                }

                break;
            case STATE.THROWINGPILLOWS:

                // Enter idle state
                EnterState(STATE.IDLE);

                break;

            case STATE.RIPPINGPILLOWS:

                // Enter idle state
                EnterState(STATE.IDLE);

                break;
            case STATE.EXHAUSTED:

                // Enter idle state
                if (_currentStateTimer <= 0f)
                    EnterState(STATE.IDLE);

                break;
        }
    }

    private void EnterState(STATE nextState)
    {
        Debug.Log("Entering state");

        switch (nextState)
        {
            case STATE.IDLE:

                // Set the duration
                _currentStateTimer = _attackCooldown;
                _animator.SetBool(ANIMATOR_EXHAUSTED_BOOL, false);
                _animator.SetBool(ANIMATOR_CONFUSED_BOOL, false);
                _animator.SetBool(ANIMATOR_IDLE_BOOL, true);

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
                _currentStateTimer = _readyingDuration;

                // Activate both planes
                for (int i = 0; i < _shockwavePlanes.Length; ++i)
                    _shockwavePlanes[i].SetActive(true);


                // Phase 1: left or right only
                if (_currentPhaseIndex == 0)
                {
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
                        float leftWeight = 100f / (_leftHandFrequency + 1);
                        float rightWeight = 100f / (_rightHandFrequency + 1);
                        float rand = Random.Range(0f, leftWeight + rightWeight);
                        _attackingHand = rand < leftWeight ? HAND.LEFT : HAND.RIGHT;
                        if (_attackingHand == HAND.LEFT) _leftHandFrequency++;
                        else _rightHandFrequency++;
                    }
                }
                // Phase 2: both hands only
                else if (_currentPhaseIndex == 1)
                {
                    _attackingHand = HAND.BOTH;
                }
                // Phase 3: left, right, or both
                else if (_currentPhaseIndex == 2)
                {
                    HAND[] options = { HAND.LEFT, HAND.RIGHT, HAND.BOTH };
                    _attackingHand = options[Random.Range(0, options.Length)];
                }



                // Disable whichever plane wasn't active
                if (_attackingHand == HAND.LEFT) _shockwavePlanes[(int)HAND.RIGHT].SetActive(false);
                else if (_attackingHand == HAND.RIGHT) _shockwavePlanes[(int)HAND.LEFT].SetActive(false);

                // Set to preparing for animator
                _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
                _animator.SetBool(ANIMATOR_PREPARING_BOOL, true);
                _animator.SetInteger(ANIMATOR_HAND_VALUE, (int)_attackingHand);

                break;
            case STATE.ATTACKING:

                _currentStateTimer = 0f;

                // Trigger attack
                _animator.SetBool(ANIMATOR_PREPARING_BOOL, false);
                _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER);

                _canChangeState = false;

                break;
            case STATE.EXHAUSTED:

                // Set the duration
                _currentStateTimer = _exhaustedDuration;
                _currentTimesHit = 0;

                _animator.SetBool(ANIMATOR_EXHAUSTED_BOOL, true);

                // Enable colliders
                for (int i = 0; i < 2; ++i)
                {
                    _fistColliders[i].enabled = true;
                    _fistColliders[i].isTrigger = false;
                }

                isInvincible = false;

                break;
            case STATE.HIT:

                // Set the pillow steal duration to 0
                _currentStealPillowTimer = 0f;

                // Set animation
                _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
                _animator.SetTrigger(ANIMATOR_HIT_TRIGGER);
                _canChangeState = false;

                break;
            case STATE.CONFUSED:

                // Set duration
                _currentStateTimer = _confusedDuration;

                // Set animation
                _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
                _animator.SetTrigger(ANIMATOR_CONFUSE_TRIGGER);
                _animator.SetBool(ANIMATOR_CONFUSED_BOOL, true);
                isInvincible = false;

                break;
            case STATE.RIPPINGPILLOWS:

                // Set the "fake" pillow active
                _fakePillow.SetActive(true);

                // Reset timer
                _currentRipPillowTimer = 0f;

                // Set animation
                _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
                _animator.SetTrigger(ANIMATOR_RIP_TRIGGER);
                _canChangeState = false;

                break;
            case STATE.THROWINGPILLOWS:

                // Set the "fake" pillow active
                _fakePillow.SetActive(true);

                // Find a destination for the pillow to go to
                FindPillowDestination();

                // Set animation
                _animator.SetBool(ANIMATOR_IDLE_BOOL, false);
                _animator.SetTrigger(ANIMATOR_THROW_TRIGGER);
                _canChangeState = false;

                break;
        }

        _currentState = nextState;
        _stateText.text = _currentState.ToString();
        Debug.Log("State: " + _currentState.ToString());
    }

    private void Idle()
    {
        // Increase duration
        _currentStealPillowTimer += Time.deltaTime;
    }

    private void Prepare() { }

    private void Attack()
    {
        // Check for player
        CheckAttackColliders();
    }

    private void Exhausted()
    {

    }





    public void StartRippingPillow()
    {
        StartCoroutine(SpawnBalls());
    }

    public void StopRippingPillow()
    {
        _fakePillow.SetActive(false);

        StopAllCoroutines();
    }

    private IEnumerator SpawnBalls()
    {
        while (true)
        {
            var cottonBall = J_SpawnManager.Instance.SpawnAtPosition("CottonBall", _fakePillow.transform.position);
            cottonBall.GetComponent<Rigidbody>().AddForce((transform.forward * Random.Range(1f, _maximumForwardForce)) + new Vector3(Random.Range(-_horizontalForce, _horizontalForce), 0f, 0f), ForceMode.Impulse);

            yield return new WaitForSeconds(_intervalBetweenRips);
        }
    }



    private void FindPillowDestination()
    {
        bool hasNearbyPillow = true;
        int attempts = 0;
        Vector3 randomPoint = Vector3.zero;

        do
        {
            hasNearbyPillow = false;
            attempts++;

            // Get random point in local space of the collider
            Vector3 localPoint = new Vector3(
                Random.Range(-_throwRangeCollider.size.x / 2f, _throwRangeCollider.size.x / 2f),
                 (-_throwRangeCollider.size.y / 2f) + (_fakePillow.GetComponent<BoxCollider>().size.y),
                Random.Range(-_throwRangeCollider.size.z / 2f, _throwRangeCollider.size.z / 2f)
            ) + _throwRangeCollider.center;

            // Convert to world space
            randomPoint = _throwRangeCollider.transform.TransformPoint(localPoint);

            // Check if any pillows are nearby
            Vector3 pillowHalfSize = _fakePillow.GetComponent<BoxCollider>().size / 2f;
            Collider[] hits = Physics.OverlapBox(randomPoint, pillowHalfSize, Quaternion.identity);

            foreach (Collider hit in hits)
            {
                if (hit.GetComponent<J_Pillow>() != null)
                {
                    hasNearbyPillow = true;
                    break;
                }
            }

        } while (hasNearbyPillow && attempts < 10);

        _throwDestination = randomPoint;
    }

    public void ThrowPillow()
    {
        // Disable the "fake" pillow
        _fakePillow.SetActive(false);

        // Spawn a new pillow and throw it to its destination
        var newPillow = J_SpawnManager.Instance.SpawnAtPosition("Pillow", _fakePillow.transform.position);
        StartCoroutine(TranslateObject(newPillow));
        _currentNumberOfPillowsInScene++;
    }

    private IEnumerator TranslateObject(GameObject pillow)
    {
        float t = 0f;
        float duration = _throwSpeed;
        Vector3 startPos = pillow.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);

            Vector3 currentPos = Vector3.Lerp(startPos, _throwDestination, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 10f;

            pillow.transform.position = currentPos;

            yield return null;
        }

        pillow.transform.position = _throwDestination;
        pillow.GetComponent<J_Pillow>().ReachDestination();
    }


    
    private void CheckPillowToBeStolen(J_Pillow pillow)
    {
        // Set the pillow
        _currentPillow = pillow;
        if (_currentPillow == null)
            _currentStealPillowTimer = 0f;
    }




    private void TriggerConfusionState(CapsuleCollider collider)
    {
        if (_currentState == STATE.CONFUSED)
            return;

        if (_transportPlayerCoroutine != null)
            return;

        _transportPlayerCoroutine = TransportPlayer(collider);
        StartCoroutine(_transportPlayerCoroutine);
    }

    private void EndConfusionState()
    {
        if (_transportPlayerCoroutine != null)
            return;

        _transportPlayerCoroutine = ReturnPlayerToArena();
        StartCoroutine(ReturnPlayerToArena());
    }

    private IEnumerator TransportPlayer(CapsuleCollider collider)
    {
        EnterState(STATE.CONFUSED);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        // Switch off the rigidbody
        player.GetComponent<Rigidbody>().useGravity = false;
        player.GetComponent <Rigidbody>().linearVelocity = Vector3.zero;

        Vector3 closestPoint = Physics.ClosestPoint(
            player.transform.position,
            collider,
            collider.transform.position,
            collider.transform.rotation
        );

        float t = 0f;
        float duration = 1f;
        Vector3 startPos = player.transform.position;

        while (t < 1f)
        {
            closestPoint = Physics.ClosestPoint(
                player.transform.position,
                collider,
                collider.transform.position,
                collider.transform.rotation
            );

            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);

            player.transform.position = Vector3.Lerp(startPos, closestPoint, t);

            yield return null;
        }

        OnTransportPlayer?.Invoke(collider);
        player.transform.position = closestPoint;
        _transportPlayerCoroutine = null;
    }

    private IEnumerator ReturnPlayerToArena()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        Vector3 colliderTop = _throwRangeCollider.transform.TransformPoint(
            _throwRangeCollider.center + new Vector3(0f, _throwRangeCollider.size.y / 2f, 0f)
        );

        float t = 0f;
        float duration = 1f;
        Vector3 startPos = player.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);
            player.transform.position = Vector3.Lerp(startPos, colliderTop, t);
            yield return null;
        }

        player.transform.position = colliderTop;
        OnTransportPlayer?.Invoke(null);
        player.GetComponent<Rigidbody>().useGravity = true;
        _transportPlayerCoroutine = null;
    }


    public void AllowStateTransition()
    {
        _canChangeState = true;
        DisableAllColliders();
    }

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
    private bool IsInCurrentAnimationState(string stateName)
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    // DEBUG
    [ContextMenu("Start Next Phase")]
    public void StartNextPhase()
    {
        if (_currentPhaseIndex + 1 == _phases.Length)
        {
            // Kill off entity
            isInvincible = false;
            TakeDamage(10000000, 0);
            return;
        }

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
        return;

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

        Gizmos.DrawWireSphere(_throwDestination, 1f);
    }
}
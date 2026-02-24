using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public enum DogStates
{
    EnterIdle,
    Idle,
    ExitIdle,
    EnterChase,
    Chase,
    ExitChase,
    EnterBite,
    Bite,
    ExitBite,
    EnterClaw,
    Claw,
    ExitClaw,
    EnterDash,
    Dash,
    ExitDash,
    EnterPingPongShit,
    PingPongShit,
    ExitPingPongShit,
    EnterDoubleClaw,
    DoubleClaw,
    ExitDoubleClaw,
    EnterDead,
    Dead
}

public class Dog : Entity
{
    [Header("Dog Stats")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float walkRange;
    [SerializeField] private float stopRange;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float baseRotateMultiplier;
    [SerializeField] private float rotateMultiplier;
    [SerializeField] private float baseSpeed;
    [SerializeField] private float walkSpeedMultiplier;
    [SerializeField] private float runSpeedMultiplier;
    [SerializeField] private float dashSpeedMultiplier;
    [SerializeField] private float idleSpeedMultiplier;
    [SerializeField] private float maxRotate;

    [Header("Attack Settings")]
    [SerializeField] private bool phase2;
    [SerializeField] private Vector2 attackFrequency;
    [SerializeField] private float dogLookaheadDistance;
    [SerializeField] private float minimumAngleToAttack;
    [SerializeField] private Vector2 bounceAmount;
    private Vector3 dashDirection;
    private int lastAttack = -1;
    private int lastlastAttack = -1;
    private float attackCooldown;
    private float stateTimer;
    private bool attackFinished;
    private bool attackReady;
    private bool dashing;
    [SerializeField] private Vector3 _attackColliderOffset;
    [SerializeField] private float _attackColliderRadius;
    [SerializeField] private Vector3 _dashColliderOffset1;
    [SerializeField] private Vector3 _dashColliderOffset2;
    [SerializeField] private float _dashColliderRadius;
    [SerializeField] private Vector2 _dashHitIntensity;
    [SerializeField] private float _dashHitStunDuration;
    [SerializeField] private float _dashHitDamage;
    [SerializeField] private float _clawHitDamage;
    [SerializeField] private float _doubleClawHitDamage;

    [Header("Misc")]
    [SerializeField] private ParticleSystem wind;
    [SerializeField] private bool canAttack;
    public bool canMove { get => _canMove; set => _canMove = value; }
    [SerializeField] private bool _canMove;
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject target;
    private CharacterController dogController;
    [SerializeField] private PlayableDirector phase2Cutscene;
    [SerializeField] private PlayableDirector deadCutscene;
    [SerializeField] private Volume _globalVolume;
    private ShockwaveDistortionVolume _distortionVolume;
    [SerializeField] private float _gravity;
    [SerializeField] private bool _attackPlayer;
    [SerializeField] private bool _attackedPlayer;
    [SerializeField] private GameObject _boneRoot;
    [SerializeField] private Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
    [SerializeField] private GameObject _afterImagePrefab;
    [SerializeField] private float _afterImageInterval;
    [SerializeField] private float _afterImageFadeInDuration;
    [SerializeField] private float _afterImageHoldDuration;
    [SerializeField] private float _afterImageFadeOutDuration;
    [SerializeField] private float _afterImageTargetAlpha;
    [ColorUsage(true, true)]
    [SerializeField] private Color _afterImageColor;
    private Coroutine _afterImageCoroutine;
    [SerializeField] private ParticleSystem[] dizzyParticles;
    [SerializeField] private SkinnedMeshRenderer[] _meshRenderers;
    private List<Material> _warningMaterials = new List<Material>();
    [SerializeField] private float _warningTargetAlpha;
    [ColorUsage(true, true)]
    [SerializeField] private Color _warningColor;
    private Coroutine _warningCoroutine;

    [Header("Debugging")]
    [SerializeField] private DogStates currentState = DogStates.EnterIdle;
    [SerializeField] private DogStates nextState;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentRotate;
    [SerializeField] private int[] attackTimes = new int[4] { 0, 0, 0, 0 };
    [SerializeField] private int bounces;
    [SerializeField] private int bounced;
    [SerializeField] private float idleSpeed;
    [SerializeField] private float currentSpeedMultiplier;


    public void StartDistortion(float loopDuration)
    {
        StartCoroutine(DistortionEffectUpdate(0.5f, loopDuration));
    }

    public IEnumerator DistortionEffectUpdate(float startEndDuration, float loopDuration)
    {
        yield return StartCoroutine(DistortionEffectStart(startEndDuration));

        float timer = 0;
        float speed = /*Random.Range(1f, 3f)*/ 1;
        float upTimer = 0;
        float target = Random.Range(0.25f, 1);
        float baseIntensity = 1;

        while (timer < loopDuration)
        {
            timer += Time.unscaledDeltaTime;
            upTimer += Time.unscaledDeltaTime * speed;

            if (upTimer >= 1)
            {
                upTimer = 0;
                speed = Random.Range(5f, 10f);
                target = Random.Range(0.25f, 1);
                baseIntensity = _distortionVolume.intensity.value;
            }

            _distortionVolume.intensity.value = Mathf.Lerp(baseIntensity, target, upTimer);

            yield return null;
        }

        yield return StartCoroutine(DistortionEffectEnd(startEndDuration, _distortionVolume.intensity.value));
    }

    public IEnumerator DistortionEffectStart(float startEndDuration)
    {
        float timer = 0;

        while (timer < startEndDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (timer > startEndDuration) timer = startEndDuration;

            _distortionVolume.intensity.value = Mathf.Lerp(0, 1, timer);

            yield return null;
        }
    }

    public IEnumerator DistortionEffectEnd(float startEndDuration, float startIntensity)
    {
        float timer = startEndDuration;

        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;

            if (timer < 0) timer = 0;

            _distortionVolume.intensity.value = Mathf.Lerp(0, startIntensity, timer);

            yield return null;
        }
    }

    private void OnEnable()
    {
        if (_globalVolume.sharedProfile.TryGet<ShockwaveDistortionVolume>(out _distortionVolume))
        {
            _distortionVolume.intensity.value = 0f;
        }
    }

    private void OnDisable()
    {
        if (_globalVolume.sharedProfile.TryGet<ShockwaveDistortionVolume>(out _distortionVolume))
        {
            _distortionVolume.intensity.value = 0f;
        }
    }

    public IEnumerator SpawnAfterImages()
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= _afterImageInterval)
            {
                timer = 0;

                GameObject afterImage = Instantiate(_afterImagePrefab, transform.position, transform.rotation);
                afterImage.GetComponent<AfterImages>().Initialise(_bones, _afterImageFadeInDuration, _afterImageHoldDuration, _afterImageFadeOutDuration, _afterImageTargetAlpha, _afterImageColor);
            }

            yield return null;
        }
    }

    public void PlayWarning(float fadeInDuration)
    {
        if (_warningCoroutine != null)
            StopCoroutine(_warningCoroutine);

        _warningCoroutine = StartCoroutine(WarningStart(fadeInDuration));
    }

    public void StopWarning(float fadeOutDuration)
    {
        if (_warningCoroutine != null)
            StopCoroutine(_warningCoroutine);

        _warningCoroutine = StartCoroutine(WarningEnd(fadeOutDuration));
    }

    public IEnumerator WarningStart(float fadeInDuration)
    {
        if (_warningMaterials != null && _warningMaterials.Count > 0)
        {
            float timer = _warningMaterials[0].GetFloat("_Alpha") / _warningTargetAlpha;

            while (timer < fadeInDuration)
            {
                timer += Time.unscaledDeltaTime;

                if (timer > fadeInDuration) timer = fadeInDuration;

                float percentage = timer / fadeInDuration;

                UpdateWarningMaterials(percentage);

                yield return null;
            }
        }
    }

    public IEnumerator WarningEnd(float fadeOutDuration)
    {
        if (_warningMaterials != null && _warningMaterials.Count > 0)
        {
            float timer = _warningMaterials[0].GetFloat("_Alpha") / _warningTargetAlpha;

            while (timer > 0)
            {
                timer -= Time.unscaledDeltaTime;

                if (timer < 0) timer = 0;

                float percentage = timer / fadeOutDuration;

                UpdateWarningMaterials(percentage);

                yield return null;
            }
        }
    }

    private void UpdateWarningMaterials(float value)
    {
        foreach (var mat in _warningMaterials)
        {
            mat.SetFloat("_Alpha", value * _warningTargetAlpha);
        }
    }

    new void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        dogController = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updatePosition = false;


        _bones.Clear();
        Transform[] allBones = _boneRoot.GetComponentsInChildren<Transform>();

        foreach (var bone in allBones)
        {
            _bones[bone.name] = bone;
        }

        _warningMaterials.Clear();
        foreach (var renderer in _meshRenderers)
        {
            Material[] mats = renderer.materials;

            if (mats.Length > 2)
            {
                mats[2].SetColor("_Color", _warningColor);

                renderer.materials = mats;
                _warningMaterials.Add(mats[2]);
            }
        }
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (!_canMove)
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= Time.deltaTime;
                currentSpeed = Mathf.Clamp01(currentSpeed);
            }

            if (currentRotate != 0)
            {
                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                animator.SetBool("Right", currentRotate > 0);
                animator.SetBool("Left", currentRotate < 0);
            }

            if (dogController.enabled)
                dogController.Move(Vector3.up * _gravity * Time.deltaTime);

            return;
        }

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        switch (currentState)
        {
            case DogStates.EnterIdle:
                currentState = DogStates.Idle;
                dogController.excludeLayers = 0;
                break;
            case DogStates.Idle:
                Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
                GameObject closestPlayer = null;
                float closestDistance = Mathf.Infinity;

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("PlayerTag"))
                    {
                        float distance = (transform.position - collider.transform.position).magnitude;

                        if (distance < closestDistance)
                        {
                            closestPlayer = collider.gameObject;
                            closestDistance = distance;
                        }
                    }
                }

                if (closestPlayer != null)
                {
                    target = closestPlayer;
                    nextState = DogStates.EnterChase;
                    currentState = DogStates.ExitIdle;
                    return;
                }

                if (currentSpeed > 0)
                {
                    currentSpeed -= Time.deltaTime;
                    currentSpeed = Mathf.Clamp01(currentSpeed);
                }

                if (Mathf.Abs(currentRotate) > 0.02f && currentSpeed < 0.25f)
                {
                    idleSpeed += Time.deltaTime * Mathf.Abs(currentRotate) * idleSpeedMultiplier;
                    idleSpeed = Mathf.Clamp(idleSpeed, 0, 0.4f);
                }

                idleSpeed -= Time.deltaTime;
                idleSpeed = Mathf.Clamp01(idleSpeed);

                animator.SetFloat("Move", Mathf.Clamp01(currentSpeed + idleSpeed));
                dogController.Move(transform.forward * currentSpeed * baseSpeed  * Time.deltaTime);

                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                animator.SetBool("Right", false);
                animator.SetBool("Left", false);

                attackCooldown -= Time.deltaTime;

                break;
            case DogStates.ExitIdle:
                currentState = nextState;
                stateTimer = 0;
                break;
            case DogStates.EnterChase:
                currentState = DogStates.Chase;
                dogController.excludeLayers = 0;
                break;
            case DogStates.Chase:
                if (target != null)
                {
                    float distance = (transform.position - target.transform.position).magnitude;

                    if (distance > detectionRange)
                    {
                        target = null;
                        nextState = DogStates.EnterIdle;
                        currentState = DogStates.ExitChase;
                        break;
                    }
                    else
                    {
                        Vector3 direction = target.transform.position - transform.position;
                        direction.y = 0;
                        direction.Normalize();

                        float angleToTarget = Vector3.Angle(transform.forward, direction);

                        if (distance > walkRange)
                        {
                            if (currentSpeed < 1)
                            {
                                currentSpeed += Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 1);
                            }

                            if (attackCooldown <= 0 && canAttack)
                            {
                                stateTimer += Time.deltaTime;
                                if (angleToTarget < minimumAngleToAttack)
                                {
                                    List<int> availableAttacks = new List<int>();

                                    for (int i = 0; i < attackTimes.Length; i++)
                                    {
                                        if (!phase2 && i == 3
                                            || (i == lastAttack && i == lastlastAttack)
                                            || i == 0 || i == 1) continue;

                                        bool attackAvailable = true;

                                        for (int j = 0; j < attackTimes.Length; j++)
                                        {
                                            if ((!phase2 && j == 3) || i == j
                                                && (j == lastAttack && j == lastlastAttack)) continue;

                                            if (attackTimes[i] > attackTimes[j] + 1)
                                            {
                                                attackAvailable = false;
                                                break;
                                            }
                                        }

                                        if (attackAvailable)
                                            availableAttacks.Add(i);
                                    }

                                    if (availableAttacks.Count > 0)
                                    {
                                        int randomAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
                                        switch (randomAttack)
                                        {
                                            case 0:
                                                nextState = DogStates.EnterBite;
                                                break;
                                            case 1:
                                                nextState = DogStates.EnterClaw;
                                                break;
                                            case 2:
                                                nextState = DogStates.EnterDash;
                                                break;
                                            case 3:
                                                nextState = DogStates.EnterPingPongShit;
                                                break;
                                        }

                                        lastlastAttack = lastAttack;
                                        lastAttack = randomAttack;

                                        currentState = DogStates.ExitChase;
                                        attackTimes[randomAttack]++;
                                        return;
                                    }
                                }
                            }

                            rotateMultiplier = 1.5f;
                            currentSpeedMultiplier = runSpeedMultiplier;
                        }
                        else if (distance > stopRange)
                        {
                            if (currentSpeed > 0.5f)
                            {
                                currentSpeed -= Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0.5f, 1);
                            }
                            else if (currentSpeed < 0.5f)
                            {
                                currentSpeed += Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 0.5f);
                            }

                            if (attackCooldown <= 0 && canAttack)
                            {
                                stateTimer += Time.deltaTime;
                                if (angleToTarget < minimumAngleToAttack)
                                {
                                    List<int> availableAttacks = new List<int>();

                                    for (int i = 0; i < attackTimes.Length; i++)
                                    {
                                        if (!phase2 && i == 3
                                            || (i == lastAttack && i == lastlastAttack)
                                            || i == 0 || i == 1) continue;

                                        bool attackAvailable = true;

                                        for (int j = 0; j < attackTimes.Length; j++)
                                        {
                                            if ((!phase2 && j == 3) || i == j
                                                && (j == lastAttack && j == lastlastAttack)) continue;

                                            if (attackTimes[i] > attackTimes[j] + 1)
                                            {
                                                attackAvailable = false;
                                                break;
                                            }
                                        }

                                        if (attackAvailable)
                                            availableAttacks.Add(i);
                                    }

                                    if (availableAttacks.Count > 0)
                                    {
                                        int randomAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
                                        switch (randomAttack)
                                        {
                                            case 0:
                                                nextState = DogStates.EnterBite;
                                                break;
                                            case 1:
                                                nextState = DogStates.EnterClaw;
                                                break;
                                            case 2:
                                                nextState = DogStates.EnterDash;
                                                break;
                                            case 3:
                                                nextState = DogStates.EnterPingPongShit;
                                                break;
                                        }

                                        lastlastAttack = lastAttack;
                                        lastAttack = randomAttack;

                                        currentState = DogStates.ExitChase;
                                        attackTimes[randomAttack]++;
                                        return;
                                    }
                                }
                            }

                            rotateMultiplier = 1;
                            currentSpeedMultiplier = walkSpeedMultiplier;
                        }
                        else
                        {
                            if (currentSpeed > 0)
                            {
                                currentSpeed -= Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 0.5f);
                            }

                            if (Mathf.Abs(currentRotate) > 0.02f && currentSpeed < 0.25f)
                            {
                                idleSpeed += Time.deltaTime * Mathf.Abs(currentRotate) * idleSpeedMultiplier;
                                idleSpeed = Mathf.Clamp(idleSpeed, 0, 0.4f);
                            }

                            if (attackCooldown <= 0 && canAttack)
                            {
                                stateTimer += Time.deltaTime;
                                if (angleToTarget < minimumAngleToAttack)
                                {
                                    List<int> availableAttacks = new List<int>();

                                    for (int i = 0; i < attackTimes.Length; i++)
                                    {
                                        if (!phase2 && i == 3
                                            || (i == lastAttack && i == lastlastAttack)) continue;

                                        bool attackAvailable = true;

                                        for (int j = 0; j < attackTimes.Length; j++)
                                        {
                                            if ((!phase2 && j == 3) || i == j
                                                && (j == lastAttack && j == lastlastAttack)) continue;

                                            if (attackTimes[i] > attackTimes[j] + 1)
                                            {
                                                attackAvailable = false;
                                                break;
                                            }
                                        }

                                        if (attackAvailable)
                                            availableAttacks.Add(i);
                                    }

                                    if (availableAttacks.Count > 0)
                                    {
                                        int randomAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
                                        switch (randomAttack)
                                        {
                                            case 0:
                                                nextState = DogStates.EnterBite;
                                                break;
                                            case 1:
                                                nextState = DogStates.EnterClaw;
                                                break;
                                            case 2:
                                                nextState = DogStates.EnterDash;
                                                break;
                                            case 3:
                                                nextState = DogStates.EnterPingPongShit;
                                                break;
                                        }

                                        lastlastAttack = lastAttack;
                                        lastAttack = randomAttack;

                                        currentState = DogStates.ExitChase;
                                        attackTimes[randomAttack]++;
                                        return;
                                    }
                                }
                            }

                            rotateMultiplier = 1;
                            currentSpeedMultiplier = 1;
                        }

                        idleSpeed -= Time.deltaTime;
                        idleSpeed = Mathf.Clamp01(idleSpeed);

                        agent.SetDestination(target.transform.position);

                        Vector3 steeringDir = Vector3.zero;

                        if (idleSpeed == 0f)
                        {
                            steeringDir = agent.desiredVelocity.normalized;
                        }
                        else
                        {
                            steeringDir = direction;
                        }

                        float angle = Vector3.SignedAngle(transform.forward, steeringDir, Vector3.up);
                        float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);

                        if (steeringDir != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(steeringDir);

                            transform.rotation = Quaternion.RotateTowards(
                                transform.rotation,
                                targetRotation,
                                baseRotateMultiplier * rotateMultiplier * Time.deltaTime
                            );
                        }

                        currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                        //transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime, 0);

                        float rotationOffset = 90f * currentRotate;
                        Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                        animator.SetFloat("Move", Mathf.Clamp01(currentSpeed + idleSpeed));
                        dogController.Move(adjustedForward * currentSpeed * baseSpeed * currentSpeedMultiplier * Time.deltaTime);
                        agent.nextPosition = transform.position;

                        animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                        animator.SetBool("Right", currentRotate > 0);
                        animator.SetBool("Left", currentRotate < 0);

                        attackCooldown -= Time.deltaTime;
                    }
                }
                else
                {
                    nextState = DogStates.EnterIdle;
                    currentState = DogStates.ExitChase;
                    return;
                }
                break;
            case DogStates.ExitChase:
                currentState = nextState;
                break;
            case DogStates.EnterBite:
                agent.enabled = false;

                animator.SetTrigger("Bite");
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.Bite;
                Debug.Log("Entering Bite State");
                dogController.excludeLayers = 0;
                break;
            case DogStates.Bite:
                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                animator.SetBool("Right", currentRotate > 0);
                animator.SetBool("Left", currentRotate < 0);

                if (attackReady)
                {
                    animator.SetTrigger("Attack");
                    attackReady = false;
                }

                if (attackFinished)
                {
                    nextState = DogStates.EnterChase;
                    currentState = DogStates.ExitBite;
                    return;
                }

                if (_attackPlayer && !_attackedPlayer)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.TransformPoint(_attackColliderOffset), _attackColliderRadius);
                    foreach (var hitCollider in hitColliders)
                    {
                        if (hitCollider.CompareTag("PlayerTag") && !_attackedPlayer)
                        {
                            Debug.Log("Hit Player with Bite");
                            hitCollider.GetComponent<Entity>().TakeDamage(_clawHitDamage, 0.0f);
                            _attackedPlayer = true;
                        }
                    }
                }

                break;
            case DogStates.ExitBite:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = Random.Range(attackFrequency.x, attackFrequency.y);
                currentState = nextState;
                Debug.Log("Exiting Bite State");
                break;
            case DogStates.EnterClaw:
                agent.enabled = false;

                animator.SetTrigger("Claw");
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.Claw;
                dogController.excludeLayers = 0;

                Debug.Log("Entering Claw State");
                break;
            case DogStates.Claw:
                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));

                if (attackReady)
                {
                    animator.SetTrigger("Attack");
                    attackReady = false;
                }

                if (attackFinished)
                {
                    nextState = DogStates.EnterChase;
                    currentState = DogStates.ExitClaw;
                    return;
                }

                if (_attackPlayer && !_attackedPlayer)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.TransformPoint(_attackColliderOffset), _attackColliderRadius);
                    foreach (var hitCollider in hitColliders)
                    {
                        if (hitCollider.CompareTag("PlayerTag") && !_attackedPlayer)
                        {
                            Debug.Log("Hit Player with Claw");
                            hitCollider.GetComponent<Entity>().TakeDamage(_clawHitDamage, 0.0f);
                            _attackedPlayer = true;
                        }
                    }
                }

                break;
            case DogStates.ExitClaw:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = Random.Range(attackFrequency.x, attackFrequency.y);
                currentState = nextState;
                Debug.Log("Exiting Claw State");
                break;
            case DogStates.EnterDash:
                agent.enabled = false;

                animator.SetTrigger("Prep");
                dashing = false;
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.Dash;
                currentSpeed = 0;
                dogController.excludeLayers = 0;

                Debug.Log("Entering Dash State");
                break;
            case DogStates.Dash:
                if ((stateInfo.IsName("Prep") || animator.GetBool("Prep")) && !animator.GetBool("Attack"))
                {
                    currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                    animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                    animator.SetBool("Right", currentRotate > 0);
                    animator.SetBool("Left", currentRotate < 0);

                    if (attackReady)
                    {
                        animator.SetTrigger("Attack");
                        attackReady = false;
                        dashing = true;
                        dashDirection = target.transform.position - transform.position;
                        dashDirection.y = 0;
                        dashDirection.Normalize();
                        rotateMultiplier = 1.5f;
                        currentSpeedMultiplier = dashSpeedMultiplier;
                        wind.Play();
                        SetAttackPlayer(1);
                        dogController.excludeLayers = LayerMask.GetMask("Player");

                        if (_afterImageCoroutine != null)
                            StopCoroutine(_afterImageCoroutine);

                        _afterImageCoroutine = StartCoroutine(SpawnAfterImages());

                        PlayWarning(0.5f);
                    }
                }
                else
                {
                    if (attackFinished)
                    {
                        nextState = DogStates.EnterChase;
                        currentState = DogStates.ExitDash;

                        foreach (var dizzyParticle in dizzyParticles)
                        {
                            dizzyParticle.Stop();
                        }

                        StopWarning(0.5f);

                        return;
                    }

                    if (dashing)
                    {
                        currentSpeed += Time.deltaTime * 5;
                        currentSpeed = Mathf.Clamp01(currentSpeed);
                        animator.SetFloat("Move", Mathf.Clamp01(currentSpeed));

                        float angle = Vector3.SignedAngle(transform.forward, dashDirection, Vector3.up);
                        float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);

                        if (dashDirection != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(dashDirection);

                            transform.rotation = Quaternion.RotateTowards(
                                transform.rotation,
                                targetRotation,
                                baseRotateMultiplier * rotateMultiplier * Time.deltaTime
                            );
                        }

                        currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                        //transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime, 0);


                        float rotationOffset = 90f * currentRotate;
                        Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                        dogController.Move(adjustedForward * currentSpeed * baseSpeed * currentSpeedMultiplier * Time.deltaTime);

                        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, dogLookaheadDistance, LayerMask.GetMask("Wall")))
                        {
                            animator.SetTrigger("Wince");
                            dashing = false;
                            wind.Stop();
                            dogController.excludeLayers = 0;

                            if (_afterImageCoroutine != null)
                                StopCoroutine(_afterImageCoroutine);

                            foreach (var dizzyParticle in dizzyParticles)
                            {
                                dizzyParticle.Play();
                            }

                            StopWarning(0.5f);
                        }

                        if (_attackPlayer && !_attackedPlayer)
                        {
                            Vector3 p1 = transform.TransformPoint(_dashColliderOffset1);
                            Vector3 p2 = transform.TransformPoint(_dashColliderOffset2);

                            Collider[] hitColliders = Physics.OverlapCapsule(p1, p2, _dashColliderRadius);

                            foreach (var hitCollider in hitColliders)
                            {
                                if (hitCollider.CompareTag("PlayerTag"))
                                {
                                    Debug.Log("Hit Player during Dash");
                                    _attackedPlayer = true;
                                    hitCollider.GetComponent<Entity>().TakeDamage(_dashHitDamage, 0.0f);
                                    hitCollider.GetComponent<PlayerController>().Stun(_dashHitStunDuration);
                                    Vector3 dogToPlayer = hitCollider.transform.position - transform.position;
                                    dogToPlayer.y = 0;
                                    dogToPlayer.Normalize();
                                    hitCollider.GetComponent<Rigidbody>().AddForce(dogToPlayer * _dashHitIntensity.x + Vector3.up * _dashHitIntensity.y, ForceMode.Impulse);
                                }
                            }
                        }
                    }
                }

                break;
            case DogStates.ExitDash:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = Random.Range(attackFrequency.x, attackFrequency.y);
                currentState = nextState;
                wind.Stop();
                _attackPlayer = false;
                Debug.Log("Exiting Dash State");
                break;
            case DogStates.EnterPingPongShit:
                agent.enabled = false;

                animator.SetTrigger("Prep");
                dashing = false;
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.PingPongShit;
                bounces = Random.Range((int)bounceAmount.x, (int)bounceAmount.y + 1);
                bounced = 0;
                dogController.excludeLayers = 0;

                Debug.Log("Entering Ping Pong State with " + bounces + " bounces");
                break;
            case DogStates.PingPongShit:
                if ((stateInfo.IsName("Prep") || animator.GetBool("Prep")) && !animator.GetBool("Attack"))
                {
                    currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                    animator.SetFloat("Turn", Mathf.Abs(currentRotate));

                    if (attackReady)
                    {
                        animator.SetTrigger("Attack");
                        attackReady = false;
                        dashing = true;
                        dashDirection = target.transform.position - transform.position;
                        dashDirection.y = 0;
                        dashDirection.Normalize(); rotateMultiplier = 2f;
                        currentSpeedMultiplier = dashSpeedMultiplier;
                        wind.Play();
                        SetAttackPlayer(1);
                        dogController.excludeLayers = LayerMask.GetMask("Player");

                        if (_afterImageCoroutine != null)
                            StopCoroutine(_afterImageCoroutine);

                        _afterImageCoroutine = StartCoroutine(SpawnAfterImages());

                        PlayWarning(0.5f);
                    }
                }
                else
                {
                    if (dashing)
                    {
                        if (bounced >= bounces)
                        {
                            dashDirection = target.transform.position - transform.position;
                            dashDirection.y = 0;
                            dashDirection.Normalize();
                            float currentRotationOffset = 90f * currentRotate;
                            Vector3 currentAdjustedForward = Quaternion.AngleAxis(currentRotationOffset, Vector3.up) * transform.forward;
                            float angleToTarget = Vector3.Angle(currentAdjustedForward, dashDirection);

                            if ((target.transform.position - transform.position).magnitude > stopRange || angleToTarget > minimumAngleToAttack)
                            {
                                rotateMultiplier = 3f;

                                currentSpeed += Time.deltaTime * 5;
                                currentSpeed = Mathf.Clamp01(currentSpeed);
                                animator.SetFloat("Move", Mathf.Clamp01(currentSpeed));

                                agent.enabled = true;
                                agent.SetDestination(target.transform.position);

                                float angle = Vector3.SignedAngle(transform.forward, agent.desiredVelocity.normalized, Vector3.up);
                                float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);

                                if (dashDirection != Vector3.zero)
                                {
                                    Quaternion targetRotation = Quaternion.LookRotation(dashDirection);

                                    transform.rotation = Quaternion.RotateTowards(
                                        transform.rotation,
                                        targetRotation,
                                        baseRotateMultiplier * rotateMultiplier * Time.deltaTime * 3
                                    );
                                }

                                currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                                //transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime * 3, 0);

                                float rotationOffset = 90f * currentRotate;
                                Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                                dogController.Move(adjustedForward * currentSpeed * baseSpeed * currentSpeedMultiplier * Time.deltaTime);
                                agent.nextPosition = transform.position;

                                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                                animator.SetBool("Right", currentRotate > 0);
                                animator.SetBool("Left", currentRotate < 0);
                            }
                            else
                            {
                                nextState = DogStates.EnterDoubleClaw;
                                currentState = DogStates.ExitPingPongShit;
                                return;
                            }
                        }
                        else
                        {
                            currentSpeed += Time.deltaTime * 5;
                            currentSpeed = Mathf.Clamp01(currentSpeed);
                            animator.SetFloat("Move", Mathf.Clamp01(currentSpeed));

                            float angle = Vector3.SignedAngle(transform.forward, dashDirection, Vector3.up);
                            float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);

                            if (dashDirection != Vector3.zero)
                            {
                                Quaternion targetRotation = Quaternion.LookRotation(dashDirection);

                                transform.rotation = Quaternion.RotateTowards(
                                    transform.rotation,
                                    targetRotation,
                                    baseRotateMultiplier * rotateMultiplier * Time.deltaTime * 1.5f
                                );
                            }

                            currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                            //transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime * 1.5f, 0);

                            float rotationOffset = 90f * currentRotate;
                            Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                            dogController.Move(adjustedForward * currentSpeed * baseSpeed * currentSpeedMultiplier * Time.deltaTime);
                            agent.nextPosition = transform.position;

                            animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                            animator.SetBool("Right", currentRotate > 0);
                            animator.SetBool("Left", currentRotate < 0);

                            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, dogLookaheadDistance, LayerMask.GetMask("Wall")))
                            {
                                if (Vector3.Dot(hit.normal, dashDirection) <= 0)
                                {
                                    if (bounced < bounces)
                                    {
                                        bounced++;
                                        dashDirection = Vector3.Reflect(dashDirection, hit.normal);
                                        dashDirection.y = 0;
                                        dashDirection.Normalize();
                                        _attackedPlayer = false;

                                        if (bounced >= bounces)
                                        {
                                            _attackPlayer = false;
                                            dogController.excludeLayers = 0;
                                        }
                                    }
                                }
                            }

                            if (_attackPlayer && !_attackedPlayer)
                            {
                                Vector3 p1 = transform.TransformPoint(_dashColliderOffset1);
                                Vector3 p2 = transform.TransformPoint(_dashColliderOffset2);

                                Collider[] hitColliders = Physics.OverlapCapsule(p1, p2, _dashColliderRadius);

                                foreach (var hitCollider in hitColliders)
                                {
                                    if (hitCollider.CompareTag("PlayerTag"))
                                    {
                                        Debug.Log("Hit Player during Dash");
                                        _attackedPlayer = true;
                                        hitCollider.GetComponent<Entity>().TakeDamage(_dashHitDamage, 0.0f);
                                        hitCollider.GetComponent<PlayerController>().Stun(_dashHitStunDuration);
                                        Vector3 dogToPlayer = hitCollider.transform.position - transform.position;
                                        dogToPlayer.y = 0;
                                        dogToPlayer.Normalize();
                                        hitCollider.GetComponent<Rigidbody>().AddForce(dogToPlayer * _dashHitIntensity.x + Vector3.up * _dashHitIntensity.y, ForceMode.Impulse);
                                    }
                                }
                            }
                        }
                    }
                }

                break;
            case DogStates.ExitPingPongShit:
                agent.enabled = true;
                agent.Warp(transform.position);

                currentState = nextState;
                wind.Stop();
                if (_afterImageCoroutine != null)
                    StopCoroutine(_afterImageCoroutine);

                StopWarning(0.5f);

                Debug.Log("Exiting Ping Pong");
                break;
            case DogStates.EnterDoubleClaw:
                agent.enabled = false;

                animator.SetTrigger("DoubleClaw");
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.Claw;
                dogController.excludeLayers = 0;

                Debug.Log("Entering DoubleClaw State");
                break;
            case DogStates.DoubleClaw:
                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));

                if (attackReady)
                {
                    animator.SetTrigger("Attack");
                    attackReady = false;
                }

                if (attackFinished)
                {
                    nextState = DogStates.EnterChase;
                    currentState = DogStates.ExitDoubleClaw;
                    return;
                }

                if (_attackPlayer && !_attackedPlayer)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.TransformPoint(_attackColliderOffset), _attackColliderRadius);
                    foreach (var hitCollider in hitColliders)
                    {
                        if (hitCollider.CompareTag("PlayerTag") && !_attackedPlayer)
                        {
                            Debug.Log("Hit player with Double Claw");
                            hitCollider.GetComponent<Entity>().TakeDamage(_doubleClawHitDamage, 0.0f);
                            _attackedPlayer = true;
                        }
                    }
                }

                break;
            case DogStates.ExitDoubleClaw:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = Random.Range(attackFrequency.x, attackFrequency.y);
                currentState = nextState;
                Debug.Log("Exiting DoubleClaw State");
                break;
            case DogStates.EnterDead:
                break;
            case DogStates.Dead:
                break;
            default:
                break;
        }

        dogController.Move(Vector3.up * _gravity * Time.deltaTime);
    }

    public void SetAttackFinished(int finished)
    {
        attackFinished = finished == 1;
    }

    public void SetAttackReady(int ready)
    {
        attackReady = ready == 1;
    }

    public void SetAttackPlayer(int attack)
    {
        _attackPlayer = attack == 1;
        _attackedPlayer = attack == 0;
    }

    public void SetDogToTransform(Transform anchor)
    {
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
    }

    public void DisableDog()
    {
        agent.enabled = false;
        dogController.enabled = false;
        _attackPlayer = false;
        wind.Stop();

        if (_afterImageCoroutine != null)
            StopCoroutine(_afterImageCoroutine);

        foreach (var dizzyParticle in dizzyParticles)
        {
            dizzyParticle.Stop();
        }

        StopWarning(0.5f);
    }

    public void ResetDog()
    {
        agent.enabled = true;
        agent.Warp(transform.position);
        dogController.enabled = true;
        currentState = DogStates.EnterIdle;
        nextState = DogStates.Idle;
        target = null;
        _attackPlayer = false;
        for (int i = 0; i < attackTimes.Length; i++)
        {
            attackTimes[i] = 0;
        }
        currentRotate = 0;
        currentSpeed = 0;
        animator.Play("Moving", 0, 0);
        animator.SetFloat("Move", 0);
        animator.SetFloat("Turn", 0);
        animator.SetBool("Right", false);
        animator.SetBool("Left", false);
        attackReady = false;
        attackFinished = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, walkRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + transform.forward * dogLookaheadDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(_attackColliderOffset), _attackColliderRadius);
        Gizmos.color = Color.magenta;
        Vector3 pointA = transform.TransformPoint(_dashColliderOffset1);
        Vector3 pointB = transform.TransformPoint(_dashColliderOffset2);
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, _dashColliderRadius);
        Gizmos.DrawWireSphere(pointB, _dashColliderRadius);
        Vector3 direction = (pointB - pointA).normalized;
        Vector3 ortho = Vector3.Cross(direction, Vector3.up).normalized * _dashColliderRadius;
        if (ortho.sqrMagnitude < 0.001f)
            ortho = Vector3.Cross(direction, Vector3.right).normalized * _dashColliderRadius;
        Gizmos.DrawLine(pointA + ortho, pointB + ortho);
        Gizmos.DrawLine(pointA - ortho, pointB - ortho);
    }

    //// Do damage without invincibility cooldown
    //public override void TakeDamage(float damageTaken)
    //{
    //    if (phase2Cutscene.state == PlayState.Playing) return;

    //    if (!isDodging)
    //    {
    //        _currentHP -= damageTaken;
    //        if (hitAudio.Length > 0 && audioSource != null)
    //        {
    //            audioSource.PlayOneShot(hitAudio[Random.Range(0, hitAudio.Length - 1)]);
    //        }
    //        if (_currentHP <= _maxHP / 2 && !phase2)
    //        {
    //            Debug.Log("Entering Phase 2");
    //            phase2Cutscene.Play();
    //            phase2 = true;
    //        }
    //        if (_currentHP <= 0)
    //        {
    //            if (audioSource != null && deathAudio != null)
    //            {
    //                audioSource.PlayOneShot(deathAudio);
    //            }
    //            Die();
    //        }
    //    }
    //}

    // Do damage with invincibility cooldown
    public override void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (phase2Cutscene.state == PlayState.Playing || deadCutscene.state == PlayState.Playing) return;

        if (!isInvincible && !isDodging && _currentHP > 0)
        {
            _currentHP -= damageTaken;
            _invincibilityMaxCooldown = invincibilityLength;
            _invincibilityCooldown = invincibilityLength;
            if (hitAudio.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(hitAudio[Random.Range(0, hitAudio.Length - 1)]);
            }
            if (_currentHP <= 0)
            {
                if (audioSource != null && deathAudio != null)
                {
                    audioSource.PlayOneShot(deathAudio);
                }
                Die();
            }
            else
            {
                if (_currentHP <= _maxHP / 2 && !phase2)
                {
                    Debug.Log("Entering Phase 2");
                    phase2Cutscene.Play();
                    phase2 = true;
                }

                if (_invincibilityCooldown > 0)
                {
                    isInvincible = true;
                }
            }
        }
    }

    // Set gameobject to be inactive
    public override void Die()
    {
        currentRotate = 0;
        currentSpeed = 0;
        animator.SetFloat("Move", 0);
        animator.SetFloat("Turn", 0);
        animator.SetBool("Right", false);
        animator.SetBool("Left", false);

        deadCutscene.Play();
        currentState = DogStates.EnterDead;

        Debug.Log("die");

        onDieEvent?.Invoke();
        J_GameManager.Instance.SetCurrentScene(J_GameManager.DOG_SCENE);
        //SceneLoader.Instance.LoadScene(J_GameManager.REST_SCENE);

        //gameObject.SetActive(false);
    }

    public void LoadIntoRestScene()
    {
        SceneLoader.Instance.LoadScene(J_GameManager.REST_SCENE);
    }
}

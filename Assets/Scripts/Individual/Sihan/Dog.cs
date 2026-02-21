using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    EnterDead,
    Dead
}

public class Dog : MonoBehaviour
{
    [SerializeField] private DogStates currentState = DogStates.EnterIdle;
    [SerializeField] private DogStates nextState;
    private Animator animator;
    private GameObject target;
    [SerializeField] private float detectionRange;
    [SerializeField] private float walkRange;
    [SerializeField] private float stopRange;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float baseRotateMultiplier;
    [SerializeField] private float rotateMultiplier;
    [SerializeField] private float speed;
    [SerializeField] private float idleSpeedMultiplier;
    private float currentSpeed;
    private float idleSpeed;
    private float currentRotate;
    [SerializeField] private float maxRotate;
    private CharacterController dogController;
    [SerializeField] private int[] attackTimes = new int[4] { 0, 0, 0, 0 };
    private float attackCooldown;
    private float stateTimer;
    [SerializeField] private float attackFrequency = 2.0f;
    [SerializeField] private bool phase2;
    private bool attackFinished;
    private bool attackReady;
    [SerializeField] private float dogLookaheadDistance;
    private bool dashing;
    [SerializeField] private float minimumAngleToAttack;
    [SerializeField] private Vector2 bounceAmount;
    [SerializeField] private int bounces;
    [SerializeField] private int bounced;
    private Vector3 dashDirection;
    private int lastAttack = -1;
    private int lastlastAttack = -1;
    [SerializeField] private ParticleSystem wind;
    private NavMeshAgent agent;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool _canMove;
    public bool canMove { get => _canMove; set => _canMove = value; }

    void Start()
    {
        animator = GetComponent<Animator>();
        dogController = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updatePosition = false;
    }

    // Update is called once per frame
    void Update()
    {
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

            return;
        }

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        switch (currentState)
        {
            case DogStates.EnterIdle:
                currentState = DogStates.Idle;
                break;
            case DogStates.Idle:
                Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
                GameObject closestPlayer = null;
                float closestDistance = Mathf.Infinity;

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Player"))
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
                dogController.Move(transform.forward * currentSpeed * speed * Time.deltaTime);

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
                                if (stateTimer > attackFrequency && angleToTarget < minimumAngleToAttack)
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
                                if (stateTimer > attackFrequency && angleToTarget < minimumAngleToAttack)
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
                                if (stateTimer > attackFrequency && angleToTarget < minimumAngleToAttack)
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

                        currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                        transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime, 0);

                        float rotationOffset = 90f * currentRotate;
                        Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                        animator.SetFloat("Move", Mathf.Clamp01(currentSpeed + idleSpeed));
                        dogController.Move(adjustedForward * currentSpeed * speed * Time.deltaTime);
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
                break;
            case DogStates.ExitBite:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = attackFrequency;
                currentState = nextState;
                Debug.Log("Exiting Bite State");
                break;
            case DogStates.EnterClaw:
                agent.enabled = false;

                animator.SetTrigger("Claw");
                attackFinished = false;
                attackReady = false;
                currentState = DogStates.Claw;
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
                break;
            case DogStates.ExitClaw:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = attackFrequency;
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
                        wind.Play();
                    }
                }
                else
                {
                    if (attackFinished)
                    {
                        nextState = DogStates.EnterChase;
                        currentState = DogStates.ExitDash;
                        return;
                    }

                    if (dashing)
                    {
                        currentSpeed += Time.deltaTime * 5;
                        currentSpeed = Mathf.Clamp01(currentSpeed);
                        animator.SetFloat("Move", Mathf.Clamp01(currentSpeed));

                        float angle = Vector3.SignedAngle(transform.forward, dashDirection, Vector3.up);
                        float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);
                        currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);

                        transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime, 0);

                        float rotationOffset = 90f * currentRotate;
                        Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                        dogController.Move(adjustedForward * currentSpeed * speed * 2.5f * Time.deltaTime);

                        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, dogLookaheadDistance, LayerMask.GetMask("Wall")))
                        {
                            animator.SetTrigger("Wince");
                            dashing = false;
                        }
                    }
                }

                break;
            case DogStates.ExitDash:
                agent.enabled = true;
                agent.Warp(transform.position);

                attackCooldown = attackFrequency;
                currentState = nextState;
                wind.Stop();
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
                        dashDirection = (target.transform.position - transform.position).normalized;
                        rotateMultiplier = 2f;
                        wind.Play();
                    }
                }
                else
                {
                    if (attackFinished)
                    {
                        nextState = DogStates.EnterChase;
                        currentState = DogStates.ExitPingPongShit;
                        return;
                    }

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

                                currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);
                                transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime * 2, 0);

                                float rotationOffset = 90f * currentRotate;
                                Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                                dogController.Move(adjustedForward * currentSpeed * speed * 3 * Time.deltaTime);
                                agent.nextPosition = transform.position;

                                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                                animator.SetBool("Right", currentRotate > 0);
                                animator.SetBool("Left", currentRotate < 0);
                            }
                            else
                            {
                                int randomAttack = Random.Range(0, 1);
                                if (randomAttack == 0)
                                {
                                    nextState = DogStates.EnterBite;
                                    currentState = DogStates.ExitPingPongShit;
                                    return;
                                }
                                else
                                {
                                    nextState = DogStates.EnterClaw;
                                    currentState = DogStates.ExitPingPongShit;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            currentSpeed += Time.deltaTime * 5;
                            currentSpeed = Mathf.Clamp01(currentSpeed);
                            animator.SetFloat("Move", Mathf.Clamp01(currentSpeed));

                            float angle = Vector3.SignedAngle(transform.forward, dashDirection, Vector3.up);
                            float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);
                            currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);

                            transform.Rotate(0, currentRotate * baseRotateMultiplier * rotateMultiplier * 90 * Time.deltaTime * 1.5f, 0);

                            float rotationOffset = 90f * currentRotate;
                            Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

                            dogController.Move(adjustedForward * currentSpeed * speed * 3 * Time.deltaTime);
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
                attackCooldown = attackFrequency;
                wind.Stop();
                Debug.Log("Exiting Ping Pong");
                break;
            default:
                break;
        }
    }

    public void SetAttackFinished(int finished)
    {
        attackFinished = finished == 1;
    }

    public void SetAttackReady(int ready)
    {
        attackReady = ready == 1;
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
    }

    public void ResetDog()
    {
        agent.enabled = true;
        agent.Warp(transform.position);
        dogController.enabled = true;
        currentState = DogStates.EnterIdle;
        target = null;
        for (int i = 0; i < attackTimes.Length; i++)
        {
            attackTimes[i] = 0;
        }
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
    }
}

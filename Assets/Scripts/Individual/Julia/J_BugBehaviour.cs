using System.Collections;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class J_BugBehaviour : Entity
{
    public enum STATE
    {
        IDLE,
        CHASE,
        ATTACK,
        DEAD
    }

    public struct FSMState
    {
        public STATE type;
        public float stateTimer;
    }


    [Header("Components")]
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private BoxCollider _jumpableBoxCollider;
    [SerializeField] private PlayerController _player;
    private bool _onNavMeshLink = false;
    private Material _dissolveMat;

    [Header("State Times")]
    [SerializeField] private float _idleDuration;

    [Header("Attack Settings")]
    [SerializeField] private Vector3 _attackOffset;
    [SerializeField] private Vector3 _attackSize;
    [SerializeField] private float _minimumStompVelocity;

    [Header("Settings")]
    [SerializeField] private LayerMask _layerToCheck;
    [SerializeField] private float _damage;
    [SerializeField] private float _jumpDuration = 0.8f;
    public System.Action OnLand, OnStartJump;
    [SerializeField] private float _lifetime = 0f;
    [SerializeField] private float _durationBeforeDestroy = 0f;
    [SerializeField] private float _minimumAttackDistance = 2f;
    private STATE _state;
    private float _currentStateTimer;
    private float _currentLifeTimer;

    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _lifetimeText;

    private void OnEnable()
    {
        if (_player == null)
            _player = FindAnyObjectByType<PlayerController>();

        // timers + state
        _currentLifeTimer = _lifetime;
        _currentStateTimer = 0f;
        _state = STATE.IDLE;

        // reset nav / physics / colliders
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = false;
            _navMeshAgent.autoTraverseOffMeshLink = false;
            _navMeshAgent.ResetPath();
        }

        var cc = GetComponent<CapsuleCollider>();
        if (cc) cc.enabled = true;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        _onNavMeshLink = false;

        // reset dissolve so it’s visible again
        if (_dissolveMat != null)
            _dissolveMat.SetFloat("_Amount", 0f);

        // start fresh
        EnterState(STATE.IDLE);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        _navMeshAgent.autoTraverseOffMeshLink = false;
        _dissolveMat = GetComponentInChildren<Renderer>().material;

        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
        }

        _currentLifeTimer = _lifetime;
        EnterState(STATE.IDLE);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (_state == STATE.DEAD)
            return;

        _currentLifeTimer -= Time.deltaTime;
        _lifetimeText.text = _currentLifeTimer.ToString();

        if (_currentLifeTimer <= 0f)
        {
            EnterState(STATE.DEAD);
            return;
        }


        UpdateState();
    }

    public override void TakeDamage(float damage, float invincibilityLength)
    {
        return;
    }

    public void SetDestination(Vector3 destination)
    {
        if (_onNavMeshLink)
            return;

        _navMeshAgent.destination = destination;
    }

    private void EnterState(STATE nextState)
    {
        //Debug.Log("entering state");

        switch (nextState)
        {
            case STATE.IDLE:

                _currentStateTimer = _idleDuration;
                //Debug.Log("idle set duration to: " + _currentStateTimer);

                break;
            case STATE.CHASE:

                // Allow chase
                if (_navMeshAgent.isStopped)
                {
                    _navMeshAgent.isStopped = false;
                }

                break;
            case STATE.ATTACK:

                //Debug.Log("Bug attacked!");

                // Stop chasing, call take damage on collider
                _navMeshAgent.isStopped = true;

                // Check if the player is within box collider
                Collider[] hits = Physics.OverlapBox(transform.position + transform.TransformDirection(_attackOffset), _attackSize, Quaternion.identity, _layerToCheck);

                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].gameObject.CompareTag("PlayerTag"))
                    {
                        Entity player = GameObject.FindWithTag("PlayerTag").GetComponent<Entity>();
                        player.TakeDamage(_damage, 0.0f);
                        //Debug.Log(player.name + " took damage!");
                        break;
                    }
                }
                

                // TODO: PLAY PINCING AUDIO

                break;
            case STATE.DEAD:
                // TODO: PLAY CRUNCHING SOUND

                // Switch off RB and colliders
                Rigidbody rb = GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                _navMeshAgent.enabled = false;

                // Start coroutine before being destroyed
                StartCoroutine(DelayBeforeDisableCollider());
                StartCoroutine(DelayBeforeDestroy());

                break;
        }

        _state = nextState;
        _stateText.text = _state.ToString();
        //Debug.Log("State: " + _state.ToString());
    }

    private void UpdateState()
    {
        _currentStateTimer -= Time.deltaTime;

        switch (_state)
        {
            case STATE.IDLE:

                // Change to chasing, chase the player
                if (_currentStateTimer <= 0f)
                {
                    ExitState();
                    EnterState(STATE.CHASE);
                }

                break;
            case STATE.CHASE:

                // Chase the player
                _navMeshAgent.SetDestination(_player.transform.position);

                if (_navMeshAgent.isOnOffMeshLink && _onNavMeshLink == false)
                {
                    //Debug.Log("starting nav mesh link movement");
                    StartNavMeshLinkMovement();
                }

                if (_onNavMeshLink)
                    FaceTarget(_navMeshAgent.currentOffMeshLinkData.endPos);

                // Attack player when close enough
                if ((_player.transform.position - transform.position).magnitude <= _minimumAttackDistance)
                {
                    ExitState();
                    EnterState(STATE.ATTACK);
                }

                break;
            case STATE.ATTACK:

                // Change state
                ExitState();
                EnterState(STATE.IDLE);

                break;
            case STATE.DEAD:
                break;
        }
    }

    private void ExitState()
    {
        switch (_state)
        {
            case STATE.IDLE:
                break;
            case STATE.CHASE:
                break;
            case STATE.ATTACK:
                break;
            case STATE.DEAD:
                break;
        }
    }
    
    private IEnumerator DelayBeforeDisableCollider()
    {
        yield return new WaitForSeconds(0.5f);
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        cc.enabled = false;
    }

    private IEnumerator DelayBeforeDestroy()
    {
        float timer = 0f;

        while (timer <= _durationBeforeDestroy)
        {
            // Shader value here
            float newAmount = Mathf.Lerp(0f, 4f, timer / _durationBeforeDestroy);
            _dissolveMat.SetFloat("_Amount", newAmount);

            timer += Time.deltaTime;

            yield return null;
        }

        // Release
        J_SpawnManager.Instance.Release("Bug", gameObject);
        J_SpawnManager.Instance.SpawnAtPosition("ThrowableBug", transform.position);
    }

    // Check for player jump
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerTag")) 
            return;

        CapsuleCollider playerCapsule = other.GetComponent<CapsuleCollider>();

        if (playerCapsule == null)
        {
            //Debug.Log("Player doesn't have a capsule collider");
            return;
        }

        Vector3 worldCenter = _jumpableBoxCollider.transform.TransformPoint(_jumpableBoxCollider.center);
        Vector3 playerCenter = playerCapsule.transform.TransformPoint(playerCapsule.center);

        float topOfBug = worldCenter.y + (_jumpableBoxCollider.size.y / 2f);
        float bottomOfCapsule = playerCenter.y - (playerCapsule.height / 2f);


        // Check position
        if (bottomOfCapsule >= topOfBug)
        {
            // Check downward velocity
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                float downwardVelocity = -playerRb.linearVelocity.y; // negative y = moving down

                if (downwardVelocity >= _minimumStompVelocity) // player is actually falling
                {
                    EnterState(STATE.DEAD);
                }
            }
        }
    }

    private void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void StartNavMeshLinkMovement()
    {
        _onNavMeshLink = true;
        NavMeshLink link = (NavMeshLink)_navMeshAgent.navMeshOwner;
        J_Spline spline = link.GetComponentInChildren<J_Spline>();

        PerformJump(link, spline);
    }

    private void PerformJump(NavMeshLink link, J_Spline spline)
    {
        bool reverseDirection = CheckIfJumpingFromEndToStart(link);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        StartCoroutine(MoveOnOffMeshLink(spline, reverseDirection));

        OnStartJump?.Invoke();
    }

    private bool CheckIfJumpingFromEndToStart(NavMeshLink link)
    {
        Vector3 startWorldPos = link.gameObject.transform.TransformPoint(link.startPoint);
        Vector3 endPosWorld = link.gameObject.transform.TransformPoint(link.endPoint);

        float distancePlayerToStart = Vector3.Distance(_navMeshAgent.transform.position, startWorldPos);
        float distancePlayerToEnd = Vector3.Distance(_navMeshAgent.transform.position, endPosWorld);

        return distancePlayerToStart > distancePlayerToEnd;
    }

    private IEnumerator MoveOnOffMeshLink(J_Spline spline, bool reverseDirection)
    {
        float currentTime = 0f;
        Vector3 agentStartPosition = _navMeshAgent.transform.position;

        while (currentTime < _jumpDuration)
        {
            currentTime += Time.deltaTime;

            float amount = Mathf.Clamp01(currentTime / _jumpDuration);
            amount = reverseDirection ? 1 - amount : amount;

            _navMeshAgent.transform.position = reverseDirection ? spline.CalculatePositionCustomEnd(amount, agentStartPosition) : spline.CalculatePositionCustomStart(amount, agentStartPosition);

            yield return new WaitForEndOfFrame();
        }

        _navMeshAgent.CompleteOffMeshLink();
        OnLand?.Invoke();
        yield return new WaitForSeconds(0.1f);
        _onNavMeshLink = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }


    private void OnDrawGizmos()
    {
        if (_player != null)
        {
            float magnitude = (_jumpableBoxCollider.transform.position - _player.transform.position).magnitude;
            float newMagnitude = (new Vector2(_jumpableBoxCollider.transform.position.x, _jumpableBoxCollider.transform.position.z) - new Vector2(_player.transform.position.x, _player.transform.position.z)).magnitude;

            Vector3 worldCenter = _jumpableBoxCollider.transform.TransformPoint(_jumpableBoxCollider.center);

            if (Mathf.Abs(worldCenter.x - _player.transform.position.x) <= (_jumpableBoxCollider.size.x / 2) && Mathf.Abs(worldCenter.z - _player.transform.position.z) <= (_jumpableBoxCollider.size.z / 2))
            {
                Gizmos.color = Color.aliceBlue;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawLine(_jumpableBoxCollider.transform.position, _player.transform.position);


            // Attack distance
            if ((transform.position - _player.transform.position).magnitude <= _minimumAttackDistance)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.black;
            }

            Gizmos.DrawLine(_player.transform.position, transform.position);
        }

        // Draw collideable box
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + transform.TransformDirection(_attackOffset), _attackSize);
    }
}

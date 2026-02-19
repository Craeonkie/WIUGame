using System.Collections;
using TMPro;
using UnityEngine;

public class J_BugBehaviour : Entity
{
    public enum STATE
    {
        IDLE,
        CHASE,
        ATTACK,
        DEAD
    }


    [SerializeField] private float _lifetime = 0f;
    [SerializeField] private float _durationBeforeDestroy = 0f;
    [SerializeField] private float _minimumAttackDistance = 2f;
    private STATE _state;
    private float _currentStateTimer;
    private float _currentLifeTimer;

    private Vector3 _currentPlayerPosition;

    [Header("Debug")]
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _remainingLifetime;

    private void OnEnable()
    {
        J_PlayerController.OnMove += UpdatePlayerPos;
    }

    private void OnDisable()
    {
        J_PlayerController.OnMove -= UpdatePlayerPos;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        _currentLifeTimer -= Time.deltaTime;

        if (_currentLifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
        UpdateState();
    }

    public override void TakeDamage(float damageTaken)
    {
        if (isInvincible)
            return;

        _currentHP -= damageTaken;
        float healthPercent = _currentHP / _maxHP;
    }

    private void EnterState(STATE nextState)
    {
        switch (nextState)
        {
            case STATE.IDLE:
                break;
            case STATE.CHASE:
                break;
            case STATE.ATTACK:
                break;
            case STATE.DEAD:

                // Start coroutine before being destroyed
                StartCoroutine(DelayBeforeDestroy());

                break;
        }

        _currentStateTimer = Time.deltaTime;
        _state = nextState;
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
                // Attack player when close enough
                if ((_currentPlayerPosition - transform.position).magnitude <= _minimumAttackDistance)
                {
                    ExitState();
                    EnterState(STATE.ATTACK);
                }

                break;
            case STATE.ATTACK:

                // Check the timer
                if (_currentStateTimer <= 0f)
                {
                    // Change state
                    ExitState();
                    EnterState(STATE.IDLE);
                }

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

    private void UpdatePlayerPos(Vector3 newPos) => _currentPlayerPosition = newPos;

    
    private IEnumerator DelayBeforeDestroy()
    {
        float timer = 0f;

        while (timer <= _durationBeforeDestroy)
        {
            // Shader value here
            Material mat = GetComponent<Renderer>().material;

            float newAmount = Mathf.Lerp(0f, 4f, timer / _durationBeforeDestroy);
            mat.SetFloat("_Amount", newAmount);

            timer += Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }
}

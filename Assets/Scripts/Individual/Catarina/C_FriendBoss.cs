using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class C_FriendBoss : Entity
{
    [Header("Phase 1")]
    [Tooltip("like when the boss reach this per it will trigger it")]
    [SerializeField] private float Phase1HealthTrigger;
    public UnityEvent _TransitionPhase1Event;
    int CurrentPhase = 0;

    [Header("Dead")]
    public UnityEvent deadEvent;
    private bool _DeadEventTriggered = false;

    [Header("Getting Atk")]
    public UnityEvent gettingAtkEvent;
    public override void Die()
    {
        if (_DeadEventTriggered) return;
        deadEvent.Invoke();
        _DeadEventTriggered = true;
    }

    public override void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (!isInvincible && !isDodging)
        {
            gettingAtkEvent.Invoke();
            _currentHP -= damageTaken;
            _invincibilityCooldown += invincibilityLength;

            if (_currentHP <= 0)
            {
                Die();
            }
            else
            {
                if (_invincibilityCooldown > 0)
                {
                    isInvincible = true;
                }
            }
        }
    }

    public override void TakeDamage(float damageTaken)
    {
        if (!isDodging)
        {
            gettingAtkEvent.Invoke();

            _currentHP -= damageTaken;
            if (_currentHP <= 0)
            {
                Die();
            }
        }
    }

    public void CheckPhase()
    {
        if (CurrentPhase == 0)
        {
            var currentPer = _currentHP / _maxHP * 100;
            if (currentPer < Phase1HealthTrigger)
            {
                CurrentPhase = 1;
                _TransitionPhase1Event.Invoke();
            }

        }
    }

    protected override void Update()
    {
        base.Update();

        CheckPhase();

        if (Input.GetMouseButtonDown(0))
        {
            TakeDamage(0);
        }
    }

    protected override void Start()
    {
        base.Start();
        spawnPoint = gameObject.transform.position;
    }
}

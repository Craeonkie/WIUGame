using System.Collections;
using System.Net;
using UnityEngine;

public class C_FriendBoss : Entity
{
    [Header("Ref")]
    [SerializeField] private Animator _Animator;
    [SerializeField] private string _IdleName;
    [SerializeField] private GameObject _RedCupParent;
    [SerializeField] private Transform _PlayerRef;
    Inventory _AIInventory;

    [Header("Hit effect")]
    [SerializeField] private GameObject[] _hitShock;
    [SerializeField] private GameObject _dizzy;
    private Coroutine _HitCoroutine;


    [Header("Phase 1")]
    [SerializeField] private float _RotateSpeed = 5f;
    [SerializeField] private Transform _Phase1SpawnPos;
    [SerializeField] private float Phase1HealthTrigger;

    public static event System.Action TransitionPhase1Action;
    int CurrentPhase = 0;
    Rigidbody _Rigidbody;

    [Header("Dead")]
    private bool _DeadEventTriggered = false;
    public static event System.Action deadAction;

    public static event System.Action gettingAtkAction;

    public void OnEnable()
    {
        C_FriendAI.onPickUPAction += pickupWeapon;
        
    }
    public void OnDisable()
    {
        C_FriendAI.onPickUPAction -= pickupWeapon;
    }
    private void pickupWeapon(GameObject pickup)
    {
        _AIInventory.PutItemInPrimary(pickup, this);
    }
    public override void Die()
    {
        if (_DeadEventTriggered) return;
        deadAction?.Invoke();
        _DeadEventTriggered = true;
    }

    public override void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (!isInvincible && !isDodging)
        {
            gettingAtkAction?.Invoke();
            if (_HitCoroutine != null)
            {
                StopCoroutine(_HitCoroutine);
            }
            _HitCoroutine = StartCoroutine(HitEffect());
            _currentHP -= damageTaken;
            _invincibilityCooldown += invincibilityLength;

            if (_currentHP <= 0)
            {
                CheckPhase();
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

    private IEnumerator HitEffect()
    {
        foreach (GameObject hs in _hitShock)
        {
            hs.SetActive(true);

            ParticleSystem[] shockSystems = hs.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in shockSystems)
            {
                if (ps == null) continue;

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
        }
        _dizzy.SetActive(true);

        ParticleSystem[] dizzySystems = _dizzy.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in dizzySystems)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.enabled = true;

            if (!ps.isPlaying) ps.Play(true);
        }

        yield return new WaitForSeconds(2.5f);

        foreach (GameObject hs in _hitShock)
        {
            hs.SetActive(false);
        }
        foreach (ParticleSystem ps in dizzySystems)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.enabled = false;
        }
    }

    public void CheckPhase()
    {
        if (CurrentPhase == 0)
        {
            var currentPer = _currentHP / _maxHP * 100;
            OnHealthChanged?.Invoke(_currentHP, _maxHP);

            if (currentPer < Phase1HealthTrigger)
            {
                CurrentPhase = 1;
                if (TransitionPhase1Action != null)
                {
                    TransitionPhase1Action?.Invoke();
                    //Teleport();
                }
            }

        }
    }

    protected override void Update()
    {
        base.Update();


        //testing remove this
        if (CurrentPhase <= 0)
        {
            CheckPhase();
            //if (Input.GetKeyDown(KeyCode.F))
            //{
            //    TakeDamage(99, 0.0f);
            //}
        }
        else
        {
            RotateToPlayer();
        }
    }

    void RotateToPlayer()
    {
        if (_PlayerRef == null) return;

        Vector3 direction = _PlayerRef.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _RotateSpeed * Time.deltaTime);
    }


    protected override void Start()
    {
        base.Start();
        spawnPoint = gameObject.transform.position;
        if (_AIInventory == null)
        {
            _AIInventory = GetComponent<Inventory>();
            if (_AIInventory == null)
            {
                Debug.LogWarning("Missing Aeon inventory script in ai!");
            }
        }

    }

    public void Teleport()
    {
        if (_Phase1SpawnPos != null)
            transform.position = _Phase1SpawnPos.position;
        _Rigidbody = GetComponent<Rigidbody>();

        transform.Rotate(0f, 180f, 0f);


        if (_Rigidbody == null)
        {
            gameObject.AddComponent<Rigidbody>();
            _Rigidbody = GetComponent<Rigidbody>();
            _Animator.CrossFade(_IdleName, 0.25f);
            _Animator.SetBool("isRunning", false);
            _Animator.SetBool("_isDefening", false);
        }
        for (int i = 0; i < _RedCupParent.transform.childCount; i++)
        {
            var cup= _RedCupParent.transform.GetChild(i);
            var _rb = cup.GetComponent<Rigidbody>();
            if (_rb == null) continue;
            _rb.isKinematic = true;
        }
    }
}

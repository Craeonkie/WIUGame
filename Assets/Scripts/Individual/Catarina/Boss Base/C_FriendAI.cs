using UnityEngine;
using UnityEngine.AI;

public class C_FriendAI : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private NavMeshAgent _Agent;
    [SerializeField] private Transform _PlayerTransform;
    [SerializeField] private Animator _Animator;

    [Header("Layers")]
    [SerializeField] private LayerMask _GroundLayer;
    [SerializeField] private string _PlayerTagName;
    [SerializeField] private LayerMask _PickUpLayer;

    [Header("Patrol")]
    [SerializeField] private float _PatrolRad = 10f;
    [SerializeField] private string _RunAnimBoolName;
    [SerializeField] private string _runAnimName;
    
    private Vector3 _CurrentPatrolPt;
    private bool _HasPt;

    [Header("Atk")]
    [SerializeField] private float _AtkCD = 1f;
    [SerializeField] private float _ATkRange = 10f;
    [SerializeField] private string _AtkAnimTriggerName;

    public static event System.Action onAtkAction;

    private bool _IsOntAtkCD;
    private float _AtkCounter = 0f;

    [Header("Detection Range")]
    [SerializeField] private float _VisionRange = 20f;
    bool _IsPlayerVisable;
    bool _IsPlayerInRange;

    [Header("Idle")]
    [SerializeField] float _IdleTime = 5f;
    float _IdleTimer;
    bool _IsIdling = false;

    [Header("PickUp")]
    [SerializeField] float _PickUpRange = 2f;
    bool _HaveWeapon = false;
    bool _FindingWeapon = false;
    bool _WeaponIsWithinDist = false;
    bool _wasfightingPlayer = false;
    float _findWeaponCooldown;
    public static event System.Action<GameObject> onPickUPAction;

    [Header("Defend")]
    [SerializeField] string _HurtAnimName;
    [SerializeField] string _DefendAnimBoolName;
    [SerializeField] string _DefendAnimName;
    [SerializeField] string _HurtTriggerName;
    [SerializeField] float _DefendTime = 3f;
    [SerializeField] float _SafeRad = 3f;
    private bool _playerInZone = false;
    public bool _isDefending { get; set; }
    float _DefendCounter = 0f;

    private void OnEnable() { 
    
        C_FriendBoss.gettingAtkAction += GettingAtk;

        C_FriendBoss.TransitionPhase1Action += Disable;
    }
    private void OnDisable()
    {

        C_FriendBoss.gettingAtkAction -= GettingAtk;

        C_FriendBoss.TransitionPhase1Action -= Disable;

    }

    public void Disable()
    {
        this.enabled = false;
    }
    private void Awake()
    {
        if (_PlayerTransform == null)
        {
            Debug.LogWarning("Player transform is null!!!");
        }
        if (_Agent == null)
        {
            _Agent = GetComponent<NavMeshAgent>();
            if (_Agent == null)
            {
                Debug.LogWarning("Missing nav mesh agent in the FRIEND!!!! ");
            }
        }
        if (_Animator == null) {
            _Animator = GetComponent<Animator>();
            if (_Animator == null)
            {
                Debug.LogWarning("Missing Animator in the FRIEND????");
            }
        }

        C_VHSTransition.FinishTransiition += EnableThis;

        _HasPt = false; 
    }

    private void OnDestroy()
    {
        C_VHSTransition.FinishTransiition -= EnableThis;
    }

    private void Update()
    {
        DetectPlayer();
        FSM();
    }

    private void EnableThis()
    {
        this.enabled = true;
    }

    // need to pick up obj
    //use it n atk
    //use aeon code
    //defend
    // for this just deetect if its getting hit if yes then start defending 

    //make sure if there no weapon n player is within range find a weapon immediately by 
    //doing the switching of state logic
    private void FSM()
    {
        if (_isDefending)
        {
            PerformDefend();
        }
        else if ((_wasfightingPlayer && !_HaveWeapon) || (((!_IsPlayerVisable && !_IsPlayerInRange) || _FindingWeapon) && !_wasfightingPlayer))
        {
            PerformaPatrol();
        }
        else if ((_IsPlayerVisable && !_IsPlayerInRange) || (_wasfightingPlayer && !_IsPlayerInRange))
        {
            if (_HaveWeapon)
            {
                PerformChase();
            }
            else
            {
                _wasfightingPlayer = true;
                FindWeapon();
            }
        }
        else if (_IsPlayerVisable && _IsPlayerInRange)
        {
            if (_HaveWeapon)
            {
                PerformAtk();
            }
            else
            {
                _wasfightingPlayer = true;
                FindWeapon();

            }
        }


        //the atk cd logic
        if (_IsOntAtkCD)
        {
            _AtkCounter -= Time.deltaTime;
            if (_AtkCounter <= 0)
            {
                _IsOntAtkCD = false;
            }
        }

    }


    //the attacking logic
    private void PerformAtk()
    {
        _wasfightingPlayer = true;
        if (_PlayerTransform != null)
        {
            transform.LookAt(_PlayerTransform);
        }
        if (!_IsOntAtkCD)
        {
            _Agent.SetDestination(transform.position);

            if (onAtkAction != null)
            {
                onAtkAction.Invoke();
            }

            _Animator.SetBool(_RunAnimBoolName,false);
            //do the atk logic here
            //like play animation type shit
            _IsOntAtkCD = true;
            _AtkCounter = _AtkCD;
        }
    }

    //the chasing logic
    private void PerformChase()
    {
        if (!_Animator.GetBool(_RunAnimBoolName))
        {
            _Animator.SetBool(_RunAnimBoolName, true);
        }
        var state = _Animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName(_runAnimName))
        {
            _Agent.SetDestination(transform.position);
            return;
        }
        if (_PlayerTransform != null)
        {
            _Agent.SetDestination(_PlayerTransform.position);
        }

    }

    //the patrolling logic
    private void PerformaPatrol()
    {
        if (!_HaveWeapon && DetectedWeapon())
        {
            PerformPickUp();
            if (_HaveWeapon)
            {
                if (_wasfightingPlayer)
                {
                    _Agent.SetDestination(_PlayerTransform.position);
                }
                return;
            }
        }

        if (_IsIdling)
        {

            _Agent.SetDestination(transform.position);

            _IdleTimer -= Time.deltaTime;

            if (_IdleTimer <= 0)
            {
                _IsIdling = false;
            }
            return;

        }
        if (!_HasPt)
        {
            FindPatrolPt();
        }
        if (_HasPt)
        {
            _Agent.SetDestination(_CurrentPatrolPt);
            _Animator.SetBool(_RunAnimBoolName, true);
        }
        if ( Vector3.Distance(transform.position,_CurrentPatrolPt)<2f)
        {
            _HasPt = false;
            _IsIdling = true;
            _IdleTimer = _IdleTime;
            _Animator.SetBool(_RunAnimBoolName, false);
        }   
    }

    //the picking up of weapon logic
    private void PerformPickUp()
    {
        Collider[] hit = Physics.OverlapSphere(transform.position, _PickUpRange, _PickUpLayer);

        if (hit.Length <= 0) return;
        _Agent.SetDestination(transform.position);
        GameObject pickUp = null;

        if (hit.Length > 1) {
            //find the closest one
            var shortestDist = float.MaxValue;
            
            for (int i = 0; i < hit.Length; i++)
            {
                var dist = Vector3.Distance(transform.position, hit[i].gameObject.transform.position);

                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    pickUp = hit[i].gameObject;
                }
            }
            if (shortestDist == float.MaxValue) return;
        }
        else
        {
            pickUp = hit[0].gameObject;
        }
        if (onPickUPAction != null)
        {
            onPickUPAction.Invoke(pickUp);
        }
        _HaveWeapon = true;
        _FindingWeapon =false;
    }

    //the defending state
    private void PerformDefend()
    {
        _Agent.SetDestination(transform.position);
        //stay into pos 
        //then put in defending state
        var state = _Animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName(_DefendAnimName)) return;

        _DefendCounter -=Time.deltaTime;
        if (_DefendCounter <= 0 || !_playerInZone) // stop defending if player is not even in safe zone
        {
            _DefendCounter = 0;
            _isDefending = false;
            _Animator.SetBool(_DefendAnimBoolName, false);
        }
    }


    //the finding of next point
    private void FindPatrolPt()
    {
        float z = Random.Range(-_PatrolRad, _PatrolRad);
        float x = Random.Range(-_PatrolRad, _PatrolRad);

        var potentioalPt = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentioalPt, out hit, 2f, NavMesh.AllAreas))
        {
            _CurrentPatrolPt = hit.position;
            _HasPt = true;
        }
    }

    //the finding of closest weapon
    private void FindWeapon()
    {
        var objs = C_HelperFunc.FindSpecificObjectsWithNoParent(_PickUpLayer);
        if (objs == null || objs.Count == 0) return;
        //find the nearest one 
        var shortestDist = float.MaxValue;
        Transform tar = null;
        foreach (var obj in objs)
        {
            var dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                tar = obj.transform;
            }
        }
        if (shortestDist == float.MaxValue) return;
        _Agent.SetDestination(tar.position);
        _HasPt = true;
        _CurrentPatrolPt = tar.position;
        _FindingWeapon = true;
        _IsIdling = false;

    }

    //the player detection logic
    private void DetectPlayer()
    {
        _IsPlayerVisable = false;
        _IsPlayerInRange = false;
        _playerInZone = false;

        Collider[] cols = Physics.OverlapSphere(transform.position, _VisionRange);
        if (cols == null || cols.Length <= 0) return;

        float shortestDist = float.MaxValue;
        Transform bestTar = null;

        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].CompareTag(_PlayerTagName)) continue;

            float dist = Vector3.Distance(transform.position, cols[i].transform.position);

            if (dist < shortestDist)
            {
                shortestDist = dist;
                bestTar = cols[i].transform;
            }
        }

        if (bestTar == null) return;

        _PlayerTransform = bestTar;

        _IsPlayerVisable = true;
        _IsPlayerInRange = shortestDist <= _ATkRange;
        _playerInZone = shortestDist <= _SafeRad;
    }

    //detecting of weapon
    private bool DetectedWeapon()
    {
        if (Time.time < _findWeaponCooldown) return Physics.CheckSphere(transform.position, _PickUpRange, _PickUpLayer);
        _findWeaponCooldown = Time.time + 0.5f;

        _WeaponIsWithinDist = Physics.CheckSphere(transform.position, _VisionRange, _PickUpLayer);
        if (!_HaveWeapon && _WeaponIsWithinDist)
        {
            FindWeapon();
        }

        return Physics.CheckSphere(transform.position, _PickUpRange, _PickUpLayer);
    }

    //debuggingggggg
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _ATkRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _VisionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _PickUpRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _SafeRad);
    }

    //the public stuff

    //call this when player is getting atk
    public void GettingAtk()
    {
        _isDefending = true;
        _DefendCounter = _DefendTime;
        _Animator.SetBool(_DefendAnimBoolName, true);
        _Animator.SetTrigger(_HurtTriggerName);
        _Animator.CrossFade(_HurtAnimName, 0.15f);

    }
}

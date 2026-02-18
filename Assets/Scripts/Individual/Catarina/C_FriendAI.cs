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
    [SerializeField] private LayerMask _PlayerLayer;

    [Header("Patrol")]
    [SerializeField] private float _PatrolRad = 10f;
    [SerializeField] private string _RunAnimBoolName;
    private Vector3 _CurrentPatrolPt;
    private bool _HasPt;

    [Header("Atk")]
    [SerializeField] private float _AtkCD = 1f;
    [SerializeField] private float _ATkRange = 10f;
    [SerializeField] private string _AtkAnimTriggerName;

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

    [Header("Dead")]
    [SerializeField] private string _DeadAnimatorName;
    [SerializeField] private string _DeadAnimBool;
    bool _IsDead = false;

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
        _HasPt = false; 
    }

    private void Update()
    {
        DetectPlayer();
        FSM();
    }


    //doing the switching of state logic
    private void FSM()
    {
        if (_IsDead)
        {
            BeDead();
            return;
        }
        else if (!_IsPlayerVisable && !_IsPlayerInRange)
        {
            PerformaPatrol();
        }
        else if (_IsPlayerVisable && !_IsPlayerInRange)
        {
            PerformChase();
        }
        else if (_IsPlayerVisable && _IsPlayerInRange)
        {
            PerformAtk();
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

    //the dead state logic
    private void BeDead()
    {
        var state = _Animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(_DeadAnimatorName) && state.normalizedTime >= 0.95f)
        {
            //dont know maybe some cut scene who knows
            return;
        }
    }

    //the dead trigger
    public void IsDead()
    {
        _IsDead = true;
        _Animator.SetBool(_DeadAnimBool, true);
        _Agent.SetDestination(transform.position);
    }

    //the attacking logic
    private void PerformAtk()
    {

        if (_PlayerTransform != null)
        {
            transform.LookAt(_PlayerTransform);
        }
        if (!_IsOntAtkCD)
        {
            _Agent.SetDestination(transform.position);
            _Animator.SetTrigger(_AtkAnimTriggerName);
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
        if (_PlayerTransform != null)
        {
            _Agent.SetDestination(_PlayerTransform.position);
        }
    }

    //the patrolling logic
    private void PerformaPatrol()
    {
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


    //the player detection logic
    private void DetectPlayer()
    {
        _IsPlayerVisable = Physics.CheckSphere(transform.position, _VisionRange, _PlayerLayer);

        _IsPlayerInRange = Physics.CheckSphere(transform.position,_ATkRange, _PlayerLayer);
    }


    //debuggingggggg
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _ATkRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _VisionRange);
    }
}

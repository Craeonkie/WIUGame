using UnityEngine;

public class C_Boid : MonoBehaviour
{
    //now hardcode get ref to the player transform
    [Header("Settings")]
    [SerializeField] private float _MaxSpeed;
    [SerializeField] private float _SteeringTime;
    [SerializeField][Range(0, 1)] private float _SeekWeight;
    [SerializeField][Range(0, 1)] private float _ObsAvoidWeight;
    [SerializeField] private float _LookAheadDist;
    [SerializeField] private float _AvoidDist;
    [SerializeField] private LayerMask _ObsLayers;
    [SerializeField] private string PlayerTagName;
    [SerializeField] private float _CastRadius = 0.5f; // much smaller

    private float autoResetCountdown = 5f;

    private float autoResetTimer = 0f;

    private float _oriObsAvoidWeight;
    private float _oriSteeringTime;

    private Rigidbody _Rigidbody;

    private float _Multiplier = 1;

    private Vector3 _Vel;
    private bool _HaveTarget = false;
    private Transform _Target;

    public static event System.Action<bool> hitSmtAction;
    private bool _DiveMode = false;
    private void Awake()
    {
        _oriObsAvoidWeight = _ObsAvoidWeight;
        _oriSteeringTime = _SteeringTime;
    }
    private void OnEnable()
    {
        _DiveMode = false;
        C_Airplane.FindTarget += SetTar;

        C_Airplane.FollowThrough += FollowThrough;
    }

    private void OnDisable()
    {
        C_Airplane.FindTarget -= SetTar;
        C_Airplane.FollowThrough -= FollowThrough;
        _DiveMode = false;
        Reset();
    }

    private void FollowThrough(float speedmultiplier)
    {
        _MaxSpeed *= speedmultiplier;
        _SteeringTime /= speedmultiplier;
        _Multiplier = speedmultiplier;
        _ObsAvoidWeight = 0;
        _HaveTarget = false;
        _DiveMode = true;
    }

    private void Reset()
    {
        if (!_DiveMode) return;
        _MaxSpeed /= _Multiplier;
        _SteeringTime = _oriSteeringTime;
        _Multiplier = 1;
        _ObsAvoidWeight = _oriObsAvoidWeight;
        _HaveTarget = false;
        _DiveMode = false;
        _Vel = Vector3.zero;
    }

    private void SetTar(Transform pos)
    {
        _Target = pos;
        _HaveTarget = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Rigidbody = GetComponent<Rigidbody>();
        if (_Rigidbody == null)
        {
            Debug.LogWarning("There no rigid body");
        }
        else
        {
            _Rigidbody.useGravity = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_DiveMode)
        {
            _Rigidbody.linearVelocity = Vector3.down * _MaxSpeed * _Multiplier;
            autoResetTimer += Time.fixedDeltaTime;
            if (autoResetTimer > autoResetCountdown)
            {
                HitSmt();
                hitSmtAction.Invoke(false);
                autoResetTimer = 0;
            }
            return;
        }

        if (!_HaveTarget) return;

        Vector3 steeringVector = Seek() + ObsAvoid();
        steeringVector = Vector3.ClampMagnitude(steeringVector, _MaxSpeed);
        var expectedEndPos = transform.position + Vector3.ClampMagnitude(_Rigidbody.linearVelocity + steeringVector, _MaxSpeed);
        Vector3.SmoothDamp(transform.position, expectedEndPos, ref _Vel, _SteeringTime);
        _Rigidbody.linearVelocity = _Vel * _Multiplier;
        if (_Rigidbody.linearVelocity != Vector3.zero)
        {
            transform.LookAt(transform.position + _Rigidbody.linearVelocity);
        }
    }

    private Vector3 ObsAvoid()
    {
        Vector3 steeringVector = Vector3.zero;
        //false = nth detected
        //make it less hard codded

        if (Physics.SphereCast(transform.position, _CastRadius, _Rigidbody.linearVelocity.normalized, out RaycastHit hitInfo, _LookAheadDist, _ObsLayers))
        {
            var avoidDirection = (transform.position - hitInfo.point).normalized;
            var targetPos = transform.position + avoidDirection * _AvoidDist;
            var desiredVel = Vector3.ClampMagnitude(targetPos - transform.position, _MaxSpeed);
            steeringVector = desiredVel - _Rigidbody.linearVelocity;

        }
        return steeringVector * _ObsAvoidWeight;
    }

    private Vector3 Seek()
    {
        var desiredVel = Vector3.ClampMagnitude(_Target.position - transform.position, _MaxSpeed);
        var steeringVector = desiredVel - _Rigidbody.linearVelocity;
        return steeringVector * _SeekWeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_DiveMode || other.CompareTag(PlayerTagName) || (_ObsLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            HitSmt();
            hitSmtAction.Invoke(false);
        }
    }

    private void HitSmt()
    {
        //for now just nothing cos need to do explosion or smt n sound 
        Debug.LogWarning("pls make sure to remove this if u have added particle affect and sound!!!!");
        Reset();
    }

    private void OnDrawGizmos()
    {
        // lookahead range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _LookAheadDist);

        // avoid dist
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _AvoidDist);


        Gizmos.color = Color.cyan;
        // draw sphere at start and end of cast
        Gizmos.DrawWireSphere(transform.position, _CastRadius);
    }
}


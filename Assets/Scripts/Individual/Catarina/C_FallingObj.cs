using UnityEngine;

public class C_FallingObj : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _waitTimer = 0;
    [SerializeField] private float _fallingSpeed;
    private C_PencilAbility _spawner;

    [Header("Collider")]
    [SerializeField] private LayerMask _groundLayer;

    private bool _startCountdown = false;
    [SerializeField] private float _oriCD = 1f;
    private float _collideCD = -1;

    [Header("Decal")]
    [SerializeField] private float _maxWarningDistance = 10f;
    private GameObject _warningDecalInstance;
    private Renderer _decalRenderer;
    // mpb is per renderer so each falling obj has its own independent alpha value
    private MaterialPropertyBlock _mpb;
    // cached shader property id, faster than passing string every frame
    private static readonly int ColorID = Shader.PropertyToID("_color");
    private bool _isBeingReleased = false;
    public void Init(C_PencilAbility spawner)
    {
        _spawner = spawner;
        _isBeingReleased = false;
        // reset physics
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        // reset state
        _waitTimer = Random.Range(2.5f, 10f);
        _collideCD = _oriCD;
        _startCountdown = false;

        // raycast down to find ground position for the decal
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            // get a decal from the pool instead of instantiating
            _warningDecalInstance = _spawner.GetDecal();
            _warningDecalInstance.transform.position = hit.point + Vector3.up * 0.01f;
            _warningDecalInstance.transform.rotation = Quaternion.identity;

            // renderer is cached in pencilability so no GetComponent needed here
            _decalRenderer = _spawner.GetDecalRenderer(_warningDecalInstance);

            // set starting alpha
            UpdateDecalAlpha();
        }
    }

    void Start()
    {
        _oriCD = _collideCD;

    }
    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }


    void Update()
    {
        // wait before falling
        _waitTimer -= Time.deltaTime;
        if (_waitTimer < 0 && _rb.isKinematic)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.down * _fallingSpeed;
        }

        // countdown after hitting ground before returning to pool
        if (_startCountdown)
        {
            _collideCD -= Time.deltaTime;
            if (_collideCD < 0)
            {
                PuttingBackInPool();
                if (_spawner != null)
                    _spawner.Release(this);
            }
        }

        // update decal alpha based on distance from ground
        UpdateDecalAlpha();
    }

    private void UpdateDecalAlpha()
    {
        if (_warningDecalInstance == null || _decalRenderer == null) return;

        float distance = Vector3.Distance(transform.position, _warningDecalInstance.transform.position);
        // closer to ground = more opaque
        float alpha = Mathf.Clamp01(1f - distance / _maxWarningDistance);

        // mpb keeps this renderer's values independent from all other decal renderers
        _decalRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorID, new Color(1f, 0f, 0f, alpha));
        _decalRenderer.SetPropertyBlock(_mpb);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _groundLayer) != 0)
            _startCountdown = true;
    }

    // safety net if unity disables this obj directly
    private void OnDisable()
    {
        if (_spawner == null) return;
        PuttingBackInPool();
    }

    public void PuttingBackInPool()
    {
        if (_warningDecalInstance == null || _isBeingReleased) return;
        _isBeingReleased = true;
        _spawner.ReleaseDecal(_warningDecalInstance);
        _warningDecalInstance = null;
        _decalRenderer = null;
    }
}
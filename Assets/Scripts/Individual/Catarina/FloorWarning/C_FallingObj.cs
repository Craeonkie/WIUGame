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
    private float _CollideCD = -1;


    [Header("Decal")]
    [SerializeField] private GameObject _warningDecalPrefab;
    private GameObject _warningDecalInstance;
    [SerializeField] private float _maxWarningDistance = 10f; // distance over which alpha changes
    [SerializeField] private Renderer _decalRenderer;
    Material _mat;
    public void Init(C_PencilAbility spawner)
    {
        _spawner = spawner;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _waitTimer = Random.Range(2.5f, 10f);
        _CollideCD = _oriCD;
        _startCountdown = false;

        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            // Spawn decal at hit point
            _warningDecalInstance = Instantiate(_warningDecalPrefab, hit.point + Vector3.up * 0.01f, Quaternion.identity);
            _decalRenderer = _warningDecalInstance.GetComponent<Renderer>();

            //set it as a child of it
            _warningDecalInstance.transform.parent = hit.collider.transform;
            _mat = _decalRenderer.material;
        }

        if (_warningDecalInstance != null)
        {
            float distance = Vector3.Distance(transform.position, _warningDecalInstance.transform.position);
            float alpha = Mathf.Clamp01(1f - distance / _maxWarningDistance);

            if (_decalRenderer != null)
            {
                Color color = _mat.GetColor("_color");
                color.a = alpha;
                _mat.SetColor("_color", color);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _oriCD = _CollideCD;
    }

    // Update is called once per frame
    void Update()
    {
        _waitTimer -= Time.deltaTime;
        if (_waitTimer < 0 && _rb.isKinematic)
        {
            // to allow falling
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.down * _fallingSpeed;
        }
        if (_startCountdown)
        {
            _CollideCD -= Time.deltaTime;
            if (_CollideCD < 0)
            {
                PuttingBackInPool();

                if (_spawner != null)
                    _spawner.Release(this);
            }
        }

        if (_warningDecalInstance != null)
        {
            float distance = Vector3.Distance(transform.position, _warningDecalInstance.transform.position);
            float alpha = Mathf.Clamp01(1f - distance / _maxWarningDistance);

            if (_decalRenderer != null)
            {
                Color color = _mat.GetColor("_color");
                color.a = alpha;
                _mat.SetColor("_color", color);
            }
        }
    }

    private void OnDisable()
    {
        PuttingBackInPool();

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _groundLayer) != 0)
        {
            _startCountdown = true;
        }
    }

    public void PuttingBackInPool()
    {

        Destroy(_warningDecalInstance);
        _warningDecalInstance = null;
        _decalRenderer = null;
    }
}

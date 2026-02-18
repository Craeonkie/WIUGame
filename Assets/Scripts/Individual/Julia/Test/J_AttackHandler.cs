using Unity.Cinemachine;
using UnityEngine;

public class J_AttackHandler : MonoBehaviour
{
    [SerializeField] private SphereCollider[] _colliders;
    [SerializeField] private CinemachineImpulseSource[] _sources;
    [SerializeField] private LayerMask _layer;

    public bool SlashSound { get; set; }
    private int _damage;

    public static System.Action<Transform, int> OnAttackSuccess;
    
    private void Start()
    {
        for (int i = 0; i < _colliders.Length; ++i)
            _colliders[i].enabled = false;
    }

    private void Update()
    {
        for (int i = 0; i < _colliders.Length; ++i)
        {
            if (_colliders[i].enabled)
            {

                Vector3 worldCenter = _colliders[i].transform.TransformPoint(_colliders[i].center);
                float scaleFactor = Mathf.Max(_colliders[i].transform.lossyScale.x, _colliders[i].transform.lossyScale.y, _colliders[i].transform.lossyScale.z);
                float actualWorldRadius = _colliders[i].radius * scaleFactor; 

                Collider[] hitColliders = Physics.OverlapSphere(worldCenter, actualWorldRadius, _layer);

                for (int j = 0; j < hitColliders.Length; j++)
                {
                    // Disable this collider
                    _colliders[i].enabled = false;

                    // Damage target if applicable
                    if (hitColliders[j].gameObject.TryGetComponent<J_Damageable>(out J_Damageable damageable))
                    {
                        damageable.TakeExternalDamage(new Vector2(transform.position.x, transform.position.z), _damage);

                        // Check if this was an attack from the player
                        if (transform.CompareTag("Player"))
                            OnAttackSuccess?.Invoke(damageable.gameObject.transform, _damage);

                        //if (SlashSound)
                        //{
                        //    AudioManager.Instance.PlayOneShot("slashHit1", damageable.transform.position);
                        //} 
                        //else
                        //{
                        //    AudioManager.Instance.PlayOneShot("punchImpact", damageable.transform.position);
                        //}
                    }

                    Debug.Log(hitColliders[j].gameObject.name);

                    // Generate impulse
                    _sources[i].GenerateImpulse(Camera.main.transform.forward);

                    
                }
            }
        }
    }

    public void SetDamage(int damage) => _damage = damage;

    public void EnableCollider(int index)
    {
        SphereCollider collider = _colliders[index];
        collider.enabled = true;
    }

    public void DisableCollider(int index)
    {
        _colliders[index].enabled = false;
    }

    public void DisableAllColliders()
    {
        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        SphereCollider collider = _colliders[0];
        Vector3 worldCenter = collider.transform.TransformPoint(collider.center);

        float scaleFactor = Mathf.Max(collider.transform.lossyScale.x,
                              collider.transform.lossyScale.y,
                              collider.transform.lossyScale.z);

        float actualWorldRadius = collider.radius * scaleFactor;

        Gizmos.DrawWireSphere(worldCenter, actualWorldRadius);
    }
}

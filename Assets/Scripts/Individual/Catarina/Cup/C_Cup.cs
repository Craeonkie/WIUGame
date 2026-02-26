using System.Collections;
using UnityEngine;

public class C_Cup : MonoBehaviour
{
    public static event System.Action hitSuccessful;
    [SerializeField] private LayerMask _interactableMask;

    [Header("Shake")]
    [SerializeField] private float _ShakeAmount = 0.05f;
    [SerializeField] private float _ShakeSpeed = 10f;
    [SerializeField] private float _ShakeDuration = 0.5f;

    private Vector3 _OriginalPos;
    private bool _IsShaking = false;

    void Start()
    {
        _OriginalPos = transform.position;
        StartCoroutine(SetToKinematic());
    }

    public void StartShake()
    {
        if (!_IsShaking)
            StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        _IsShaking = true;
        float elapsed = 0f;

        while (elapsed < _ShakeDuration)
        {
            float offsetX = Mathf.Sin(elapsed * _ShakeSpeed * Mathf.PI) * _ShakeAmount;
            transform.position = _OriginalPos + new Vector3(offsetX, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = _OriginalPos;
        _IsShaking = false;
    }
    
    IEnumerator SetToKinematic()
    {
        yield return new WaitForSeconds(3f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        int objLayer = collision.gameObject.layer;

        if ((_interactableMask & (1 << objLayer)) != 0)
        {
            AudioLibrary.Instance.PlaySoundAtPointCustom("PlasticCup", transform.position);

            if (collision.gameObject != null)
            {
                Destroy(collision.gameObject);
            }
            hitSuccessful?.Invoke();
            StartShake();
        }
    }
}

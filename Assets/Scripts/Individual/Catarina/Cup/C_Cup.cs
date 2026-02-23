using System.Collections;
using UnityEngine;

public class C_Cup : MonoBehaviour
{
    public static event System.Action hitSuccessful;
    [SerializeField] private LayerMask _interactableMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SetToKinematic());
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
            hitSuccessful?.Invoke();

        }
    }
}

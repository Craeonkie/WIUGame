using UnityEngine;

public class J_CheckCollider : MonoBehaviour
{
    [SerializeField] LayerMask _layer;
    public System.Action OnColliderTriggerEnter;
    public System.Action OnColliderTriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        if ((_layer.value & (1 << other.gameObject.layer)) != 0)
        {
            OnColliderTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((_layer.value & (1 << other.gameObject.layer)) != 0)
        {
            OnColliderTriggerExit?.Invoke();
        }
    }
}

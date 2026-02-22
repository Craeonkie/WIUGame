using UnityEngine;
using static C_WeaponSpawner;

public class C_CatapultChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _Layer;
    [SerializeField] private bool _isLeft;
    public static event System.Action<bool,bool> ChangeMoveAction;


    private void OnTriggerExit(Collider other)
    {
        if ((_Layer & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("came into trigger");

            ChangeMoveAction?.Invoke(false, _isLeft);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_Layer & (1 << other.gameObject.layer)) != 0)
        {
            ChangeMoveAction?.Invoke(true, _isLeft);
        }
    }   
}

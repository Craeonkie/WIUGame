using UnityEngine;

public class J_ShockwaveCheck : MonoBehaviour
{
    [SerializeField] private float _groundCheckDist;
    public static System.Action<bool> OnTouchingShockwave;
    public LayerMask _groundLayer;
    public static bool CheckForShockwave;
    public static bool TouchingShockwave;

    private void FixedUpdate()
    {
        //if (!CheckForShockwave)
            //return;

        if (Physics.Raycast(transform.position, Vector3.down, _groundCheckDist, _groundLayer))
        {
            Debug.Log("distance");
            TouchingShockwave = true;
        }
        else
        {
            Debug.Log('n');
            TouchingShockwave = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down * _groundCheckDist);
    }
}

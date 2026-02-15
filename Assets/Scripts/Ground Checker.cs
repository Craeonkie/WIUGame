using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private bool isGrounded;
    [SerializeField] LayerMask collidableMasks;
    [SerializeField] List<Collider> overlappingCollidors;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & collidableMasks) != 0)
        {
            overlappingCollidors.Add(other);
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (overlappingCollidors.Remove(other))
        {
            if (overlappingCollidors.Count == 0)
            {
                isGrounded = false;
            }
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}

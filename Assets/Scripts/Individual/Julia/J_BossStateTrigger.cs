using UnityEngine;

public class J_BossStateTrigger : Interactable
{
    [SerializeField] private float _minimumInteractDistance;
    public static System.Action <CapsuleCollider> OnShoulderTriggered;

    private void OnEnable()
    {
        PlayerController.OnInteract += Interact;
    }

    private void OnDisable()
    {
        PlayerController.OnInteract -= Interact;
    }

    private void Interact()
    {
        // Check player controller's distance to this object
        PlayerController player = FindFirstObjectByType<PlayerController>();
    
        if ((player.transform.position - transform.position).magnitude <= _minimumInteractDistance)
        {
            Debug.Log("help");
            OnShoulderTriggered?.Invoke(GetComponent<CapsuleCollider>());
        }
    }

    private void OnDrawGizmos()
    {
        // Check player controller's distance to this object
        //PlayerController player = FindFirstObjectByType<PlayerController>();
        //if ((player.transform.position - transform.position).magnitude <= _minimumInteractDistance)
        //    Gizmos.color = Color.green;
        //else
        //    Gizmos.color = Color.yellow;

        //Gizmos.DrawLine(player.transform.position, transform.position);
    }
}

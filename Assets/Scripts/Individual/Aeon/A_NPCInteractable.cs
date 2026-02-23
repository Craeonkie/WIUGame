using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : Interactable
{
    [SerializeField] private float rotationSpeed;
    private bool facingPlayer;
    private RestingPlayerController player;
    public UnityEvent interactWithNPC;

    private void Start()
    {
        player = FindFirstObjectByType<RestingPlayerController>();
    }

    public override void InteractWith()
    {
        base.InteractWith();
        interactWithNPC.Invoke();
    }

    public void ExitDialogue()
    {
        facingPlayer = false;
    }

    private void Update()
    {
        // Rotate NPC towards player
        if (facingPlayer)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), Time.deltaTime * rotationSpeed);
        }
    }
}

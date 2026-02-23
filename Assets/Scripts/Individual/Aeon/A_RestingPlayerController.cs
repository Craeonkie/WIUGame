using UnityEngine;
using UnityEngine.InputSystem;

// Simplified player controller for the resting scene
public class RestingPlayerController : Entity
{
    [Header("Input System")]
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;

    [Header("Movement")]
    [SerializeField] private float _jumpPower = 5f;
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Rigidbody myRigidbody;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private float _interactionConeAngle = 45f;

    [Header("References")]
    [SerializeField] private GroundChecker groundChecker;

    private Vector2 _inputMove;

    protected override void Start()
    {
        base.Start();

        // Setup Inputs
        if (_playerInput != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _interactAction = _playerInput.actions["Interact"];

            _moveAction.Enable();
            _jumpAction.Enable();
            _interactAction.Enable();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void Update()
    {
        base.Update();

        bool isGrounded = groundChecker != null ? groundChecker.IsGrounded() : true;
        _inputMove = _moveAction.ReadValue<Vector2>();
        bool isMoving = _inputMove != Vector2.zero;

        if (isGrounded && _jumpAction.WasPressedThisDynamicUpdate())
        {
            myRigidbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }

        if (!myRigidbody.isKinematic && cameraTransform != null)
        {
            Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            Vector3 cameraForward = cameraYawOnly * Vector3.forward;
            Vector3 cameraRight = cameraYawOnly * Vector3.right;

            Vector3 moveDirection = (cameraForward * _inputMove.y + cameraRight * _inputMove.x).normalized;

            // Rotate player body towards movement direction
            if (moveDirection != Vector3.zero && isGrounded)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }

            // Apply velocity
            float targetSpeed = isMoving ? _maxSpeed : 0f;
            Vector3 moveVelocity = moveDirection * targetSpeed;

            // Preserve vertical velocity (gravity/jumping)
            myRigidbody.linearVelocity = new Vector3(moveVelocity.x, myRigidbody.linearVelocity.y, moveVelocity.z);
        }

        if (cameraTransform == null)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(cameraTransform.position, _interactionRange, interactablesLayer);

        float closestDist = _interactionRange;
        Interactable closestInteractable = null;

        foreach (Collider col in hits)
        {
            if (!col.TryGetComponent<Interactable>(out var interactable))
            {
                continue;
            }

            Vector3 playerXZ = new(cameraTransform.position.x, 0f, cameraTransform.position.z);
            Vector3 objectXZ = new(col.transform.position.x, 0f, col.transform.position.z);
            float dist = Vector3.Distance(playerXZ, objectXZ);

            Vector3 transformForwardXZ = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;

            // Flatten direction to object to XZ plane
            Vector3 toObjectXZ = new Vector3(col.transform.position.x - cameraTransform.position.x, 0f, col.transform.position.z - cameraTransform.position.z).normalized;

            float angle = Vector3.Angle(transformForwardXZ, toObjectXZ);

            if (dist <= closestDist && angle <= _interactionConeAngle)
            {
                closestDist = dist;
                closestInteractable = interactable;
            }
        }

        // Trigger Interaction
        if (closestInteractable != null && _interactAction.WasPressedThisFrame())
        {
            print("Trying to interact");
            closestInteractable.InteractWith();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraTransform.position, _interactionRange);

        if (cameraTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * _interactionRange);
        }
    }
}
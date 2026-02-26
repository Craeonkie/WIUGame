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
    [SerializeField] private float _currentSpeed = 0f;
    [SerializeField] private Rigidbody myRigidbody;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private float _interactionConeAngle = 45f;

    [Header("References")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private MouseMovement[] mouseMovements;

    [Header("Object Highlighting")]
    [SerializeField] private GameObject _currentlyHighlightedObject;
    [SerializeField] private GameObject _interactIcon;

    private Vector2 _inputMove;
    private bool _inDialogue;

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

        Cursor.visible = false;
    }

    protected override void Update()
    {
        base.Update();

        bool isGrounded = groundChecker != null && groundChecker.IsGrounded();
        _inputMove = _moveAction.ReadValue<Vector2>();
        bool isMoving = _inputMove != Vector2.zero && !_inDialogue;

        if (isGrounded && _jumpAction.WasPressedThisDynamicUpdate() && !_inDialogue)
        {
            myRigidbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
        }

        if (!myRigidbody.isKinematic && cameraTransform != null)
        {
            Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            Vector3 cameraForward = cameraYawOnly * Vector3.forward;
            Vector3 cameraRight = cameraYawOnly * Vector3.right;

            Vector3 moveDirection = (cameraForward * _inputMove.y + cameraRight * _inputMove.x).normalized;

            // Apply velocity
            float targetSpeed = isMoving ? _maxSpeed : 0f;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _maxSpeed /*/ 0.1f*/ * Time.deltaTime);
            Vector3 moveVelocity = moveDirection * _currentSpeed;
            myRigidbody.linearVelocity = new Vector3(moveVelocity.x, myRigidbody.linearVelocity.y, moveVelocity.z);
        }

        if (cameraTransform == null)
        {
            return;
        }


        if (!_inDialogue)
        {
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
            if (closestInteractable != null && closestInteractable.gameObject.tag == "Interactable")
            {
                // Position interact icon over the item to interact with
                if (closestInteractable.gameObject != _currentlyHighlightedObject)
                {
                    _currentlyHighlightedObject = closestInteractable.gameObject;
                    _interactIcon.SetActive(true);
                }
                if (_interactAction.WasPressedThisFrame())
                {
                    closestInteractable.InteractWith();
                }
            }
            else
            {
                _interactIcon.SetActive(false);
                _currentlyHighlightedObject = null;
            }

            if (_currentlyHighlightedObject != null)
            {
                _interactIcon.transform.position = _currentlyHighlightedObject.transform.position + Vector3.up * 2;
                _interactIcon.transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up); 
            }
        }
    }

    public void ToggleInDialogue(bool inDialogue)
    {
        _inDialogue = inDialogue;
        foreach (MouseMovement mouseMovement in mouseMovements)
        {
            mouseMovement.enabled = !inDialogue;
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
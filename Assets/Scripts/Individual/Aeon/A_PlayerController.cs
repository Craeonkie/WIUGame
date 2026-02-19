using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    [Header("Input System")]
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _rollAction;
    private InputAction _interactAction;
    private InputAction _dropAction;
    private InputAction _primaryAction;
    private InputAction _secondaryAction;
    private InputAction _specialAction;
    private InputAction _equipPrimary;
    private InputAction _equipSecondary;

    [Header("Movement")]
    [SerializeField] private float _jumpPower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _rollSpeed;
    [SerializeField] private Rigidbody myRigidbody;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float playerRotationSpeed;
    [SerializeField] private float rollDuration;
    [SerializeField] private float landDuration;
    private Vector3 _rollDirection;
    private float _currentSpeed;
    private float _currentRollTimer;
    private float _currentLandTimer;
    [SerializeField] private float _currentStunDuration;
    [SerializeField] private bool _isStunned;

    [Header("Item Pickup Properties")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _pickupConeRadius;
    [SerializeField] private float _pickupRange;

    [Header("Other scripts of note")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AnimationHandler animationHandler;
    [SerializeField] protected Animator _animator;

    private Vector2 _inputMove;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _isRolling = false;
    [SerializeField] private bool _canAct = true;
    [SerializeField] private bool _wasGroundedPreviously = false;

    protected override void Start()
    {
        base.Start();
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _rollAction = _playerInput.actions["Roll"];
        _interactAction = _playerInput.actions["Interact"];
        _dropAction = _playerInput.actions["Drop"];
        _primaryAction = _playerInput.actions["Primary"];
        _secondaryAction = _playerInput.actions["Secondary"];
        _specialAction = _playerInput.actions["Special"];
        _equipPrimary = _playerInput.actions["Equip Primary"];
        _equipSecondary = _playerInput.actions["Equip Secondary"];

        // Enable actions if not auto-enabled
        _moveAction.Enable();
        _jumpAction.Enable();
        _rollAction.Enable();
        _interactAction.Enable();
        _dropAction.Enable();
        _primaryAction.Enable();
        _secondaryAction.Enable();
        _specialAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;

        _rollDirection = Vector3.forward;
    }

    protected override void Update()
    {
        base.Update();

        if (_currentStunDuration > 0)
        {
            _currentStunDuration -= Time.deltaTime;
            _isStunned = true;
            if (_currentStunDuration <= 0)
            {
                _isStunned = false;
            }
        }

        // Handle player landed duration
        if (_currentLandTimer > 0)
        {
            _currentLandTimer -= Time.deltaTime;
        }

        bool canMove = animationHandler.CanMove();
        bool isGrounded = groundChecker.IsGrounded();
        _inputMove = _moveAction.ReadValue<Vector2>();
        bool isMoving = _inputMove != Vector2.zero && canMove;

        // Only accept input and rotation if player isn't stunned
        if (!_isStunned)
        {
            // Rotation
            if (isMoving && !_isRolling && _currentLandTimer <= 0)
            {
                Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTarget.transform.eulerAngles.y, 0);
                Vector3 cameraForward = cameraYawOnly * Vector3.forward;
                Vector3 cameraRight = cameraYawOnly * Vector3.right;

                Vector3 moveDir = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

                float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
                transform.eulerAngles = new Vector3(0, newY, 0);
                _rollDirection = moveDir;
            }

            // Runs if is grounded and not jumping
            if (isGrounded && !_isJumping)
            {
                // Jumping
                if (_jumpAction.WasPressedThisDynamicUpdate() && canMove && _canAct)
                {
                    _isJumping = true;
                    _isRolling = false;
                    myRigidbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
                }
                // Rolling
                else if (_rollAction.WasPressedThisDynamicUpdate() && canMove && _canAct && !_isRolling)
                {
                    _animator.SetTrigger("IsRolling");
                    _currentRollTimer = rollDuration;
                    _isRolling = true;
                    isDodging = true;
                    _canAct = false;
                }
            }
            
            // Reenable ability to act if player is not midair and not rolling
            if (isGrounded && !_isRolling)
            {
                _canAct = true;
            }

            // Handle max velocity
            float targetSpeed = 0.0f;

            // Handle movement normally
            if (!_isRolling && _currentLandTimer <= 0)
            {
                if (isMoving && _currentLandTimer <= 0)
                {
                    targetSpeed = _maxSpeed;
                }

                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _maxSpeed / 0.1f * Time.deltaTime);
                Vector3 moveVelocity = transform.forward * _currentSpeed;
                myRigidbody.linearVelocity = new Vector3(moveVelocity.x, myRigidbody.linearVelocity.y, moveVelocity.z);
            }
            // Rolling
            else if (_isRolling)
            {
                myRigidbody.linearVelocity = new Vector3(_rollSpeed * _rollDirection.x, myRigidbody.linearVelocity.y, _rollSpeed * _rollDirection.z);
                _currentRollTimer -= Time.deltaTime;
                if (_currentRollTimer <= 0)
                {
                    _currentSpeed = _maxSpeed;
                    _isRolling = false;
                    isDodging = false;
                    _canAct = true;
                }
            }
        }

        //// Handle other inputs
        // Cast a sphere around the player (or use a raycast forward if preferred)
        Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange, interactablesLayer);

        Interactable closestInteractable = null;
        float closestDist = _pickupRange;

        foreach (Collider col in hits)
        {
            bool alreadyHolding = false;

            if (col.gameObject == inventory.ReturnCurrentPrimaryItem() || col.gameObject == inventory.ReturnCurrentSecondaryItem())
            {
                alreadyHolding = true;
            }

            if (alreadyHolding)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, col.transform.position);
            float angle = Vector3.Angle(transform.forward, col.transform.position - transform.position);
            if (dist <= closestDist && angle <= _pickupConeRadius && col.gameObject.TryGetComponent<Interactable>(out closestInteractable))
            {
                closestDist = dist;
            }
        }

        if (closestInteractable != null)
        {
            inventory.HighlightObject(closestInteractable.gameObject);
            if (_interactAction.WasPressedThisDynamicUpdate() && !_isStunned)
            {
                inventory.InteractWith(closestInteractable, animationHandler);
            }
        }

        // Dropping
        if (_dropAction.WasPressedThisDynamicUpdate() && !_isStunned)
        {
            if (inventory.ReturnCurrentItem() != null)
            {
                inventory.DropItem(inventory.ReturnCurrentItem());
            }
        }

        // Only accept input when the player is able act
        if (_canAct && !_isStunned)
        {
            // Primary
            if (_primaryAction.WasPressedThisDynamicUpdate())
            {
                animationHandler.TryingToUsePrimary(true);
            }
            if (_primaryAction.WasReleasedThisDynamicUpdate())
            {
                animationHandler.TryingToUsePrimary(false);
            }

            // Secondary
            if (_secondaryAction.WasPressedThisDynamicUpdate())
            {
                animationHandler.TryingToUseSecondary(true);
            }
            if (_secondaryAction.WasReleasedThisDynamicUpdate())
            {
                animationHandler.TryingToUseSecondary(false);
            }

            // Special
            if (_specialAction.WasPressedThisDynamicUpdate())
            {
                animationHandler.TryingToUseSpecial(true);
            }
            if (_specialAction.WasReleasedThisDynamicUpdate())
            {
                animationHandler.TryingToUseSpecial(false);
            }
        }

        // Send parameters to animator
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("IsGrounded", isGrounded);
        if (_isJumping)
        {
            _animator.SetTrigger("IsJumping");
            _isJumping = false;
        }
        _animator.SetFloat("Y Velocity", myRigidbody.linearVelocity.y);

        // Handle when the player lands
        if (!_wasGroundedPreviously && isGrounded)
        {
            _currentLandTimer = landDuration;
        }
        else if (_isRolling || !isGrounded)
        {
            _currentLandTimer = 0;
        }

        // Update this for the next frame
        _wasGroundedPreviously = isGrounded;
    }

    // Check if current animation is over
    public virtual bool CurrentAnimationOver(int state)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(state);

        if (stateInfo.normalizedTime < 1.0f)
        {
            _animationHasReset = true;
        }

        return (stateInfo.normalizedTime >= 1.0f) && _animationHasReset;
    }

    // Stun the player
    public virtual void Stun(float stunDuration)
    {
        _currentStunDuration = stunDuration;
        animationHandler.GoBackToIdle();
        _canAct = false;
    }
}
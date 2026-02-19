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

    [Header("Movement")]
    [SerializeField] private float _jumpPower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _rollSpeed;
    [SerializeField] private Rigidbody myRigidbody;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float playerRotationSpeed;
    [SerializeField] private float rollDuration;
    private Vector3 _rollDirection;
    private float _currentSpeed;
    private float _currentRollTimer;

    [Header("Item Pickup Properties")]
    [SerializeField] private LayerMask interactablesLayer;
    [SerializeField] private float _pickupConeRadius;
    [SerializeField] private float _pickupRange;

    [Header("Other scripts of note")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AnimationHandler animationHandler;

    private Vector2 _inputMove;
    private bool _isJumping = false;
    private bool _isRolling = false;
    private bool _canAct = true;

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

        bool canMove = animationHandler.CanMove();
        bool isGrounded = groundChecker.IsGrounded();

        _inputMove = _moveAction.ReadValue<Vector2>();

        // Rotation
        if (_inputMove != Vector2.zero && canMove && !_isRolling)
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
                _canAct = false;
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
        else if (isGrounded && !_isJumping && !_isRolling)
        {
            _canAct = true;
        }

        // Handle max velocity
        float targetSpeed = 0.0f;
        bool isMoving = _inputMove != Vector2.zero && canMove;

        // Handle movement normally
        if (!_isRolling)
        {
            if (isMoving)
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

        //// Handle other inputs
        // Interacting
        if (_interactAction.WasPressedThisDynamicUpdate())
        {
            // Cast a sphere around the player (or use a raycast forward if preferred)
            Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange, interactablesLayer);

            GameObject closest = null;
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
                    closest = col.gameObject;
                }
            }

            if (closest != null)
            {
                inventory.HighlightObject(closestInteractable.gameObject);
                inventory.InteractWith(closestInteractable, animationHandler);
            }
        }

        // Dropping
        if (_dropAction.WasPressedThisDynamicUpdate())
        {
            if (inventory.ReturnCurrentItem() != null)
            {
                inventory.DropItem(inventory.ReturnCurrentItem());
            }
        }

        // Only accept input when the player is able act
        if (_canAct)
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

        // Update interaction handler
        //animationHandler.ToggleAbilityToAct(true);

        // Send parameters to animator
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("IsGrounded", isGrounded);
        if (_isJumping)
        {
            _animator.SetTrigger("IsJumping");
            _isJumping = false;
        }
        _animator.SetFloat("Y Velocity", myRigidbody.linearVelocity.y);
    }

    //// Handle Inventory UI visibility
    //if (_isCanvasActive)
    //{
    //    inventoryUICanvas.SetActive(true);
    //    inventoryUICanvas.GetComponent<CanvasGroup>().alpha = Mathf.MoveTowards(inventoryUICanvas.GetComponent<CanvasGroup>().alpha, 1.0f, Time.deltaTime * 2.0f);
    //}
    //else if (inventoryUICanvas.activeSelf)
    //{
    //    inventoryUICanvas.GetComponent<CanvasGroup>().alpha = Mathf.MoveTowards(inventoryUICanvas.GetComponent<CanvasGroup>().alpha, 0.0f, Time.deltaTime * 2.0f);
    //    if (inventoryUICanvas.GetComponent<CanvasGroup>().alpha == 0.0f)
    //    {
    //        inventoryUICanvas.SetActive(false);
    //    }
    //}
    //// Do damage without invincibility cooldown
    //public override void TakeDamage(float damageTaken)
    //{
    //    if (!isInvincible)
    //    {
    //        _currentHP -= damageTaken;
    //        if (_currentHP <= 0)
    //        {
    //            isDead = true;
    //            _animationHasReset = false;
    //            _animator.SetTrigger("Die");
    //            if (_deathSound != null)
    //            {
    //                _audioSource.PlayOneShot(_deathSound);
    //            }
    //            _damagedCurrentDuration = 20.0f;
    //        }
    //        else
    //        {
    //            _animator.SetTrigger("GetAttacked");
    //            animationHandler.GoBackToIdle();
    //            _damagedCurrentDuration = _damagedMaxDuration;
    //        }
    //        if (_hitSound != null)
    //        {
    //            _audioSource.PlayOneShot(_hitSound);
    //        }
    //    }
    //}

    //// Do damage with invincibility cooldown
    //public override void TakeDamage(float damageTaken, float invincibilityLength)
    //{
    //    if (!isInvincible)
    //    {
    //        _currentHP -= damageTaken;
    //        _invincibilityCooldown += invincibilityLength;
    //        if (_currentHP <= 0)
    //        {
    //            isDead = true;
    //            _animationHasReset = false;
    //            _animator.SetTrigger("Die");
    //            if (_deathSound != null)
    //            {
    //                _audioSource.PlayOneShot(_deathSound);
    //            }
    //            _damagedCurrentDuration = 20.0f;
    //        }
    //        else
    //        {
    //            _animator.SetTrigger("GetAttacked");
    //            animationHandler.GoBackToIdle();
    //            _damagedCurrentDuration = _damagedMaxDuration;
    //        }
    //        if (_hitSound != null)
    //        {
    //            _audioSource.PlayOneShot(_hitSound);
    //        }
    //    }
    //}
}

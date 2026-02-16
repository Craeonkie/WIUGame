using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

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
    [SerializeField] private Rigidbody myRigidbody;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float playerRotationSpeed;
    private float _currentSpeed;

    [Header("Other scripts of note")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AnimationHandler animationHandler;

    private Vector2 _inputMove;
    private bool _isJumping = false;
    private Vector3 _velocity;

    // Animator weights for running
    private float _currentWeight;
    private float _targetWeight;

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
    }

    protected override void Update()
    {
        base.Update();

        bool canMove = true;
        bool isGrounded = groundChecker.IsGrounded();

        _inputMove = _moveAction.ReadValue<Vector2>();

        // Rotation
        if (_inputMove != Vector2.zero && canMove)
        {
            Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTarget.transform.eulerAngles.y, 0);
            Vector3 cameraForward = cameraYawOnly * Vector3.forward;
            Vector3 cameraRight = cameraYawOnly * Vector3.right;

            Vector3 moveDir = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

            float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, newY, 0);
            _targetWeight = 1;
        }
        else
        {
            _targetWeight = 0;
        }

        // Jumping
        if (isGrounded && !_isJumping)
        {
            if (_jumpAction.WasPressedThisDynamicUpdate() && canMove)
            {
                _velocity.Set(_velocity.x, _jumpPower, _velocity.z);
                _isJumping = true;
                animationHandler.ToggleAbilityToAct(false);
            }
            else
            {
                animationHandler.ToggleAbilityToAct(true);
            }
        }

        // Reset jump state when grounded
        if (_isJumping && _velocity.y < 0)
        {
            _isJumping = false;
        }

        if (!isGrounded || _isJumping)
        {
            _velocity.y += -9.8f * Time.deltaTime;
        }
        else
        {
            _velocity.y = 0;
        }

        // Handle max velocity
        float targetSpeed = 0.0f;
        bool isMoving = _inputMove != Vector2.zero && canMove;
        if (isMoving)
        {
            targetSpeed = _maxSpeed;
        }

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _maxSpeed / 0.1f * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * _currentSpeed;
        _velocity.Set(moveVelocity.x, _velocity.y, moveVelocity.z);
        myRigidbody.linearVelocity = _velocity;

        //// Handle other inputs
        // Interacting
        if (_interactAction.WasPressedThisDynamicUpdate())
        {
            inventory.TryToInteract();
        }

        // Dropping
        if (_dropAction.WasPressedThisDynamicUpdate())
        {
            inventory.TryToDropItem();
        }

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

        // Update interaction handler
        //animationHandler.ToggleAbilityToAct(true);

        // Send parameters to animator
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("IsGrounded", isGrounded);
        _animator.SetBool("IsJumping", _isJumping);
        _animator.SetFloat("Y Velocity", _velocity.y);

        _currentWeight = Mathf.MoveTowards(_currentWeight, _targetWeight, Time.deltaTime * 10);
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
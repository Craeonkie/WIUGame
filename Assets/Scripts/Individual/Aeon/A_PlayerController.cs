using Unity.Cinemachine;
using UnityEditor.Build;
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
    [SerializeField] private float rollDuration;
    [SerializeField] private float landDuration;
    [SerializeField] private float playerRotationSpeed;

    [Header("Camera Movement")]
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject thirdPersonCamera;
    public GameObject cameraTarget;
    [SerializeField] private MouseMovement[] mouseRotationScripts;

    [Header("Aiming")]
    [SerializeField] private float _throwRotationSpeed = 480.0f;
    [SerializeField] private float _endThrowNormalizedTime = 0.8f;
    [SerializeField] private string _throwingAnimationName = "Throw";
    [SerializeField] private float _defaultBlendSpeed = 2.0f;
    [SerializeField] private float _aimBlendingSpeed = 1.0f;
    [SerializeField] private bool _setBlendingSpeedBackToNormal = false;

    [Header("Energy System")]
    [SerializeField] private float _maxEnergy = 100.0f;
    [SerializeField] private float _remainingEnergy = 0.0f;
    [SerializeField] private float rollEnergyRequired;
    [SerializeField] private float energyPassiveRegeneration;

    [Header("Special Cooldown")]
    [SerializeField] private float _maxSpecialCooldown = 1.0f;
    [SerializeField] private float _currentSpecialCooldown = 0.0f;
    [SerializeField] private bool _canUseSpecial = true;

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
    [SerializeField] private bool _handsAreFree;
    [SerializeField] private float _currentWeight;
    [SerializeField] private float _targetWeight;

    [Header("Other scripts of note")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AnimationHandler animationHandler;
    [SerializeField] protected Animator _animator;

    [Header("Events to invoke")]
    public static System.Action OnInteract;

    private Vector2 _inputMove;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _isRolling = false;
    [SerializeField] private bool _isMovingObject = false;
    [SerializeField] private bool _isHoldingItem = false;
    [SerializeField] private bool _canPlayerInput = true;
    [SerializeField] private bool _playerInputPaused = true;
    [SerializeField] private bool _isAiming = false;
    [SerializeField] private bool _wasGroundedPreviously = false;
    [SerializeField] private GameObject _itemBeingMoved;

    public static System.Action<float, float> OnPlayerHealthChanged;

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

        _remainingEnergy = _maxEnergy;
        _canUseSpecial = true;
    }

    protected override void Update()
    {
        base.Update();

        // Handle stunned
        if (_currentStunDuration > 0)
        {
            _currentStunDuration -= Time.deltaTime;
            _isStunned = true;
            if (_currentStunDuration <= 0)
            {
                _isStunned = false;
            }
        }

        // Handle special cooldown
        if (_currentSpecialCooldown > 0)
        {
            _currentSpecialCooldown -= Time.deltaTime;
            if (_currentSpecialCooldown <= 0)
            {
                _canUseSpecial = true;
            }
        }

        // Stamina regeneration
        if (_remainingEnergy < _maxEnergy && energyPassiveRegeneration != 0)
        {
            _remainingEnergy = Mathf.MoveTowards(_remainingEnergy, _maxEnergy, energyPassiveRegeneration * Time.deltaTime);
        }

        // Handle player landed duration
        if (_currentLandTimer > 0)
        {
            _currentLandTimer -= Time.deltaTime;
        }

        bool canMove = animationHandler.CanMove();
        bool isGrounded = groundChecker.IsGrounded();
        _inputMove = _moveAction.ReadValue<Vector2>();
        bool isMoving = _inputMove != Vector2.zero && canMove && !_isStunned;

        // Only accept input and rotation if player isn't stunned
        if (!_isStunned)
        {
            // Handle rotation
            if (isMoving && !_isMovingObject && !_isAiming && !_isRolling && _currentLandTimer <= 0)
            {
                Quaternion cameraYawOnly = Quaternion.Euler(0, followCamera.transform.eulerAngles.y, 0);
                Vector3 cameraForward = cameraYawOnly * Vector3.forward;
                Vector3 cameraRight = cameraYawOnly * Vector3.right;

                Vector3 moveDir = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

                float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
                transform.eulerAngles = new Vector3(0, newY, 0);
                _rollDirection = transform.forward;
            }

            // Runs if is grounded and not jumping
            if (isGrounded && !_isJumping)
            {
                // Jumping
                if (_jumpAction.WasPressedThisDynamicUpdate() && canMove && _canPlayerInput && !_isAiming && !_isMovingObject)
                {
                    _isJumping = true;
                    _isRolling = false;
                    myRigidbody.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
                }
                // Rolling (Only if the player has enough stamina)
                else if (_rollAction.WasPressedThisDynamicUpdate() && canMove && _canPlayerInput && !_isRolling && !_isAiming && !_isMovingObject)
                {
                    if (UseEnergy(rollEnergyRequired, false))
                    {
                        _animator.SetTrigger("IsRolling");
                        _currentRollTimer = rollDuration;
                        _isRolling = true;
                        isDodging = true;
                        _canPlayerInput = false;
                    }
                }
            }

            // Reenable ability to act if player is not midair and not rolling
            if (isGrounded && !_isRolling && !_isMovingObject && !_isStunned && !_playerInputPaused)
            {
                _canPlayerInput = true;
            }

            // Handle movement
            if (!_isRolling && !myRigidbody.isKinematic && !_isStunned)
            {
                // Handle max velocity
                float targetSpeed = 0.0f;
                if (isMoving && _currentLandTimer <= 0)
                {
                    targetSpeed = _maxSpeed;
                }

                // Always move forward
                if (!_isAiming)
                {
                    _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _maxSpeed / 0.1f * Time.deltaTime);
                    Vector3 moveVelocity = transform.forward * _currentSpeed;
                    myRigidbody.linearVelocity = new Vector3(moveVelocity.x, myRigidbody.linearVelocity.y, moveVelocity.z);
                }
                // Move according to camera forward
                else
                {
                    _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _maxSpeed / 0.1f * Time.deltaTime);

                    Quaternion cameraYawOnly = Quaternion.Euler(0, thirdPersonCamera.transform.eulerAngles.y, 0);
                    Vector3 cameraForward = cameraYawOnly * Vector3.forward;
                    Vector3 cameraRight = cameraYawOnly * Vector3.right;

                    Vector3 moveDir = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
                    Vector3 moveVelocity = moveDir * _currentSpeed;

                    myRigidbody.linearVelocity = new Vector3(moveVelocity.x, myRigidbody.linearVelocity.y, moveVelocity.z);
                }
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
                    _canPlayerInput = true;
                }
            }
        }

        // Handle breaking items (A bit scuffed)
        if (inventory.ReturnCurrentItem() != null)
        {
            Item temp = inventory.ReturnCurrentItem().GetComponent<Item>();
            if (temp.CheckIfBroken())
            {
                inventory.DropItem(temp.gameObject);
            }
        }

        //// Handle other inputs
        // Cast a sphere around the player (or use a raycast forward if preferred)
        if (!_isMovingObject)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _pickupRange, interactablesLayer);

            Interactable closestInteractable = null;
            float closestDist = _pickupRange;

            foreach (Collider col in hits)
            {
                bool alreadyHolding = false;

                if (col.gameObject == inventory.ReturnPrimaryItem() || col.gameObject == inventory.ReturnSecondaryItem())
                {
                    alreadyHolding = true;
                }

                if (alreadyHolding || (!_handsAreFree && !col.CompareTag("Interactable")))
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
                    string tag = closestInteractable.tag;

                    OnInteract?.Invoke();

                    // Act according to the item's tag
                    if (tag == "Weapon")
                    {
                        inventory.PutItemInPrimary(closestInteractable.gameObject, this);
                        inventory.EquipPrimary();
                        animationHandler.EquipItem((Item)closestInteractable);
                    }
                    else if (tag == "Item")
                    {
                        inventory.PutItemInSecondary(closestInteractable.gameObject, this);
                        inventory.EquipSecondary();
                        animationHandler.EquipItem((Item)closestInteractable);
                    }
                    else if (tag == "Interactable")
                    {
                        Debug.Log("here");
                        closestInteractable.InteractWith();
                    }
                    //else if (tag == "Moveable")
                    //{
                    //    closestInteractable.InteractWith();
                    //    StartMovingItem(closestInteractable.gameObject);
                    //}
                }
            }
        }
        //else
        //{
        //    //if (_jumpAction.WasPressedThisDynamicUpdate() || _interactAction.WasPressedThisDynamicUpdate())
        //    //{
        //    //    StopMovingItem();
        //    //}
        //}

        // Only accept input when the player is able act
        if (_canPlayerInput && !_isStunned && isGrounded && _handsAreFree)
        {
            // Primary
            if (_primaryAction.WasPressedThisDynamicUpdate() && !_isAiming)
            {
                animationHandler.TryingToUsePrimary(true);
                if (inventory.ReturnCurrentItem() != null && inventory.ReturnCurrentItem().TryGetComponent<ThrowableItem>(out _))
                {
                    StartAiming();
                }
            }
            if (_primaryAction.WasReleasedThisDynamicUpdate())
            {
                animationHandler.TryingToUsePrimary(false);
            }

            // Secondary
            if (_secondaryAction.WasPressedThisDynamicUpdate() && !_isAiming)
            {
                animationHandler.TryingToUseSecondary(true);
            }
            if (_secondaryAction.WasReleasedThisDynamicUpdate() && !_isAiming)
            {
                animationHandler.TryingToUseSecondary(false);
            }

            // Special
            if (_canUseSpecial)
            {
                if (_specialAction.WasPressedThisDynamicUpdate() && !_isAiming)
                {
                    animationHandler.TryingToUseSpecial(true);
                }
                if (_specialAction.WasReleasedThisDynamicUpdate() && !_isAiming)
                {
                    animationHandler.TryingToUseSpecial(false);
                }
            }

            if (!animationHandler.IsActing() && !_isAiming)
            {
                // Dropping
                if (_dropAction.WasPressedThisDynamicUpdate() && !_isStunned)
                {
                    if (inventory.ReturnCurrentItem() != null)
                    {
                        inventory.DropItem(inventory.ReturnCurrentItem());
                    }
                }

                // Equip Primary
                if (_equipPrimary.WasPressedThisDynamicUpdate() && !_isStunned)
                {
                    inventory.EquipPrimary();
                    animationHandler.UnequipItem();
                    if (inventory.ReturnCurrentItem() != null)
                    {
                        animationHandler.EquipItem(inventory.ReturnCurrentItem().GetComponent<Item>());
                    }
                }

                // Equip Secondary
                if (_equipSecondary.WasPressedThisDynamicUpdate() && !_isStunned)
                {
                    inventory.EquipSecondary();
                    animationHandler.UnequipItem();
                    if (inventory.ReturnCurrentItem() != null)
                    {
                        animationHandler.EquipItem(inventory.ReturnCurrentItem().GetComponent<Item>());
                    }
                }
            }
        }

        // Handle aiming camera logic
        if (_isAiming)
        {
            // Handle player in throwing animation
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_throwingAnimationName))
            {
                // Make the player stop aiming and go back to normal
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > _endThrowNormalizedTime)
                {
                    StopAiming();
                }
                // Make the player stop inputting when throwing
                else
                {
                    foreach (MouseMovement mouseRotationScript in mouseRotationScripts)
                    {
                        mouseRotationScript.enabled = false;
                    }
                }
            }
            // Make player rotate towards the correct direction when aiming
            else
            {
                if (cameraTarget.transform.localEulerAngles.y > 0)
                {
                    Vector3 cameraTargetRot = cameraTarget.transform.localEulerAngles;
                    Vector3 playerRot = transform.localEulerAngles;
                    if (cameraTargetRot.y >= 180.0f)
                    {
                        cameraTargetRot.y -= 360.0f;
                    }

                    float newCameraY = Mathf.MoveTowards(cameraTargetRot.y, 0f, _throwRotationSpeed * Time.deltaTime);
                    float delta = cameraTargetRot.y - newCameraY;

                    cameraTargetRot.y = newCameraY;
                    playerRot.y += delta;

                    cameraTarget.transform.localEulerAngles = cameraTargetRot;
                    transform.localEulerAngles = playerRot;

                    foreach (MouseMovement mouseRotationScript in mouseRotationScripts)
                    {
                        mouseRotationScript.enabled = false;
                    }
                }
                // Only let player rotate when they are facing the correct direction
                else
                {
                    foreach (MouseMovement mouseRotationScript in mouseRotationScripts)
                    {
                        mouseRotationScript.enabled = true;
                    }
                }
            }
        }

        if (_setBlendingSpeedBackToNormal && Camera.main.TryGetComponent<CinemachineBrain>(out CinemachineBrain cinemachineBrain))
        {
            if (cinemachineBrain.ActiveBlend != null)
            {
                _setBlendingSpeedBackToNormal = false;
                cinemachineBrain.DefaultBlend.Time = _defaultBlendSpeed;
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
        // Only trigger if not already holding the item and the item isn't null
        if (inventory.ReturnSecondaryItem() != null && inventory.ReturnSecondaryItem() == inventory.ReturnCurrentItem())
        {
            _isHoldingItem = true;
        }
        else
        {
            _isHoldingItem = false;
        }
        _animator.SetBool("IsHoldingItem", _isHoldingItem);
        if (_handsAreFree)
        {
            _targetWeight = 0;
        }
        else
        {
            _targetWeight = 1;
        }
        _currentWeight = Mathf.MoveTowards(_currentWeight, _targetWeight, Time.deltaTime * 2.0f);
        _animator.SetLayerWeight(1, _currentWeight);
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

    //// Check if current animation is over
    //public virtual bool CurrentAnimationOver(int state)
    //{
    //    AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(state);

    //    if (stateInfo.normalizedTime < 1.0f)
    //    {
    //        _animationHasReset = true;
    //    }

    //    return (stateInfo.normalizedTime >= 1.0f) && _animationHasReset;
    //}

    // Stun the player
    public virtual void Stun(float stunDuration)
    {
        _currentStunDuration = stunDuration;
        animationHandler.GoBackToIdle();
        _isStunned = true;
        _isRolling = false;
        myRigidbody.linearVelocity = Vector3.zero;
    }

    // Aim in preparation to throw an object or item
    public virtual void StartAiming()
    {
        _isAiming = true;
        AlignCameraTarget();
        if (Camera.main.TryGetComponent<CinemachineBrain>(out CinemachineBrain cinemachineBrain))
        {
            cinemachineBrain.DefaultBlend.Time = _aimBlendingSpeed;
        }
        thirdPersonCamera.GetComponent<CinemachineCamera>().Priority = 10;
        followCamera.GetComponent<CinemachineCamera>().Priority = 9;
        foreach (MouseMovement mouseRotationScript in mouseRotationScripts)
        {
            mouseRotationScript.enabled = true;
        }
    }

    // Make the camera target rotate towards the follow camera
    public void AlignCameraTarget()
    {
        cameraTarget.transform.rotation = Quaternion.Euler(0.0f, followCamera.transform.eulerAngles.y, 0.0f);
    }

    // Stop aiming
    public virtual void StopAiming()
    {
        _isAiming = false;
        CenterFollowCamera();
        thirdPersonCamera.GetComponent<CinemachineCamera>().Priority = 9;
        followCamera.GetComponent<CinemachineCamera>().Priority = 10;
        _setBlendingSpeedBackToNormal = true;
        foreach (MouseMovement mouseRotationScript in mouseRotationScripts)
        {
            mouseRotationScript.enabled = false;
        }
    }

    // Set camera to face the player's forward
    public void CenterFollowCamera()
    {
        followCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value = transform.rotation.eulerAngles.y;
        followCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value = 10.0f;
    }

    // Do damage with invincibility cooldown
    public override void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (!isInvincible && !isDodging)
        {
            _currentHP -= damageTaken;
            _animator.SetTrigger("GetHit");
            _invincibilityMaxCooldown = invincibilityLength;
            _invincibilityCooldown = invincibilityLength;
            OnPlayerHealthChanged?.Invoke(_currentHP, _maxHP);

            if (hitAudio.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(hitAudio[Random.Range(0, hitAudio.Length - 1)]);
            }
            if (_currentHP <= 0)
            {
                audioSource.PlayOneShot(deathAudio);
                Die();
            }
            else
            {
                if (_invincibilityCooldown > 0)
                {
                    isInvincible = true;
                }
            }

            InterruptAction();
        }
    }

    // Interrupt the player's action
    public void InterruptAction()
    {
        //if (_isMovingObject)
        //{
        //    StopMovingItem();
        //}
    }

    // Toggle player ability to move and input
    public void TogglePlayerAbilityToAct(bool canAct)
    {
        InterruptAction();
        _playerInputPaused = canAct;
        _canPlayerInput = canAct;
    }

    // Toggle player ability to move and input
    public void TogglePlayerAbilityToPickUpitems(bool canPickUpItems)
    {
        _handsAreFree = canPickUpItems;
    }

    // Sihan function
    public void SetPlayerToTransform(Transform anchor)
    {
        if (myRigidbody.isKinematic)
        {
            myRigidbody.isKinematic = false;

            myRigidbody.linearVelocity = Vector3.zero;
            myRigidbody.angularVelocity = Vector3.zero;
        }

        myRigidbody.isKinematic = true;

        myRigidbody.transform.position = anchor.position;
        myRigidbody.transform.rotation = anchor.rotation;
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;

        myRigidbody.isKinematic = false;
    }

    // Die
    public override void Die()
    {
        _animator.SetTrigger("Die");
        _animator.SetLayerWeight(3, 1);
        Stun(1000);
    }

    // Use energy (used to perform special attack mostly)
    public bool UseEnergy(float energyUsed, bool usedSpecial)
    {
        if (_remainingEnergy >= energyUsed)
        {
            if (usedSpecial)
            {
                _canUseSpecial = false;
                _currentSpecialCooldown += _maxSpecialCooldown;
            }

            _remainingEnergy -= energyUsed;
            return true;
        }

        return false;
    }
}
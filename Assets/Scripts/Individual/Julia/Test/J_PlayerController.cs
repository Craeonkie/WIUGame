using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class J_PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _sword;
    [SerializeField] private Transform _sheathedTransform;
    [SerializeField] private Transform _unsheathedTransform;
    private InputActionAsset _inputActions;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera _shiftLockCamera;

    [Header("Player Stamina")]
    private float _playerStamina;
    [SerializeField] private float _maxStamina;
    [SerializeField] private float _staminaRecoverySpeed;
    [SerializeField] private float _runStaminaCost;
    [SerializeField] private float _blockHitStaminaCost;
    [SerializeField] private float _slideStaminaCost;
    [SerializeField] private float _dashStaminaCost;
    [SerializeField] private float _specialAttackStaminaCost;
    [SerializeField] private float _parryStaminaCost;
    private float _blockTime;

    [Header("Damage")]
    [SerializeField] private int _playerSwordDamage;
    [SerializeField] private int _parryDamage;

    [Header("Movement Settings")]
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jogAirMoveSpeed;
    [SerializeField] private float _crouchAirMoveSpeed;
    [SerializeField] private float _runAirMoveSpeed;
    private Vector2 _moveDirection;
    private float _airMoveSpeed;
    private Vector3 _velocity;
    private float _gravity = -9.81f; // default value
    private float _airTime;

    [Header("Lock On Setting")]
    [SerializeField] private float _lockOnLength;

    private bool _isDead = false;

    private GameObject _target;

    [Header("Animation Settings")]
    [SerializeField] private float _setCrouchColliderDuration;
    [SerializeField] private float _resetCrouchColliderDuration;
    [SerializeField] private float _setJumpColliderDuration; // 0.25f
    [SerializeField] private float _resetJumpColliderDuration;
    [SerializeField] private float _setSlideColliderDuration; // 0.25f
    [SerializeField] private float _resetSlideColliderDuration;
    [SerializeField]  private float _crouchColliderCenterY = 0.67f;
    [SerializeField]  private float _crouchColliderHeight = 1.4f;
    [SerializeField]  private float _fallColliderCenterY = 1.29f;
    [SerializeField]  private float _fallColliderHeight = 1.2f;
    [SerializeField]  private float _slideColliderCenterY = 1.2f;
    [SerializeField]  private float _slideColliderHeight = 1.2f;
    private Vector3 _originalColliderCenter;
    private float _originalColliderHeight;
    private IEnumerator _setJumpColliderCoroutine = null;
    private IEnumerator _setCrouchColliderCoroutine = null;

    private Queue<char> _dashInput;
    private bool _isDashing = false;
    private float _dashBufferTime = 0.5f;
    private IEnumerator _dashClearCoroutine = null;

    private bool _isStairsDetected = false;

    // Actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _leftAttackAction;
    private InputAction _rightAttackAction;
    private InputAction _specialAttackAction;

    private InputAction _lookAction;

    private bool _isJumping = false;

    private bool _isCrouching = false;
    private bool _isSliding = false;
    private bool _canQueueAttack = true;
    private bool _isAttacking = false;
    private bool _isBlocking = false;
    private bool _isSwordEquipped = false;

    // Events
    public static System.Action<Vector2> OnLook;
    public static System.Action OnSwitchCamera;
    public static System.Action<Vector2> OnZoom;
    public static System.Action<GameObject> OnLock;
    public static System.Action OnToggleWeapon;
    public static System.Action OnOpenMenu;
    public static System.Action OnDead;
    public static System.Action<bool> OnBlock;
    public static System.Action<float, float> OnStaminaChange;

    public static System.Action<Vector3> OnMove;

    public enum MOVESTATE
    {
        NONE,
        WALK,
        RUN,
        CROUCH,
        SLIDE,
    }
    private MOVESTATE _currentMoveState = MOVESTATE.NONE;


    // Attack
    public enum ATTACKTYPE
    {
        LIGHT1 = 2,
        LIGHT2 = 3,
        LIGHT3 = 5,
        HEAVY1 = 4,
        HEAVY2 = 6,
        HEAVY3 = 8,
        SPECIAL = 10,
        LIGHTHEAVYCOMBO1 = 7,
        LIGHTHEAVYCOMBO2 = 9,
        AIR = 12
    }

    public enum ATTACK {
        LIGHT = 1,
        HEAVY = 2,
        SPECIAL = 3,
        AIR = 4
    }

    //[SerializeField] private string[] _attackNames;
    private Dictionary<int, ATTACKTYPE> _attacks = new Dictionary<int, ATTACKTYPE>
    {
        { 1, ATTACKTYPE.LIGHT1 },
        { 11, ATTACKTYPE.LIGHT2 },
        { 111, ATTACKTYPE.LIGHT3 },
        { 2, ATTACKTYPE.HEAVY1 },
        { 22, ATTACKTYPE.HEAVY2 },
        { 222, ATTACKTYPE.HEAVY3 },
        { 3, ATTACKTYPE.SPECIAL },
        { 112, ATTACKTYPE.LIGHTHEAVYCOMBO1 },
        { 221, ATTACKTYPE.LIGHTHEAVYCOMBO2 },
        { 4 , ATTACKTYPE.AIR}
    };
    private Dictionary<ATTACKTYPE, string> _meleeAttackAnimationNames = new Dictionary<ATTACKTYPE, string>
    {
        { ATTACKTYPE.LIGHT1, "rightPunch" },
        { ATTACKTYPE.LIGHT2, "leftPunch" },
        { ATTACKTYPE.LIGHT3, "comboPunch" },
        { ATTACKTYPE.HEAVY1, "crossPunch" },
        { ATTACKTYPE.HEAVY2, "leftHook" },
        { ATTACKTYPE.HEAVY3, "rightJabandElbow" },
        { ATTACKTYPE.SPECIAL, "flyingKick" },
        { ATTACKTYPE.LIGHTHEAVYCOMBO1, "roundhouseKick" },
        { ATTACKTYPE.LIGHTHEAVYCOMBO2, "360kick" },
        { ATTACKTYPE.AIR, "kneeAndPunch" }
    };
    private Dictionary<ATTACKTYPE, string> _swordAttackAnimationNames = new Dictionary<ATTACKTYPE, string>
    {
        { ATTACKTYPE.LIGHT1, "lightSlash1" },
        { ATTACKTYPE.LIGHT2, "lightSlash2" },
        { ATTACKTYPE.LIGHT3, "lightSlash3" },
        { ATTACKTYPE.HEAVY1, "heavySlash1" },
        { ATTACKTYPE.HEAVY2, "heavySlash2" },
        { ATTACKTYPE.HEAVY3, "heavySlash3" },
        { ATTACKTYPE.SPECIAL, "swordSpecial" },
        { ATTACKTYPE.LIGHTHEAVYCOMBO1, "lightToHeavyCombo" },
        { ATTACKTYPE.LIGHTHEAVYCOMBO2, "heavyToLightCombo" },
        { ATTACKTYPE.AIR, "swordAirAttack" }
    };

    private List<IEnumerator> _attackQueue = new List<IEnumerator>();
    private List<int> _attackList = new List<int>();

    private void OnEnable()
    {
        transform.GetComponent<J_Damageable>().OnHit += HandleHit;
        transform.GetComponent<J_Damageable>().OnDead += HandleDeath;
    }

    private void OnDisable()
    {
        transform.GetComponent<J_Damageable>().OnHit -= HandleHit;
        transform.GetComponent<J_Damageable>().OnDead -= HandleDeath;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActions = _playerInput.actions;

        _playerStamina = _maxStamina;

        // Actions
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _leftAttackAction = _playerInput.actions["Primary"];
        _rightAttackAction = _playerInput.actions["Secondary"];
        _specialAttackAction = _playerInput.actions["Special"];

        _lookAction = _playerInput.actions["Look"];

        _originalColliderCenter = _controller.center;
        _originalColliderHeight = _controller.height;

        _airMoveSpeed = 0f;

        _dashInput = new Queue<char>();

        _target = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDead)
            return;

        // Handle movement
        HandleMovement();
        CheckMovementState();

        // Handle dashing
        HandleDash();

        // Handle jumping
        HandleJump();

        // Handle Block
        HandleBlock();

        // Handle attack
        //HandleAttack();

        // Handle movement based on camera
        HandleLook();

        // Handle menu
        HandleMenu();

        // Check for stairs
        HandleStairs();

        // Stamina recovery
        if (_playerStamina < _maxStamina)
            _playerStamina += _staminaRecoverySpeed * Time.deltaTime;
        else
            _playerStamina = _maxStamina;

        OnStaminaChange?.Invoke(_playerStamina, _maxStamina);

        // Check for falling
        _animator.SetBool("IsGrounded", _controller.isGrounded);
        _animator.SetBool("IsFalling", !_controller.isGrounded);
    }

    private void HandleMovement()
    {
        _moveDirection = Vector2.Lerp(_moveDirection, _moveAction.ReadValue<Vector2>(), 10f * Time.deltaTime);


        _animator.SetFloat("Horizontal", _moveDirection.x);
        _animator.SetFloat("Vertical", _moveDirection.y);

        // Idle / Crouch Idle
        if (_moveDirection.magnitude <= 0 && !_isCrouching)
        {
            // Toggle both animator booleans off
            _currentMoveState = MOVESTATE.NONE;
            _animator.SetBool("IsRunning", false);
        }
        // Walk
        else
        {
            _currentMoveState = MOVESTATE.WALK;

            // Check move direction
            _animator.SetBool("IsRunning", false);
        }
    }

    private void HandleDash()
    {
        if (_dashInput.Count > 1 || !_controller.isGrounded || _isDashing)
            return;

        if (_moveAction.WasPressedThisFrame() && _playerStamina > _dashStaminaCost)
        {
            Vector2 _moveDir = _moveAction.ReadValue<Vector2>();
            char nextDashInput = '-';

            // Forward
            if (_moveDir.y == 1) nextDashInput = 'W';
            else if (_moveDir.y == -1) nextDashInput = 'S'; 
            else if (_moveDir.x == 1) nextDashInput = 'D';
            else if (_moveDir.x == -1) nextDashInput = 'A';

            // Check queue
            if (_dashInput.Count == 0 && nextDashInput != '-') {
                _dashInput.Enqueue(nextDashInput);
                if (_dashClearCoroutine != null)
                    StopCoroutine(_dashClearCoroutine);
                
                _dashClearCoroutine = ClearDashQueue();
                StartCoroutine(_dashClearCoroutine);
            }
            // Check Dash
            else if (_dashInput.Count > 0)
            {
                if (_dashInput.Peek() != nextDashInput)
                    _dashInput.Clear();
                else
                {
                    switch (nextDashInput)
                    {
                        case 'W':
                            _animator.SetTrigger("DashForward");
                            break;
                        case 'S':
                            _animator.SetTrigger("DashBackward");
                            break;
                        case 'A':
                            _animator.SetTrigger("DashLeft");
                            break;
                        case 'D':
                            _animator.SetTrigger("DashRight");
                            break;
                    }

                    _isDashing = true;
                    _dashInput.Enqueue(nextDashInput);

                    _playerStamina -= _dashStaminaCost;
                    _playerStamina = Mathf.Max(0, _playerStamina);

                    //AudioManager.Instance.PlayOneShot("dodge");

                    if (_dashClearCoroutine != null)
                        StopCoroutine(_dashClearCoroutine);
                }
            }
        }
    }

    private IEnumerator ClearDashQueue()
    {
        yield return new WaitForSeconds(_dashBufferTime);

        _dashInput.Clear();
        _isDashing = false;
    }

    public void EndDash()
    {
        _dashClearCoroutine = ClearDashQueue();
        StartCoroutine(_dashClearCoroutine);
    }


    private void HandleJump()
    {
        // Check jumping
        if (_isJumping && _velocity.y < 0)
        {
            _isJumping = false;
            _animator.SetBool("IsJumping", false);
            _animator.SetBool("IsFalling", true);
        }

        if (_jumpAction.WasPressedThisFrame())
        {
            // Stand up
            if (_currentMoveState == MOVESTATE.CROUCH)
            {
                _isCrouching = false;
                _animator.SetBool("IsCrouching", _isCrouching);
            }
            else if (_controller.isGrounded && (IsInCurrentAnimationState("Jog") || IsInCurrentAnimationState("Run") || _currentMoveState > MOVESTATE.NONE) && !_isAttacking)
            {
                _animator.SetBool("IsJumping", true);
                _animator.SetBool("IsFalling", false);
            }
        }
    }

    // Not every jump animation is synced
    public void StartJump()
    {
        _isJumping = true;

        _canQueueAttack = false;
        ParseAttack(); // Immediately clear attack, this usually occurs if player spams left click when about to jump and it just so happens to fall within this buffer zone

        _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        
        _setJumpColliderCoroutine = SetCollider(_setJumpColliderDuration, new Vector3(0f, _fallColliderCenterY, 0), _fallColliderHeight);
        StartCoroutine(_setJumpColliderCoroutine);
    }

    public void OnLand()
    {
        _velocity.y = -2f;

        if (_setJumpColliderCoroutine != null)
            StopCoroutine(_setJumpColliderCoroutine);

        _setJumpColliderCoroutine = SetCollider(_resetJumpColliderDuration, _originalColliderCenter, _originalColliderHeight);
        StartCoroutine(_setJumpColliderCoroutine);
    }


    public void StopSlide()
    {
        _isSliding = false;
        _animator.SetBool("IsSliding", _isSliding);
    }
    public void ResetSlideCollider()
    {
        StartCoroutine(SetCollider(_resetSlideColliderDuration, _originalColliderCenter, _originalColliderHeight));
    }


    private void HandleBlock()
    {
        //if (_blockAction.IsPressed())
        //{
        //    if (!_isBlocking)
        //        OnBlock?.Invoke(true);

        //    _isBlocking = true;
        //    _blockTime += Time.deltaTime;
        //}
        //else
        //{
        //    if (_isBlocking)
        //        OnBlock?.Invoke(false);

        //    _isBlocking = false;
        //    _blockTime = 0f;
        //}

        //_animator.SetBool("IsBlocking", _isBlocking);
        //_animator.SetFloat("BlockTime", _blockTime);
    }

    private void HandleHit(Vector2 hitSource)
    {
        // Get the direction of hit
        Vector2 dir = (hitSource - new Vector2(transform.position.x, transform.position.z)).normalized;

        // Get the angle between the forward direction of the player and the hit direction
        // If angle is more than 0 , its turning to the left, else its turning to the right
        // Front --> Angle is less than 45, and more than -45
        // Right --> Angle is more than 45 and less than 135
        // Left --> Angle is less than -45 and more than -135
        // Back --> Angle is more than 135 or less than -135
        Vector2 forward2D = new Vector2(transform.forward.x, transform.forward.z);
        float angle = Vector2.SignedAngle(forward2D, dir);

        bool isHitFromFront = angle > -45f && angle < 45f;

        Debug.Log("dir.y: " + dir.y);
        Debug.Log("Is Blocking: " + _isBlocking);

        // Blocking animation
        if (_isBlocking && isHitFromFront)
        {
            _animator.SetTrigger("BlockHit");
            int extraDamage = _isSwordEquipped ? _playerSwordDamage : 0;
            transform.GetComponent<J_AttackHandler>().SetDamage(extraDamage + _parryDamage);

            _playerStamina -= _blockHitStaminaCost;
            _playerStamina = Mathf.Max(0, _playerStamina);

            //if (_isSwordEquipped)
            //    AudioManager.Instance.PlayOneShot("swordBlockHit");
            //else
            //    AudioManager.Instance.PlayOneShot("blockHit");

            if (_playerStamina <= 0)
            {
                _isBlocking = false;
                OnBlock?.Invoke(false);
            }
        }
        // Normal hit animation
        else
        {
            _animator.SetFloat("Angle", angle);
            _animator.SetTrigger("Hit");

            //AudioManager.Instance.PlayOneShot("hurt");

            _isBlocking = false;
            OnBlock?.Invoke(false);
        }

        _dashInput.Clear();
        _isDashing = false;

        ParseAttack();
    }

    private void HandleDeath(Vector2 hitSource)
    {
        Debug.Log("die");

        // Get the direction of hit
        Vector2 dir = (hitSource - new Vector2(transform.position.x, transform.position.z)).normalized;

        // Get the angle between the forward direction of the player and the hit direction
        // If angle is more than 0 , its turning to the left, else its turning to the right
        // Front --> Angle is less than 45, and more than -45
        // Right --> Angle is more than 45 and less than 135
        // Left --> Angle is less than -45 and more than -135
        // Back --> Angle is more than 135 or less than -135
        Vector2 forward2D = new Vector2(transform.forward.x, transform.forward.z);
        float angle = Vector2.SignedAngle(forward2D, dir);

        //AudioManager.Instance.PlayOneShot("death");
        
        ParseAttack();

        _animator.SetBool("IsDead", true);
        _animator.SetTrigger("Die");
        _isDead = true;
    }

    public void InvokeDeathScreen() => OnDead?.Invoke();


    public void SetSword()
    {
        if (_isSwordEquipped)
        {
            _sword.transform.parent = _unsheathedTransform;
            _sword.transform.localPosition = Vector3.zero;
            _sword.transform.localRotation = Quaternion.Euler(0f, -279f, 0f);
        }
        else
        {
            _sword.transform.parent = _sheathedTransform;
            _sword.transform.localPosition = Vector3.zero;
            _sword.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }


    private void HandleAttack()
    {
        //if (!_canQueueAttack || GetAttackStep() > 100 || !_controller.isGrounded || _isDashing || _isBlocking)
        //    return;

        if (!_canQueueAttack || GetAttackStep() > 100 || 
            (GetAttackStep() == 0 && (IsInCurrentAnimationState("Jog") || IsInCurrentAnimationState("Run")) != true))
            return;

        // Left Attack / Air Attack
        if (_leftAttackAction.IsPressed() && _animator.GetBool("IsJumping"))
        {
            if (ValidateAttack(ATTACK.AIR))
            {
                //Debug.Log("Light Attack was validated!");

                _attackList.Add((int)ATTACK.AIR);
                _animator.SetInteger("AttackStep", GetAttackStep());

                //Debug.Log("Current Attack Step: " + GetAttackStep());

                _canQueueAttack = false;
                _isAttacking = true;

                _animator.SetBool("IsJumping", false);

                // Get the damage of the attack
                int extraDamage = _isSwordEquipped ? _playerSwordDamage : 0;
                transform.GetComponent<J_AttackHandler>().SetDamage(extraDamage + (int)_attacks[GetAttackStep()]);
            }
            else
                ParseAttack();
        }
        else if (_leftAttackAction.WasPressedThisFrame() && !IsCurrentAnimationEnded(GetCurrentAttack()))
        {
            if (ValidateAttack(ATTACK.LIGHT))
            {
                //Debug.Log("Light Attack was validated!");

                _attackList.Add((int)ATTACK.LIGHT);
                _animator.SetInteger("AttackStep", GetAttackStep());

                //Debug.Log("Current Attack Step: " + GetAttackStep());

                _canQueueAttack = false;
                _isAttacking = true;

                // Get the damage of the attack
                int extraDamage = _isSwordEquipped ? _playerSwordDamage : 0;
                transform.GetComponent<J_AttackHandler>().SetDamage(extraDamage + (int)_attacks[GetAttackStep()]);
            }
            else
                ParseAttack();            
        }
        // Right Attack
        else if (_rightAttackAction.WasPressedThisFrame() && !IsCurrentAnimationEnded(GetCurrentAttack()))
        {
            if (ValidateAttack(ATTACK.HEAVY))
            {
                //Debug.Log("Heavy Attack was validated!");
                _attackList.Add((int)ATTACK.HEAVY);
                _animator.SetInteger("AttackStep", GetAttackStep());

                //Debug.Log("Current Attack Step: " + GetAttackStep());

                _canQueueAttack = false;
                _isAttacking = true;

                // Get the damage of the attack
                int extraDamage = _isSwordEquipped ? _playerSwordDamage : 0;
                transform.GetComponent<J_AttackHandler>().SetDamage(extraDamage + (int)_attacks[GetAttackStep()]);
            }
            else
                ParseAttack();
        }
        // Special attack
        else if (_specialAttackAction.WasPressedThisFrame() && _playerStamina > _specialAttackStaminaCost)
        {
            if (ValidateAttack(ATTACK.SPECIAL))
            {
                //Debug.Log("Special Attack was validated!");
                _attackList.Add((int)ATTACK.SPECIAL);
                _animator.SetInteger("AttackStep", GetAttackStep());

                //Debug.Log("Current Attack Step: " + GetAttackStep());

                _canQueueAttack = false;
                _isAttacking = true;

                // Get the damage of the attack
                int extraDamage = _isSwordEquipped ? _playerSwordDamage : 0;
                transform.GetComponent<J_AttackHandler>().SetDamage(extraDamage + (int)_attacks[GetAttackStep()]);

                _playerStamina -= _specialAttackStaminaCost;
                _playerStamina = Mathf.Max(0, _playerStamina);
            }
            else
                ParseAttack();
        }
        // Parse the attack combo
        else if (IsCurrentAnimationEnded(GetCurrentAttack()))
        {
            //Debug.Log("Ran out of time, unable to queue attacks.");
            ParseAttack();
        }
    }

    int GetAttackStep()
    {
        int count = 0;
        for (int i = 0; i < _attackList.Count; ++i)
        {
            count = count * 10 + _attackList[i];
        }

        return count;
    }

    private void ParseAttack()
    {
        _isAttacking = false;
        _attackQueue.Clear();
        _attackList.Clear();
        _animator.SetInteger("AttackStep", 0);
        _animator.SetBool("IsAttacking", false);
    }

    private bool ValidateAttack(ATTACK nextAttack)
    {
        int nextAttackCount = GetAttackStep() * 10 + (int)nextAttack;
        return _attacks.ContainsKey(nextAttackCount);
    }

    private string GetCurrentAttack()
    {
        if (_attacks.TryGetValue(GetAttackStep(), out ATTACKTYPE attackType))
        {
            if (_isSwordEquipped)
            {
                if (_swordAttackAnimationNames.ContainsKey(attackType))
                {
                    return _swordAttackAnimationNames[attackType];
                }
            } 
            else
            {
                if (_meleeAttackAnimationNames.ContainsKey(attackType))
                {
                    //Debug.Log(_meleeAttackAnimationNames[attackType]);
                    return _meleeAttackAnimationNames[attackType];
                }
            }
        }

        return null;
    }

    public void AllowAttack() => _canQueueAttack = true;
    public void StopAttack() => _canQueueAttack = false;
    public void ResetAttack()
    {
        _canQueueAttack = true;
        ParseAttack();
    }


    private void CheckMovementState()
    {
        switch (_currentMoveState)
        {
            case MOVESTATE.NONE:
                _airMoveSpeed = 0f;
                break;
            case MOVESTATE.WALK:
                _airMoveSpeed = _jogAirMoveSpeed;
                break;
            case MOVESTATE.RUN:
                _airMoveSpeed = _runAirMoveSpeed;
                break;
            case MOVESTATE.CROUCH:
                _airMoveSpeed = _crouchAirMoveSpeed;
                break;
        }
    }


    private bool IsInCurrentAnimationState(string stateName)
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private bool IsCurrentAnimationReadyForNextStep(string name)
    {
        // Check if the current animation has played enough to transition
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.normalizedTime >= 0.7f && stateInfo.IsName(name); // Adjust based on when you want to allow transitions
    }

    private bool IsCurrentAnimationEnded(string name)
    {
        // Check if the current animation has played enough to transition
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.normalizedTime >= 0.95f && stateInfo.IsName(name); // Adjust based on when you want to allow transitions
    }

    private void OnAnimatorMove()
    {
        Vector3 displacement;

        if (_controller.isGrounded && !_isJumping)
        {
            displacement = _animator.deltaPosition;

            if (_isStairsDetected) 
                _velocity.y += -100f;
            else
                _velocity.y = -20f;
        }
        else
        {
            _velocity.y += _gravity * Time.deltaTime;
            Vector2 input = _inputActions["Move"].ReadValue<Vector2>();

            // Calculate move direction relative to the character's current facing
            Vector3 moveDir = (transform.forward * input.y) + (transform.right * input.x);

            // Normalize to prevent diagonal movement from being faster
            if (moveDir.magnitude > 1f) moveDir.Normalize();

            displacement = moveDir * _airMoveSpeed * Time.deltaTime;
        }

        displacement.y = _velocity.y * Time.deltaTime;

        // Move the controller
        _controller.Move(displacement);



        // Call OnMove here
        OnMove?.Invoke(transform.position); 
    }

    private IEnumerator SetCollider(float duration, Vector3 newCenter, float newHeight)
    {
        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            // height: 1.2, center: 1.29
            _controller.center = Vector3.Lerp(_controller.center, newCenter, t);
            _controller.height = Mathf.Lerp(_controller.height, newHeight, t);
            timer += Time.deltaTime;
            yield return null;
        }

        _controller.center = newCenter;
        _controller.height = newHeight;
    }

    private void HandleLook()
    {
        if (_shiftLockCamera.enabled)
        {
            Vector2 _mouseDelta = _lookAction.ReadValue<Vector2>();
            OnLook?.Invoke(_mouseDelta);
        }
        //else if (_freeLookCamera.enabled && _moveDirection.sqrMagnitude > 0.01f)
        //{
        //    float stickAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.y) * Mathf.Rad2Deg;

        //    float currentCamYaw = Camera.main.transform.eulerAngles.y;
        //    float absoluteMoveYaw = currentCamYaw + stickAngle;

        //    OnLook?.Invoke(new Vector2(absoluteMoveYaw, 0));
        //}
        else
        {
            if (_target != null)
            {
                Vector3 direction = _target.transform.position - transform.position;

                float radians = Mathf.Atan2(direction.x, direction.z);
                float degrees = radians * Mathf.Rad2Deg;
                float targetYaw = degrees;
                OnLook?.Invoke(new Vector2(targetYaw, 0));
            }
        }
    }

    private void HandleMenu()
    {
        //if (_openMenuAction.WasPerformedThisFrame())
        //    OnOpenMenu?.Invoke();
    }

    public void PlaySound(string soundName)
    {
        //AudioManager.Instance.PlayOneShot(soundName);
    }

    private void HandleStairs()
    {
        int stairsLayer = LayerMask.NameToLayer("Stairs");
        int layerMask = 1 << stairsLayer;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 1f, layerMask))
        {
            _isStairsDetected = true;
        } 
        else
        {
            _isStairsDetected = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        int stairsLayer = LayerMask.NameToLayer("Stairs");
        int layerMask = 1 << stairsLayer;

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 1f, layerMask);
        if (hitInfo.collider != null)
        {
            Gizmos.DrawSphere(hitInfo.point, 0.1f);
        }

        Gizmos.DrawRay(transform.position, Vector3.down);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up, transform.forward * _lockOnLength);


        Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo1, 1f);
        Gizmos.DrawRay(transform.position, transform.forward);
        if (hitInfo1.collider != null)
        {
            // Get the normal
            Vector3 normal = hitInfo1.normal;
            Gizmos.DrawSphere(hitInfo1.point, 0.1f);

            // Get the slope's vector
            Vector3 perpendicular = Vector3.Cross(normal, transform.forward);
            Gizmos.color = Color.purple;
            Gizmos.DrawRay(hitInfo1.point, perpendicular.normalized);

            // Get tangent
            Vector3 tangent = Vector3.Cross(perpendicular, normal);
            Gizmos.color = Color.rosyBrown;
            Gizmos.DrawRay(hitInfo1.point, tangent.normalized);

            // Get angle between tangent and forward
            float angle = Vector3.Angle(transform.up, tangent);
        }
    }
}

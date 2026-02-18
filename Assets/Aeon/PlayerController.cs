using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    [Header("Input System")]
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _rollAction;
    private InputAction _primaryAction;
    private InputAction _secondaryAction;
    private InputAction _specialAction;

    [Header("Movement")]
    [SerializeField] private float _jumpPower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private Rigidbody myRigidbody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float playerRotationSpeed;
    [SerializeField] private float _crossFadeDuration;
    [SerializeField] private float _returnToIdleDuration;
    [SerializeField] private float _jumpFallLandDurations;

    [Header("Other scripts of note")]
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Inventory inventory;
    [SerializeField] private AttackHandler attackHandler;

    private float _currentSpeed;
    private Vector3 worldMoveDirection;
    private bool _isMoving;
    private bool _isJumping;
    private string _currentAnimation;

    // Attacking
    private bool _isAttacking;
    private bool _canAttack;

    protected override void Start()
    {
        base.Start();
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _rollAction = _playerInput.actions["Roll"];
        _primaryAction = _playerInput.actions["Primary"];
        _secondaryAction = _playerInput.actions["Secondary"];
        _specialAction = _playerInput.actions["Special"];
        _isMoving = false;
        _isJumping = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Get attackhandler
        _isAttacking = attackHandler.IsAttacking();

        //// Obtain the direction the player intends to move forward towards
        // Obtain the forward and the right of the camera on a 2d plane
        Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTarget.transform.eulerAngles.y, 0);
        Vector3 cameraForward = cameraYawOnly * Vector3.forward;
        Vector3 cameraRight = cameraYawOnly * Vector3.right;

        // Decipher which way the player should move based on the direction the camera is facing
        Vector2 playerMovementDirection = _moveAction.ReadValue<Vector2>();

        // Turn player in direction while moving
        Quaternion targetRotation;
        if (playerMovementDirection != Vector2.zero && !_isAttacking)
        {
            targetRotation = Quaternion.LookRotation(cameraForward * playerMovementDirection.y + cameraRight * playerMovementDirection.x, Vector3.up);

            _isMoving = true;

            // Translate the y rotation according to the camera angle when moving
            float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
            float changeInY = newY - transform.eulerAngles.y;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + changeInY, transform.eulerAngles.z);
            cameraTarget.transform.eulerAngles = new Vector3(cameraTarget.transform.eulerAngles.x, cameraTarget.transform.eulerAngles.y - changeInY, cameraTarget.transform.eulerAngles.z);
        }

        // Grounded
        if (groundChecker.IsGrounded() && !_isJumping)
        {
            // Set animation to land if player was jumping or falling
            if (_currentAnimation == "Jump" || _currentAnimation == "Falling")
            {
                _currentAnimation = "Land";
                playerAnimator.CrossFade("Land", _jumpFallLandDurations);
            }

            // Stop moving if input is 0
            if (playerMovementDirection == Vector2.zero || _isAttacking)
            {
                if (_isMoving)
                {
                    _isMoving = false;
                    if (_currentAnimation != "Land")
                    {
                        _currentAnimation = "Idle 1";
                        playerAnimator.CrossFade("Idle 1", _returnToIdleDuration);
                    }
                }
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, _maxSpeed * Time.deltaTime / _returnToIdleDuration);
            }
            // Move if input isn't 0
            else if (!_isAttacking)
            {
                // Set animator if not already set
                if (_currentAnimation != "Run Forwards")
                {
                    _currentAnimation = "Run Forwards";
                    playerAnimator.CrossFade(_currentAnimation, _crossFadeDuration);
                }
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _maxSpeed / _crossFadeDuration * Time.deltaTime);
            }

            if (_jumpAction.WasPressedThisFrame() && !_isAttacking)
            {
                myRigidbody.AddForce(transform.up * _jumpPower, ForceMode.Impulse);
                playerAnimator.CrossFade("Jump", _returnToIdleDuration);
                _isJumping = true;
                _canAttack = false;
            }
            else
            {
                _canAttack = true;
            }
        }
        // In air
        else if (!groundChecker.IsGrounded())
        {
            // Animations
            if (myRigidbody.linearVelocity.y < 0.0f && !_isAttacking)
            {
                _currentAnimation = "Falling";
                playerAnimator.CrossFade("Falling", _jumpFallLandDurations);
                _isJumping = false;
            }

            // Stop moving if input is 0
            if (playerMovementDirection == Vector2.zero || _isAttacking)
            {
                if (_isMoving)
                {
                    _isMoving = false;
                }
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, _maxSpeed * Time.deltaTime / _returnToIdleDuration);
            }
            // Move if input isn't 0
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _maxSpeed / _crossFadeDuration * Time.deltaTime);
            }
        }

        // Set velocity
        myRigidbody.linearVelocity = new Vector3((_currentSpeed * transform.forward).x, myRigidbody.linearVelocity.y, (_currentSpeed * transform.forward).z);
    }

    public bool CanAttack()
    {
        return _canAttack;
    }
}

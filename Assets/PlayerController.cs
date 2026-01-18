using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _rollAction;

    [Header("Movement")]
    [SerializeField] private float _jumpPower;
    [SerializeField] private Rigidbody myRigidbody;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float playerRotationSpeed;
    [SerializeField] private float _crossFadeDuration;
    [SerializeField] private float _returnToIdleDuration;
    [SerializeField] private float _jumpFallLandDurations;
    [SerializeField] private float _maxSpeed;

    private float _currentSpeed;
    private Vector3 worldMoveDirection;
    private bool _isMoving;
    private bool _isJumping;
    private string _currentAnimation;

    protected override void Start()
    {
        base.Start();
        _moveAction = playerInput.actions["Move"];
        _jumpAction = playerInput.actions["Jump"];
        _rollAction = playerInput.actions["Roll"];
        _isMoving = false;
        _isJumping = false;
        _currentSpeed = 0;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //// Obtain the direction the player intends to move forward towards
        // Obtain the forward and the right of the camera on a 2D plane
        Quaternion cameraYawOnly = Quaternion.Euler(0, cameraTarget.transform.eulerAngles.y, 0);
        Vector3 cameraForward = cameraYawOnly * Vector3.forward;
        Vector3 cameraRight = cameraYawOnly * Vector3.right;

        // Decipher which way the player should move based on the direction the camera is facing
        Vector2 playerMovementDirection = _moveAction.ReadValue<Vector2>();

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
            if (playerMovementDirection == Vector2.zero)
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
            }
            // Move if input isn't 0
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward * playerMovementDirection.y + cameraRight * playerMovementDirection.x, Vector3.up);

                _isMoving = true;

                // Translate the y rotation according to the camera angle when moving
                float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
                float changeInY = newY - transform.eulerAngles.y;

                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + changeInY, transform.eulerAngles.z);
                cameraTarget.transform.eulerAngles = new Vector3(cameraTarget.transform.eulerAngles.x, cameraTarget.transform.eulerAngles.y - changeInY, cameraTarget.transform.eulerAngles.z);

                // Set animator if not already set
                if (_currentAnimation != "Run Forwards")
                {
                    _currentAnimation = "Run Forwards";
                    playerAnimator.CrossFade(_currentAnimation, _crossFadeDuration);
                }
            }

            if (_jumpAction.WasPressedThisFrame())
            {
                myRigidbody.AddForce(transform.up * _jumpPower, ForceMode.Impulse);
                playerAnimator.CrossFade("Jump", _returnToIdleDuration);
                _isJumping = true;
            }
        }
        
        // In air
        if (!groundChecker.IsGrounded())
        {
            // Animations
            if (myRigidbody.linearVelocity.y < 0.0f)
            {
                _currentAnimation = "Falling";
                playerAnimator.CrossFade("Falling", _jumpFallLandDurations);
            }

            // Stop moving if input is 0
            if (playerMovementDirection == Vector2.zero)
            {
                if (_isMoving)
                {
                    _isMoving = false;
                }
            }
            // Move if input isn't 0
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward * playerMovementDirection.y + cameraRight * playerMovementDirection.x, Vector3.up);

                _isMoving = true;

                // Translate the y rotation according to the camera angle when moving
                float newY = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, playerRotationSpeed * Time.deltaTime);
                float changeInY = newY - transform.eulerAngles.y;

                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + changeInY, transform.eulerAngles.z);
                cameraTarget.transform.eulerAngles = new Vector3(cameraTarget.transform.eulerAngles.x, cameraTarget.transform.eulerAngles.y - changeInY, cameraTarget.transform.eulerAngles.z);

                // Move in direction
            }
        }
        // Land player
        else if (groundChecker.IsGrounded() && _isJumping && myRigidbody.linearVelocity.y <= 0.0f)
        {
            _isJumping = false;
            playerAnimator.CrossFade("Landing", _jumpFallLandDurations);
        }
    }
}

using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : MonoBehaviour
{
    [Header("Player Input and Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActions;

    [Header("Movement Settings")]
    [SerializeField] private float rotateSpeed; 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMultiplier;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float moveSpeedTransition;
    [SerializeField] private float moveBackwardsOrSidewaysMultiplier;
    private CharacterController characterController;
    private Vector3 move;
    private InputAction moveAction;
    private float speed;
    private float currentSpeedMultiplier;
    private InputAction runAction;
    public bool isMoving;
    public bool isSprinting;
    public bool forcedCrouch { get; set; } = false;


    [Header("Jump Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3.0f;
    private InputAction jumpAction;
    private Vector3 jumpVelocity;
    public bool doubleJump;


    
    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        doubleJump = false;
    }

    void Start()
    {
        _inputActions = _playerInput.actions;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;


        Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        if (moveDirection.magnitude > 0)
        {
            moveDirection = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            characterController.Move(moveDirection.normalized * moveSpeed);
        }


        if (moveDirection.magnitude > 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        Jump();

        if (characterController.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = gravity / 4;
        }

        jumpVelocity.y += gravity * Time.deltaTime;

        characterController.Move(jumpVelocity);
    }

    private void LateUpdate()
    {
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            doubleJump = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && doubleJump)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            doubleJump = false;
        }
        else if (characterController.isGrounded)
        {
            doubleJump = false;
        }
    }

    public void SetPlayerToTransform(Transform anchor)
    {
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
    }
}
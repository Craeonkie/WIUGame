using UnityEngine;
using UnityEngine.InputSystem;

public class C_CameraMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Transform player;
    [SerializeField] private Transform cam;

    [Header("Walking")]
    [SerializeField] private float walkingSpeed = 5f;

    private InputActionAsset _inputActionAsset;
    private Vector2 input;
    private Vector3 moveDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        input = _inputActionAsset["Move"].ReadValue<Vector2>();

        float vertical = 0f;

        vertical = _inputActionAsset["VerticalMove"].ReadValue<float>();


        MovingPlayer(vertical);

        player.position += moveDirection * walkingSpeed * Time.deltaTime;

    }

    private void MovingPlayer(float vertical)
    {
        moveDirection = Vector3.zero;

        // camera-relative forward/right (flattened)
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        if (Mathf.Abs(input.x) > 0.1f)
        {
            moveDirection += camRight * input.x;
        }

        if (Mathf.Abs(input.y) > 0.1f)
        {
            moveDirection += camForward * input.y;
        }

        // vertical (only when not snow)
        if (Mathf.Abs(vertical) > 0.1f)
        {
            moveDirection += Vector3.up * vertical;
        }

        // optional: normalize so diagonal isn't faster
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }
}

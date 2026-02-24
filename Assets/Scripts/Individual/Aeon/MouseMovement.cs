using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMovement : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    private InputAction mouseAction;

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateUpDown;
    [SerializeField] private bool rotateLeftRight;
    [SerializeField] private float clampLimitUp;
    [SerializeField] private float clampLimitDown;
    [SerializeField] private float upDownSensitivity = 100f;
    [SerializeField] private float leftRightSensitivity = 100f;
    [SerializeField] private bool lockCursor = true;

    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        ToggleCursorLock(lockCursor);
        mouseAction = playerInput.actions["Look"];
    }

    void Update()
    {
        if (lockCursor)
        {
            yRotation = transform.localEulerAngles.y;
            xRotation = transform.localEulerAngles.x;
            if (xRotation >= 180.0f)
            {
                xRotation -= 360.0f;
            }

            if (rotateLeftRight)
            {
                float mouseX;
                mouseX = mouseAction.ReadValue<Vector2>().x * leftRightSensitivity;

                // Control rotation around y axis (Look left and right)
                yRotation += mouseX;
            }

            if (rotateUpDown)
            {
                float mouseY;
                mouseY = mouseAction.ReadValue<Vector2>().y * upDownSensitivity;

                // Control rotation around x axis (Look up and down)
                xRotation -= mouseY;

                // Clamp the rotation so we can't over-rotate
                xRotation = Mathf.Clamp(xRotation, -clampLimitDown, clampLimitUp);
            }

            //applying both rotations
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }

    void ToggleCursorLock(bool locked)
    {
        if (locked)
        {
            // Locking the cursor to the centre of the screen and making it invisible
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
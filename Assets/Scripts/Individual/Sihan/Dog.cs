using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public enum DogStates
{
    EnterIdle,
    Idle,
    ExitIdle,
    EnterChase,
    Chase,
    ExitChase,
    EnterBite,
    Bite,
    ExitBite,
    EnterClaw,
    Claw,
    ExitClaw,
    EnterDash,
    Dash,
    ExitDash,
    EnterPingPongShit,
    PingPongShit,
    ExitPingPongShit,
    EnterDead,
    Dead
}

public class Dog : MonoBehaviour
{
    [SerializeField] private DogStates currentState = DogStates.EnterIdle;
    [SerializeField] private DogStates nextState;
    private Animator animator;
    private GameObject target;
    [SerializeField] private float detectionRange;
    [SerializeField] private float walkRange;
    [SerializeField] private float stopRange;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float rotateMultiplier;
    [SerializeField] private float speed;
    private float currentSpeed;
    private float currentRotate;
    [SerializeField] private float maxRotate;
    private CharacterController dogController;
    private int[] attackTimes = new int[4] { 0, 0, 0, 0 };

    void Start()
    {
        animator = GetComponent<Animator>();
        dogController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case DogStates.EnterIdle:
                currentState = DogStates.Idle;
                break;
            case DogStates.Idle:
                Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
                GameObject closestPlayer = null;
                float closestDistance = Mathf.Infinity;

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Player"))
                    {
                        float distance = (transform.position - collider.transform.position).magnitude;

                        if (distance < closestDistance)
                        {
                            closestPlayer = collider.gameObject;
                            closestDistance = distance;
                        }
                    }
                }

                if (closestPlayer != null)
                {
                    target = closestPlayer;
                    nextState = DogStates.EnterChase;
                    currentState = DogStates.ExitIdle;
                    break;
                }

                if (currentSpeed > 0)
                {
                    currentSpeed -= Time.deltaTime;
                    currentSpeed = Mathf.Clamp(currentSpeed, 0, 1);
                }

                animator.SetFloat("Move", currentSpeed);
                dogController.Move(transform.forward * currentSpeed * speed);

                currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);
                animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                animator.SetBool("Right", false);
                animator.SetBool("Left", false);

                if (currentRotate > 5 && currentSpeed < 0.25f)
                {
                    currentSpeed = 0.25f;
                }

                break;
            case DogStates.ExitIdle:
                currentState = nextState;
                break;
            case DogStates.EnterChase:
                currentState = DogStates.Chase;
                break;
            case DogStates.Chase:
                if (target != null)
                {
                    float distance = (transform.position - target.transform.position).magnitude;

                    if (distance > detectionRange)
                    {
                        target = null;
                        nextState = DogStates.EnterIdle;
                        currentState = DogStates.ExitChase;
                        break;
                    }
                    else
                    {
                        Vector3 direction = (target.transform.position - transform.position).normalized;
                        direction.y = 0;

                        if (distance > walkRange)
                        {
                            if (currentSpeed < 1)
                            {
                                currentSpeed += Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 1);
                            }
                        }
                        else if (distance > stopRange)
                        {
                            if (currentSpeed > 0.5f)
                            {
                                currentSpeed -= Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0.5f, 1);
                            }
                            else if (currentSpeed < 0.5f)
                            {
                                currentSpeed += Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 0.5f);
                            }
                        }
                        else
                        {
                            if (currentSpeed > 0)
                            {
                                currentSpeed -= Time.deltaTime;
                                currentSpeed = Mathf.Clamp(currentSpeed, 0, 0.5f);
                            }

                            if (Mathf.Abs(currentRotate) < 5 && currentSpeed < 0.25f)
                            {
                                currentSpeed = 0.25f;
                            }
                        }

                        animator.SetFloat("Move", currentSpeed);
                        dogController.Move(transform.forward * currentSpeed * speed);


                        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                        float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);
                        currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);

                        transform.Rotate(0, currentRotate * rotateMultiplier * 90 * Time.deltaTime, 0);

                        bool isTurningRight = angle > 1.0f;
                        bool isTurningLeft = angle < -1.0f;

                        animator.SetFloat("Turn", Mathf.Abs(currentRotate));
                        animator.SetBool("Right", isTurningRight);
                        animator.SetBool("Left", isTurningLeft);
                    }
                }
                else
                {
                    nextState = DogStates.EnterIdle;
                    currentState = DogStates.ExitChase;
                }
                break;
            case DogStates.ExitChase:
                currentState = nextState;
                break;
            default:
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, walkRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopRange);
    }
}

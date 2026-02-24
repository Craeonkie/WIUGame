using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DogWaypointFollower : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float baseSpeedMultiplier = 5f;
    [SerializeField] [Range(0,1)] private float maxSpeed;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float maxRotate = 90f;
    [SerializeField] private bool loop = true;

    [Header("Components")]
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private int currentWaypointIndex = 0;
    private float currentRotate;
    private float currentSpeed;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updatePosition = false;

        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        if (waypoints.Count == 0) return;

        float distanceToTarget = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);

        if (distanceToTarget < arrivalThreshold)
        {
            UpdateNextWaypoint();
        }

        MoveAndRotate();
    }

    private void UpdateNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loop) currentWaypointIndex = 0;
            else return;
        }

        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    private void MoveAndRotate()
    {
        Vector3 desiredDir = agent.desiredVelocity.normalized;

        if (desiredDir != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            float targetRotate = Mathf.Clamp(angle / 90f, -maxRotate / 90f, maxRotate / 90f);

            Quaternion targetRotation = Quaternion.LookRotation(desiredDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            currentSpeed += Time.deltaTime * acceleration;
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
            currentRotate = Mathf.MoveTowards(currentRotate, targetRotate, Time.deltaTime * rotateSpeed);

            float rotationOffset = 90f * currentRotate;
            Vector3 adjustedForward = Quaternion.AngleAxis(rotationOffset, Vector3.up) * transform.forward;

            controller.Move(adjustedForward * currentSpeed * baseSpeedMultiplier * Time.deltaTime);
            agent.nextPosition = transform.position; 

            animator.SetFloat("Move", currentSpeed);
            animator.SetFloat("Turn", Mathf.Abs(currentRotate));
            animator.SetBool("Right", currentRotate > 0);
            animator.SetBool("Left", currentRotate < 0);
        }
        else
        {
            currentSpeed -= Time.deltaTime * acceleration;
            currentRotate = Mathf.MoveTowards(currentRotate, 0, Time.deltaTime * rotateSpeed);

            animator.SetFloat("Move", currentSpeed);
            animator.SetFloat("Turn", currentRotate);
            animator.SetBool("Right", currentRotate > 0);
            animator.SetBool("Left", currentRotate < 0);
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            else if (loop && waypoints[0] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
        }
    }
}
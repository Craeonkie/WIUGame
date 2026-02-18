using UnityEngine;
using UnityEngine.InputSystem;

public class C_Cannon : MonoBehaviour
{
    [Header("Cannon Movement")]
    [SerializeField] private C_Ball _ballPrefab;
    [SerializeField] private float _force = 20;
    [SerializeField] private Transform _ballSpawn;
    [SerializeField] private Transform _barrelPivot;
    [SerializeField] private float _rotateSpeed = 30;
    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActionAsset;

    [Header("Simulate Trajectory")]
    [SerializeField] private C_TrajectorySimulation _projection;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        _inputActionAsset = _playerInput.actions;
        _projection.SimulateTrajectory(_ballSpawn.forward * _force); //so at least the aim will be there 

    }

    // Update is called once per frame
    void Update()
    {
        bool aimingChanged = HandleControls();

        if (aimingChanged)
        {
            _projection.SimulateTrajectory(_ballSpawn.forward * _force);
        }
    }
    private bool HandleControls()
    {
        bool changed = false;

        Vector2 input = _inputActionAsset["Move"].ReadValue<Vector2>();

        if (input.y < 0)
        {
            _barrelPivot.Rotate(Vector3.right * _rotateSpeed * Time.deltaTime);
            changed = true;
        }
        else if (input.y > 0)
        {
            _barrelPivot.Rotate(Vector3.left * _rotateSpeed * Time.deltaTime);
            changed = true;
        }

        if (input.x < 0)
        {
            transform.Rotate(Vector3.down * _rotateSpeed * Time.deltaTime);
            changed = true;
        }
        else if (input.x > 0)
        {
            transform.Rotate(Vector3.up * _rotateSpeed * Time.deltaTime);
            changed = true;
        }
        if (_inputActionAsset["Jump"].WasPressedThisFrame())
        {
            var spawned = Instantiate(_ballPrefab, _ballSpawn.position, _ballSpawn.rotation);
            spawned.Init(_ballSpawn.forward * _force, false);
            Destroy(spawned, 1.5f);
        }
        return changed;
    }

}

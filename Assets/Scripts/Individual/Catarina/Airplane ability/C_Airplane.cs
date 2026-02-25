using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Pool;

public class C_Airplane : C_BossAbility
{
    public static event System.Action <Transform> FindTarget;
    public static event System.Action<float> FollowThrough;
    public static event System.Action<C_BossCameraManager.c_CameraMode> ChangeCamera;

    public static event System.Action finishAbility;


    [Header("Ref")]
    [SerializeField] private Transform[] _SpawnTransform;
    [SerializeField] private GameObject _AirplanePrefab;
    [SerializeField] private float _SearchTime = 10f;
    [SerializeField] private CinemachineCamera _airplaneCam;

    private Coroutine _AirplaneCamCoroutine;

    private PlayerController _player;

    [Header("FollowThrough")]
    [SerializeField] private float _followAirplaneCameraTime = 1.5f;
    [SerializeField] private float _FollowThroughSpeedMultiplier = 2f; // prob call this via System.action? 
    private float _CurrentSearchTimeCounter = 0f;
    C_Boid currentAirplane = null;
    private bool _followThroughTriggered = false;
    private ObjectPool<C_Boid> _AirplanePool;
    private void Awake()
    {
        C_FriendBossPhase2.StartAirplaneAbility += StartAbility;

        _player = FindFirstObjectByType<PlayerController>();

        if (_player == null)
        {
            Debug.Log("Player not in scene or more specifc PLAYER CONTROLLER SCRIPT");
        }

        _AirplanePool = new ObjectPool<C_Boid>(() =>
        {
            // when there is no obj in the pool
            var airplane = Instantiate(_AirplanePrefab, transform.position, Quaternion.identity);
            airplane.gameObject.SetActive(false);
            return airplane.GetComponent<C_Boid>();
        }, _airplane =>
        {
            // when need an obj and there is one available in the pool
            _airplane.gameObject.SetActive(true);
            currentAirplane = _airplane;
        }, _airplane =>
        {
            // when done and released back to pool
            if (_airplane != null)
            {
                _airplane.gameObject.SetActive(false);
            }
            currentAirplane = null;
        }, _airplane =>
        {
            // destroy obj
            if (_airplane != null)
                Destroy(_airplane.gameObject);
        }, false, 1, 1);
    }

    private void OnDestroy()
    {
        C_FriendBossPhase2.StartAirplaneAbility -= StartAbility;
    }

    protected override void OnEnable()
    {
        base.OnDisable();

        C_Boid.hitSmtAction += ReturnToPool;
        GameSetUp();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        C_Boid.hitSmtAction -= ReturnToPool;
        GameTearDown();
        this.startAbility = false;
    }
    protected override void GameLogic()
    {
        _CurrentSearchTimeCounter += Time.deltaTime;
        if (_CurrentSearchTimeCounter > _SearchTime)
        {
            if (currentAirplane == null) return;

            if (FollowThrough != null && !_followThroughTriggered)
            {
                if (currentAirplane == null) return;
                _followThroughTriggered = true;
                FollowThrough?.Invoke(_FollowThroughSpeedMultiplier);
            }
        }
    }

    private float switchCamOriSpeed = 0f;

    protected override void GameSetUp()
    {
        if (_SpawnTransform.Length <= 0) return;
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();

            if (_player == null)
            {
                Debug.LogWarning("Missing player");
                return;
            }
        }
        _CurrentSearchTimeCounter = 0f;

        var spawnPos = _SpawnTransform[Random.Range(0, _SpawnTransform.Length)].position;
        currentAirplane = _AirplanePool.Get(); // replaces Instantiate
        currentAirplane.transform.position = spawnPos;

        if (currentAirplane == null)
        {
            Debug.LogWarning("missing the c_boid script");
            return;
        }
        if (FindTarget != null)
        {
            FindTarget?.Invoke(_player.transform);
        }
        _followThroughTriggered = false;
        this.startAbility = true;

        _airplaneCam.Target.TrackingTarget = currentAirplane.transform;

        if (Camera.main.TryGetComponent<CinemachineBrain>(out CinemachineBrain brain))
        {
            switchCamOriSpeed = brain.DefaultBlend.Time;
            brain.DefaultBlend.Time = 0f;
        }
        ChangeCamera?.Invoke(C_BossCameraManager.c_CameraMode.AIRPLANE_CAMERA);
        if (_AirplaneCamCoroutine != null)
        {
            StopCoroutine(_AirplaneCamCoroutine);
        }
        _AirplaneCamCoroutine = StartCoroutine(FollowAirplaneCamera());
    }

    protected override void GameTearDown()
    {
        if (currentAirplane == null) return;
        _AirplanePool.Release(currentAirplane);
        finishAbility?.Invoke();
        //do an explosion here NOT HERE DO IT IN THE BOID CODE
        this.enabled = false;
    }

    private void ReturnToPool(bool _isTrue)
    {
        if (currentAirplane == null) return;
        _AirplanePool.Release(currentAirplane);
        finishAbility?.Invoke();
        //do an explosion here NOT HERE DO IT IN THE BOID CODE
        this.enabled = false;
    }

    private IEnumerator FollowAirplaneCamera()
    {
        yield return new WaitForSeconds(_followAirplaneCameraTime);
        ChangeCamera?.Invoke(C_BossCameraManager.c_CameraMode.TOP_CAMERA);
        if (Camera.main.TryGetComponent<CinemachineBrain>(out CinemachineBrain brain))
        {
            brain.DefaultBlend.Time = switchCamOriSpeed;
        }
    }
}

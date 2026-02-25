using UnityEngine;

public class C_FriendBossPhase2 : MonoBehaviour
{
    public enum ability
    {
        NONE=-1,
        AIRPLANE=0,
        PENCIL=1
    }
    [Header("Settings")]
    [SerializeField] float StartWaitTime = 3f;
    [SerializeField] float _MinTiming = 8;
    [SerializeField] float _MaxTiming =12;
    [SerializeField] private int _MaxConsecutive = 2;

    private ability _LastAbility = ability.NONE;
    private int _ConsecutiveCount = 0;
    private float _counter = 0f;


    public static event System.Action StartAirplaneAbility;


    public static event System.Action StartFallingObjAbility;
    public static event System.Action<C_BossCameraManager.c_CameraMode> ChangeCameraAnagle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //C_FriendBoss.TransitionPhase1Action += awakeSelf;
        C_Airplane.finishAbility += ResetAirplane;
        C_PencilAbility.finishAbility += ResetPencil;

    }
    private void OnDestroy()
    {
        //C_FriendBoss.TransitionPhase1Action -= awakeSelf;
        C_Airplane.finishAbility -= ResetAirplane;
        C_PencilAbility.finishAbility -= ResetPencil;
    }

    private bool firstTime = false, _isInStartWait = false;
    private void OnEnable()
    {
        ChangeCameraAnagle?.Invoke(C_BossCameraManager.c_CameraMode.TOP_CAMERA);

        if (!firstTime)
        {
            firstTime = true;
            _isInStartWait = true;  
            startingCounter = 0f;
        }
        else
        {
            // Skip the wait on subsequent enables
            _AbilityActive = false;
            _counter = Random.Range(_MinTiming, _MaxTiming);
        }
    }

    private void ResetAirplane()
    {
        _AbilityActive = false;
        _counter = Random.Range(_MinTiming, _MaxTiming);
    }

    private void ResetPencil()
    {
        _AbilityActive = false;
        _counter = Random.Range(_MinTiming, _MaxTiming);
    }

    private bool _AbilityActive = false;
    private float startingCounter = 0f;

    void Update()
    {
        if (_isInStartWait)
        {
            startingCounter += Time.deltaTime;
            if (startingCounter >= StartWaitTime)
            {
                _isInStartWait = false;
                _counter = Random.Range(_MinTiming, _MaxTiming);
            }
            return;
        }

        if (_AbilityActive) return;
        _counter -= Time.deltaTime;
        if (_counter <= 0f)
        {
            TriggerNextAbility();
        }
    }

    private void TriggerNextAbility()
    {
        _AbilityActive = true;

        ability next = PickNextAbility();
        _ConsecutiveCount = (next == _LastAbility) ? _ConsecutiveCount + 1 : 1;
        _LastAbility = next;

        if (next == ability.AIRPLANE)
        {
            Debug.Log("Airplane ability is invoke");
            StartAirplaneAbility?.Invoke();
        }
        else
        {
            Debug.Log("falling obj ability is invoke");
            StartFallingObjAbility?.Invoke();
        }
    }

    private ability PickNextAbility()
    {
        // if same ability has played max consecutive times, force the other one
        if (_ConsecutiveCount >= _MaxConsecutive)
        {
            return _LastAbility == ability.AIRPLANE ? ability.PENCIL : ability.AIRPLANE;
        }

        // otherwise pick randomly
        return (ability)Random.Range(0, 2);
    }
}

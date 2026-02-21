using UnityEngine;

public class C_FriendBossPhase2 : MonoBehaviour
{
    [Header("Airplane ability")]
    [SerializeField] private float _AirplaneTriggerTiming = 10f;
    private float _counter = 0f;
    private bool canTriggerAirplane = false;
    public static event System.Action StartAirplaneAbility;

    [Header("Falling Obj ability")]
    [SerializeField] private float _FallingObjTriggerTiming = 10f;
    private bool canTriggerFallingObj = true;
    public static event System.Action StartFallingObjAbility;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        C_FriendBoss.TransitionPhase1Action += awakeSelf;
        C_Airplane.finishAbility += ResetAirplane;
        C_PencilAbility.finishAbility += ResetPencil;

    }
    private void OnDestroy()
    {
        C_FriendBoss.TransitionPhase1Action -= awakeSelf;
        C_Airplane.finishAbility -= ResetAirplane;
        C_PencilAbility.finishAbility -= ResetPencil;
    }

    private void awakeSelf()
    {
        this.enabled = true;
    }

    private void ResetAirplane()
    {
        _counter = _AirplaneTriggerTiming;
        canTriggerAirplane = false;
    }

    private void ResetPencil()
    {
        _counter = _FallingObjTriggerTiming;
        canTriggerFallingObj = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canTriggerFallingObj)
        {
            _counter -= Time.deltaTime;
            if (_counter <= 0f)
            {
                canTriggerFallingObj = true;
            }
        }
        if (canTriggerFallingObj)
        {
            if (StartFallingObjAbility != null)
            {
                StartFallingObjAbility.Invoke();
            }
        }
        else if (canTriggerAirplane)
        {
            if (StartAirplaneAbility != null)
            {
                StartAirplaneAbility.Invoke();
            }
        }
    }
}

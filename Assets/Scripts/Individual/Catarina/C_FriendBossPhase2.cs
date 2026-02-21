using UnityEngine;

public class C_FriendBossPhase2 : MonoBehaviour
{
    [Header("Airplane ability")]
    [SerializeField] private float _AirplaneTriggerTiming = 10f;
    private float _counter = 0f;
    private bool canTriggerAirplane = true;

    public static event System.Action StartAirplaneAbility;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        C_FriendBoss.TransitionPhase1Action += awakeSelf;
        C_Airplane.finishAbility += ResetAirplane;
    }
    private void OnDestroy()
    {
        C_FriendBoss.TransitionPhase1Action -= awakeSelf;
        C_Airplane.finishAbility -= ResetAirplane;

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

    // Update is called once per frame
    void Update()
    {
        if (!canTriggerAirplane)
        {
            _counter -= Time.deltaTime;
            if (_counter <= 0f)
            {
                canTriggerAirplane = true;
            }
        }
        if (canTriggerAirplane)
        {
            if (StartAirplaneAbility != null)
            {
                StartAirplaneAbility.Invoke();
            }
        }
    }
}

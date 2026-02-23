using UnityEngine;

public class C_CupManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int _NumberOfSuccessHit;
    [SerializeField] float _CD=0.25f;
    private int CurrentHit;

    public static event System.Action _EndGame;

    bool endGameCalled = false;
    bool StartCD = false;
    float _currentTimer = 0f;
    // Update is called once per frame
    void Update()
    {
        if (StartCD)
        {
            _currentTimer += Time.deltaTime;
            if (_currentTimer > _CD)
            {
                _currentTimer = 0f;
                StartCD = false;
            }
        }
        if (CurrentHit >= _NumberOfSuccessHit && !endGameCalled)
        {
            _EndGame?.Invoke();
            endGameCalled = true;
        }
    }

    private void OnEnable()
    {
        C_Cup.hitSuccessful += AddHit;
    }

    private void OnDisable()
    {
        C_Cup.hitSuccessful -= AddHit;
    }

    private void AddHit()
    {
        if (StartCD) return;

        CurrentHit ++;
        StartCD = true;
    }
}

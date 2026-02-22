using System.Collections.Generic;
using UnityEngine;

public class C_CombatCombo : MonoBehaviour
{
    [Header("SetUp")]
    [SerializeField] Animator _Anim;
    [SerializeField] float _SecAtkCD;

    [Header("Attack")]
    [SerializeField] List<C_AttackSO> _Combo;
    [SerializeField] string _AnimAtkTag;
    [SerializeField] string _AnimAtkName;

    [Header("Combo Pause")]
    [SerializeField] float _ComboCD = 3f;

    bool _EndComboQueue = false;
    float _LastComboEnd;
    int _ComboCounter;
    float _LastAtkTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnEnable()
    {
        C_FriendAI.onAtkAction += Attack;

        C_FriendBoss.TransitionPhase1Action += Disable;
    }

    private void OnDisable()
    {
        C_FriendAI.onAtkAction -= Attack;

        C_FriendBoss.TransitionPhase1Action -= Disable;
    }

    public void Disable()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        ExitAttack();
    }

    void ExitAttack()
    {
        var state = _Anim.GetCurrentAnimatorStateInfo(0);

        if (!_EndComboQueue && state.IsTag(_AnimAtkTag) && state.normalizedTime >= 0.95f)
        {
            _EndComboQueue = true;
            Invoke(nameof(EndCombo), 0.2f);
        }
    }

    void EndCombo()
    {
        Debug.Log("Came into end combo");

        _ComboCounter = 0;
        _LastComboEnd = Time.time;
        _EndComboQueue = false;
    }

    public void Attack()
    {

        if (Time.time - _LastAtkTime > 1.5f && _ComboCounter < _Combo.Count)
        {
            CancelInvoke(nameof(EndCombo));
            _EndComboQueue = false;

            Debug.Log("Combo Counter:" + _ComboCounter + " size:" + _Combo.Count);
            _Anim.CrossFade(_Combo[_ComboCounter].clip.name, 0.25f, 0, 0);

            _ComboCounter++;
            _LastAtkTime = Time.time;
        }

        else
        {
            if (_ComboCounter >= _Combo.Count)
            {
                if (Time.time - _LastAtkTime > _ComboCD)
                {
                    _ComboCounter = 0;
                }
            }
        }
    }
}

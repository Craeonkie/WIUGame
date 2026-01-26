using UnityEngine;
using UnityEngine.InputSystem;

public class AttackHandler : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _primaryAction;
    private InputAction _secondaryAction;
    private InputAction _specialAction;

    [Header("Variables")]
    [SerializeField] private float _crossFadeDuration;

    [Header("Other scripts of note")]
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Animator _animator;

    [SerializeField] private bool _isAttacking;
    [SerializeField] private bool _chainingAttack;
    [SerializeField] private int _attackCombo;

    enum AttackType
    {
        None,
        Primary,
        Secondary,
        Special,
    }
    private AttackType attackType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _primaryAction = _playerInput.actions["Primary"];
        _secondaryAction = _playerInput.actions["Secondary"];
        _specialAction = _playerInput.actions["Special"];
    }

    void Update()
    {
        if (_primaryAction.WasPressedThisDynamicUpdate() || _secondaryAction.WasPressedThisDynamicUpdate() || _specialAction.WasPressedThisDynamicUpdate())
        {
            if (_playerController.CanAttack())
            {
                WeaponData weaponData = _inventory.ReturnWeaponData();
                if (weaponData != null)
                {
                    // Primary attack
                    if (_primaryAction.WasPressedThisDynamicUpdate())
                    {
                        if (weaponData.primaryAttack != null && weaponData.primaryAttack.Length > 0)
                        {
                            TryToAttack(AttackType.Primary);
                        }
                    }
                    
                    // Secondary attack
                    if (_secondaryAction.WasPressedThisDynamicUpdate())
                    {
                        if (weaponData.secondaryAttack != null && weaponData.secondaryAttack.Length > 0)
                        {
                            TryToAttack(AttackType.Secondary);
                        }
                    }

                    // Special attack
                    if (_specialAction.WasPressedThisDynamicUpdate())
                    {
                        if (weaponData.specialAttack != null && weaponData.specialAttack.Length > 0)
                        {
                            TryToAttack(AttackType.Special);
                        }
                    }
                }
            }
        }

        if (_isAttacking)
        {
            HandleAttackEnd();
        }
    }

    void TryToAttack(AttackType inputtedAttackType)
    {
        if (!_isAttacking)
        {
            _isAttacking = true;
            attackType = inputtedAttackType;
            PlayAttack();
        }
        else
        {
            if (attackType == inputtedAttackType)
            {
                _chainingAttack = true;
            }
        }
    }

    void HandleAttackEnd()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1.0f)
        {
            WeaponData weaponData = _inventory.ReturnWeaponData();
            bool comboFinished = true;

            // Try to attack again
            if (_chainingAttack)
            {
                _chainingAttack = false;
                if (attackType == AttackType.Primary && weaponData.primaryAttack.Length == _attackCombo)
                {
                    comboFinished = true;
                }
                else if (attackType == AttackType.Secondary && weaponData.secondaryAttack.Length == _attackCombo)
                {
                    comboFinished = true;
                }
                else if (attackType == AttackType.Special && weaponData.specialAttack.Length == _attackCombo)
                {
                    comboFinished = true;
                }
                else
                {
                    comboFinished = false;
                }
            }

            // Go back to idle
            if (comboFinished)
            {
                _attackCombo = 0;
                _isAttacking = false;
                _animator.Play("Idle 1");
            }
            else
            {
                PlayAttack();
            }
        }
    }

    void PlayAttack()
    {
        // Play animation; state name must match clip.name
        WeaponData weaponData = _inventory.ReturnWeaponData();
        if (weaponData != null)
        {
            string animationClipName;
            if (attackType == AttackType.Primary)
            {
                animationClipName = weaponData.primaryAttack[_attackCombo].animationClip.name;
            }
            else if (attackType == AttackType.Secondary)
            {
                animationClipName = weaponData.secondaryAttack[_attackCombo].animationClip.name;
            }
            else if (attackType == AttackType.Special)
            {
                animationClipName = weaponData.specialAttack[_attackCombo].animationClip.name;
            }
            else
            {
                animationClipName = "Idle 1";
            }

            _animator.Play(animationClipName);
            _attackCombo += 1;
        }
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }
}

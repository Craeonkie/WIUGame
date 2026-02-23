using UnityEngine;

public class StabWeapon : Weapon
{
    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Runs when animation handler is done acting but own isActing is still true, running only once
        if (_animationHandler != null && !_animationHandler.IsActing() && _isActing)
        {
            HandleActionEnd();
        }
    }

    // Try to perform action
    public override void TryToAct(InputType inputType, bool isBeingHeld, bool wasPressedThisFrame)
    {
        if (wasPressedThisFrame)
        {
            if (!_animationHandler.IsActing())
            {
                _inputType = inputType;
                PerformAction();
            }
            else if (!_chainingAnimation)
            {
                if (_inputType == inputType)
                {
                    _chainingAnimation = true;
                    _resetAnimationChain = false;
                }
                else
                {
                    _chainingAnimation = true;
                    _resetAnimationChain = true;
                    _inputType = inputType;
                }
            }
        }
    }

    // Runs when the current action ends
    protected override void HandleActionEnd()
    {
        EndAction();
        _isActing = false;

        // Try to attack again
        bool comboFinished = true;
        if (_chainingAnimation)
        {
            _chainingAnimation = false;
            if (_resetAnimationChain)
            {
                _resetAnimationChain = false;
                _currentAnimationChain = 0;
            }

            if (_inputType == InputType.Primary && primary.Length == _currentAnimationChain)
            {
                comboFinished = true;
            }
            else if (_inputType == InputType.Secondary && secondary.Length == _currentAnimationChain)
            {
                comboFinished = true;
            }
            else if (_inputType == InputType.Special && special.Length == _currentAnimationChain)
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
            _currentAnimationChain = 0;
            _resetAnimationChain = false;
            _animationHandler.GoBackToIdle();
        }
        else
        {
            PerformAction();
        }
    }

    // Actually performing the action
    public override void PerformAction()
    {
        switch (_inputType)
        {
            case InputType.Primary:
                {
                    if (primary.Length > 0)
                    {
                        _currentAnimation = primary[_currentAnimationChain];
                    }
                    else
                    {
                        return;
                    }
                    break;
                }
            case InputType.Secondary:
                {
                    if (secondary.Length > 0)
                    {
                        _currentAnimation = secondary[_currentAnimationChain];
                    }
                    else
                    {
                        return;
                    }
                    break;
                }
            // Special
            default:
                {
                    if (special.Length > 0)
                    {
                        _currentAnimation = special[_currentAnimationChain];
                    }
                    else
                    {
                        return;
                    }
                    break;
                }
        }

        // Set Animator
        _animationHandler.PerformAction(_currentAnimation);
        _isActing = true;
        _currentAnimationChain += 1;

        // Do own logic
        BeginAttack(_currentAnimation.damage);
    }

    // Called whenever an animation ends
    public override void EndAction()
    {
        EndAttack();
    }

    //protected override void OnTriggerEnter(Collider other)
    //{
    //    if (isAttacking && !hitEntities.Contains(other.gameObject.GetComponent<Entity>()) && !IsPartOfHierarchy(other.transform, transform.root))
    //    {
    //        if (other.gameObject.TryGetComponent<Entity>(out Entity thisEntity))
    //        {
    //            thisEntity.TakeDamage(currentAttackDamage, 0.1f);
    //        }
    //        hitEntities.Add(other.gameObject.GetComponent<Entity>());
    //    }
    //}
}
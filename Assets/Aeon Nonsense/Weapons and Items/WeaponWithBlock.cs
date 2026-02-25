using UnityEngine;

public class WeaponWithBlock : Weapon
{
    private bool isBlockingBeingHeld;

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        // Runs when animation handler is done acting but own isActing is still true, running only once
        if (!isBlockingBeingHeld && _animationHandler != null && !_animationHandler.IsActing() && _isActing)
        {
            HandleActionEnd();
        }
    }

    // Try to perform action
    public override void TryToAct(InputType inputType, bool isBeingHeld, bool wasPressedThisFrame)
    {
        if (inputType == InputType.Secondary)
        {
            if (isBlockingBeingHeld && isBeingHeld == false)
            {
                _currentAnimationChain = 0;
                _chainingAnimation = false;
                _resetAnimationChain = false;
                HandleActionEnd();
            }
            if (wasPressedThisFrame && !_isActing)
            {
                _inputType = inputType;
                _currentAnimationChain = 0;
                _chainingAnimation = false;
                _resetAnimationChain = false;
                PerformAction();
            }
            isBlockingBeingHeld = isBeingHeld;
            return;
        }

        if (isBlockingBeingHeld)
        {
            return;
        }

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
        bool canAttack = true;

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

        // Handle special attack
        if ((PlayerController)_entityUsingItem && consumesEnergy)
        {
            if (_inputType == InputType.Special)
            {
                canAttack = false;
            }
            if (((PlayerController)_entityUsingItem).UseEnergy(_currentAnimation.energyUsed, _inputType == InputType.Special))
            {
                canAttack = true;
            }
        }

        // Will be false if move requires energy and there isn't enough
        if (canAttack)
        {
            // Set Animator
            _animationHandler.PerformAction(_currentAnimation);
            _isActing = true;
            _currentAnimationChain += 1;

            // Do own logic
            if (_currentAnimation.isBlock)
            {
                BeginBlocking();
            }
            else
            {
                BeginAttack(_currentAnimation.damage);
            }
        }
        else
        {
            _currentAnimationChain = 0;
            _resetAnimationChain = false;
            _animationHandler.GoBackToIdle();
        }
    }

    // Called whenever an animation ends
    public override void EndAction()
    {
        EndAttack();
        EndBlocking();
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
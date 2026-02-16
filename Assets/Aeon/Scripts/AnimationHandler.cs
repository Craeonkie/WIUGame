using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType
{
    Primary,
    Secondary,
    Special,
}

public class AnimationHandler : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float _crossFadeDuration;

    [Header("Other scripts of note")]
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private bool _isActing;
    [SerializeField] private bool _canAct;
    [SerializeField] private bool _canMove;

    private float _currentWeight;
    private float _targetWeight;
    private float _leftArmCurrentWeight;
    private float _leftArmTargetWeight;
    private float _rightArmCurrentWeight;
    private float _rightArmTargetWeight;
    private float _bodyCurrentWeight;
    private float _bodyTargetWeight;

    private bool _holdingPrimary;
    private bool _holdingSecondary;
    private bool _holdingSpecial;

    private bool _pressedPrimary;
    private bool _pressedSecondary;
    private bool _pressedSpecial;

    private bool _animatorSkipOneFrame;
    Animation _currentAnimation;

    private void Start()
    {
        _isActing = false;
        _canMove = true;
    }

    void Update()
    {
        GameObject itemGameobject = _inventory.ReturnCurrentPrimaryItem();

        if (itemGameobject != null && _canAct)
        {
            Item item = itemGameobject.GetComponent<Item>();
            item.SetAnimationHandler(this);

            // Receive inputs
            item.TryToAct(InputType.Primary, _holdingPrimary, _pressedPrimary);
            item.TryToAct(InputType.Secondary, _holdingSecondary, _pressedSecondary);
            item.TryToAct(InputType.Special, _holdingSpecial, _pressedSpecial);

            _pressedPrimary = false;
            _pressedSecondary = false;
            _pressedSpecial = false;
        }
        else
        {
            GoBackToIdle();
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(1);

        if (_animatorSkipOneFrame)
        {
            _animatorSkipOneFrame = false;
        }
        else if (!_currentAnimation.pressAndHold && stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(1) && _isActing)
        {
            _isActing = false;
        }

        _currentWeight = Mathf.MoveTowards(_currentWeight, _targetWeight, Time.deltaTime * 10);
        _leftArmCurrentWeight = Mathf.MoveTowards(_leftArmCurrentWeight, _leftArmTargetWeight, Time.deltaTime * 10);
        _rightArmCurrentWeight = Mathf.MoveTowards(_rightArmCurrentWeight, _rightArmTargetWeight, Time.deltaTime * 10);
        _bodyCurrentWeight = Mathf.MoveTowards(_bodyCurrentWeight, _bodyTargetWeight, Time.deltaTime * 10);

        // Base model
        _animator.SetLayerWeight(1, _currentWeight);
        // Left Arm
        _animator.SetLayerWeight(3, _leftArmCurrentWeight);
        // Right Arm
        _animator.SetLayerWeight(4, _rightArmCurrentWeight);
        // Body
        _animator.SetLayerWeight(5, _bodyCurrentWeight);
    }
    
    public void PerformAction(Animation currentAnimation)
    {
        _currentAnimation = currentAnimation;
        _isActing = true;

        string animationClipName = currentAnimation.animationClip.name;
        _canMove = currentAnimation.canMoveWhileAnimating;
        _animator.applyRootMotion = currentAnimation.hasRootMotion;

        _animatorSkipOneFrame = true;
        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 1);
        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 3);
        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 4);
        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 5);

        if (currentAnimation.movesLeftArmWhileRunning)
        {
            _leftArmTargetWeight = 1;
            _leftArmCurrentWeight = 1;
        }
        else
        {
            _leftArmTargetWeight = 0;
        }
        if (currentAnimation.movesRightArmWhileRunning)
        {
            _rightArmTargetWeight = 1;
            _rightArmCurrentWeight = 1;
        }
        else
        {
            _rightArmTargetWeight = 0;
        }
        if (currentAnimation.movesBodyWhileRunning)
        {
            _bodyTargetWeight = 1;
            _bodyCurrentWeight = 1;
        }
        else
        {
            _bodyTargetWeight = 0;
        }

        _targetWeight = 1;
        _currentWeight = 1;

        if (currentAnimation.audioClip != null)
        {
            _audioSource.PlayOneShot(currentAnimation.audioClip);
        }
    }

    // Returns the animator state to idle
    public void GoBackToIdle()
    {
        _isActing = false;
        _canMove = true;
        _targetWeight = 0;
        _leftArmTargetWeight = 0;
        _rightArmTargetWeight = 0;
        _bodyTargetWeight = 0;
        _animatorSkipOneFrame = true;
        _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 1);
        _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 3);
        _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 4);
        _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 5);
    }

    public bool CanMove()
    {
        return _canMove;
    }

    public bool IsActing()
    {
        return _isActing;
    }

    public void ToggleAbilityToAct(bool canAct)
    {
        _canAct = canAct;
    }

    public void TryingToUsePrimary(bool input)
    {
        _holdingPrimary = input;
        _pressedPrimary = input;
    }

    public void TryingToUseSecondary(bool input)
    {
        _holdingSecondary = input;
        _pressedSecondary = input;
    }

    public void TryingToUseSpecial(bool input)
    {
        _holdingSpecial = input;
        _pressedSpecial = input;
    }

    public Animator ReturnAnimator()
    {
        return _animator;
    }
}

[System.Serializable]
public struct Animation
{
    [Header("Animation Information")]
    public float damage;
    public bool hasRootMotion;
    public bool pressAndHold;

    [Header("Body movements while running")]
    public bool canMoveWhileAnimating;
    public bool movesLeftArmWhileRunning;
    public bool movesRightArmWhileRunning;
    public bool movesBodyWhileRunning;

    [Header("Animation")]
    public AnimationClip animationClip;

    [Header("Accompanying Audio(If any)")]
    public AudioClip audioClip;
}
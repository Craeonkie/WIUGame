using System;
using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Item _currentItem;

    // If the player is currently acting
    [SerializeField] private bool _isActing;
    // If the player may walk and such
    [SerializeField] private bool _canMove;
    // To handle returning to idle when the player is suddenly no longer holding an item
    [SerializeField] private bool _wasHoldingItem;
    [SerializeField] private bool _letAnimationFinish;

    private bool _holdingPrimary;
    private bool _holdingSecondary;
    private bool _holdingSpecial;

    private bool _pressedPrimary;
    private bool _pressedSecondary;
    private bool _pressedSpecial;

    Animation _currentAnimation;

    private void Start()
    {
        _isActing = false;
        _canMove = true;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        // Receive inputs if the current item isn't null
        if (_currentItem != null)
        {
            // Receive inputs
            _currentItem.TryToAct(InputType.Primary, _holdingPrimary, _pressedPrimary);
            _currentItem.TryToAct(InputType.Secondary, _holdingSecondary, _pressedSecondary);
            _currentItem.TryToAct(InputType.Special, _holdingSpecial, _pressedSpecial);

            _pressedPrimary = false;
            _pressedSecondary = false;
            _pressedSpecial = false;

            _wasHoldingItem = true;

            if (_isActing)
            {
                Quaternion targetRotation = Quaternion.Euler(_currentAnimation.rotation);
                _currentItem.transform.localRotation = Quaternion.RotateTowards(
                    _currentItem.transform.localRotation,
                    targetRotation,
                    _currentAnimation.rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                _currentItem.transform.localRotation = Quaternion.RotateTowards(
                    _currentItem.transform.localRotation,
                    Quaternion.identity,
                    _currentAnimation.rotationSpeed * Time.deltaTime
                    );
            }
        }
        // If the player was holding an item the previous frame but there isn't one anymore, return to idle
        else if (_wasHoldingItem && !_letAnimationFinish)
        {
            _wasHoldingItem = false;
            _currentItem = null;
            GoBackToIdle();
        }
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (!_currentAnimation.pressAndHold && stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0) && _isActing)
        {
            if (_letAnimationFinish)
            {
                _letAnimationFinish = false;
                _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 0);
            }
            if (_isActing)
            {
                GoBackToIdle();
            }
            _canMove = true;
        }
    }

    public void PerformAction(Animation currentAnimation)
    {
        _currentAnimation = currentAnimation;
        _isActing = true;

        string animationClipName = currentAnimation.animationClip.name;
        _canMove = false;
        _animator.applyRootMotion = currentAnimation.hasRootMotion;
        _myRigidbody.isKinematic = true;

        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 0);

        if (currentAnimation.audioClip != null)
        {
            _audioSource.PlayOneShot(currentAnimation.audioClip);
        }
    }

    // Returns the animator state to idle
    public void GoBackToIdle()
    {
        if (_isActing)
        {
            _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 0);
        }
        _isActing = false;
        _canMove = true;
        _animator.applyRootMotion = false;
        _myRigidbody.isKinematic = false;
    }

    public bool CanMove()
    {
        return _canMove;
    }

    // Call if an animation is playing
    public bool IsActing()
    {
        return _isActing;
    }

    // Called when the player presses and releases the primary button
    public void TryingToUsePrimary(bool input)
    {
        _holdingPrimary = input;
        _pressedPrimary = input;
    }

    // Called when the player presses and releases the secondary button
    public void TryingToUseSecondary(bool input)
    {
        _holdingSecondary = input;
        _pressedSecondary = input;
    }

    // Called when the player presses and releases the special button
    public void TryingToUseSpecial(bool input)
    {
        _holdingSpecial = input;
        _pressedSpecial = input;
    }

    // Equip the referenced item
    public void ReferenceItem(Item item)
    {
        StopReferencingOldItem();
        _currentItem = item;
        _currentItem.transform.localEulerAngles = Vector3.zero;
        _currentItem.SetAnimationHandler(this);
        GoBackToIdle();
    }

    // Update animation handler to stop referencing current item
    public void StopReferencingOldItem()
    {
        if (_currentItem != null)
        {
            _currentItem.transform.localEulerAngles = Vector3.zero;
            _currentItem = null;
            GoBackToIdle();
        }
    }

    // Update animation handler to stop referencing current item but let the animation it had finish (throwing)
    public void StopReferencingItemButFinishAnimation()
    {
        if (_currentItem != null)
        {
            _currentItem.transform.localEulerAngles = Vector3.zero;
            _currentItem = null;
            _letAnimationFinish = true;
        }
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
    public int durabilityUsed;
    public int energyUsed;
    public bool hasRootMotion;
    public bool pressAndHold;
    public bool isBlock;
    public Vector3 rotation;
    public float rotationSpeed;

    [Header("Animation")]
    public AnimationClip animationClip;

    [Header("Accompanying Audio(If any)")]
    public AudioClip audioClip;
}
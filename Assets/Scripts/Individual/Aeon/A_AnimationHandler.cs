using UnityEngine;

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
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Item _currentItem;

    [SerializeField] private bool _isActing;
    [SerializeField] private bool _canMove;

    private float _currentWeight;
    private float _targetWeight;

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
        if (_currentItem != null)
        {
            // Receive inputs
            _currentItem.TryToAct(InputType.Primary, _holdingPrimary, _pressedPrimary);
            _currentItem.TryToAct(InputType.Secondary, _holdingSecondary, _pressedSecondary);
            _currentItem.TryToAct(InputType.Special, _holdingSpecial, _pressedSpecial);

            _pressedPrimary = false;
            _pressedSecondary = false;
            _pressedSpecial = false;
        }
        else
        {
            GoBackToIdle();
        }

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (_animatorSkipOneFrame)
        {
            _animatorSkipOneFrame = false;
        }
        else if (!_currentAnimation.pressAndHold && stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0) && _isActing)
        {
            _isActing = false;
        }

        _currentWeight = Mathf.MoveTowards(_currentWeight, _targetWeight, Time.deltaTime * 10);

        // Base model
        _animator.SetLayerWeight(0, _currentWeight);
    }
    
    public void PerformAction(Animation currentAnimation)
    {
        _currentAnimation = currentAnimation;
        _isActing = true;

        string animationClipName = currentAnimation.animationClip.name;
        //_canMove = currentAnimation.canMoveWhileAnimating;
        _canMove = false;
        _animator.applyRootMotion = currentAnimation.hasRootMotion;

        _animatorSkipOneFrame = true;
        _animator.CrossFadeInFixedTime(animationClipName, _crossFadeDuration, 1);

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
        _animatorSkipOneFrame = true;
        _animator.CrossFadeInFixedTime("Idle", _crossFadeDuration, 0);
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

    // Update animation handler to use items from here
    public void SetItem(Item item)
    {
        _currentItem = item;
        _currentItem.SetAnimationHandler(this);
        GoBackToIdle();
    }
}

[System.Serializable]
public struct Animation
{
    [Header("Animation Information")]
    public float damage;
    public bool hasRootMotion;
    public bool pressAndHold;

    //[Header("Body movements while running")]
    //public bool canMoveWhileAnimating;
    //public bool movesLeftArmWhileRunning;
    //public bool movesRightArmWhileRunning;
    //public bool movesBodyWhileRunning;

    [Header("Animation")]
    public AnimationClip animationClip;

    [Header("Accompanying Audio(If any)")]
    public AudioClip audioClip;
}
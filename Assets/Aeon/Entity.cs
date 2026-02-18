using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] protected float _maxHP;
    [SerializeField] protected float _currentHP;
    [SerializeField] protected float _invincibilityCooldown;
    [SerializeField] protected float _invincibilityFlickerGap;
    [SerializeField] protected float _invincibilityFlickerCurrentTimer;
    [SerializeField] protected GameObject _model;
    [SerializeField] protected SkinnedMeshRenderer[] renderers;
    [SerializeField] protected Vector3 spawnPoint;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected bool isInvincible = false;
    [SerializeField] protected bool isDodging = false;
    [SerializeField] protected bool _animationHasReset = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _currentHP = _maxHP;
        renderers = _model.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_invincibilityCooldown > 0)
        {
            // Handle invincibility flickering
            _invincibilityFlickerCurrentTimer -= Time.deltaTime;

            if (_invincibilityFlickerCurrentTimer <= 0)
            {
                _invincibilityFlickerCurrentTimer += _invincibilityFlickerGap;
                foreach (var renderer in renderers)
                {
                    renderer.enabled = !renderer.enabled;
                }
            }

            // Handle invincibility end
            isInvincible = true;
            _invincibilityCooldown -= Time.deltaTime;

            if (_invincibilityCooldown <= 0 )
            {
                isInvincible = false;
                _invincibilityCooldown = 0;
                foreach(var renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

    public virtual void Respawn()
    {
        _currentHP = _maxHP;
        transform.position = spawnPoint;
    }

    // Do damage without invincibility cooldown
    public virtual void TakeDamage(float damageTaken)
    {
        if (!isDodging)
        {
            _currentHP -= damageTaken;
            if (_currentHP <= 0)
            {
                Die();
            }
        }
    }

    // Do damage with invincibility cooldown
    public virtual void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (!isInvincible && !isDodging)
        {
            _currentHP -= damageTaken;
            _invincibilityCooldown += invincibilityLength;
            if (_currentHP <= 0)
            {
                Die();
            }
            else
            {
                if (_invincibilityCooldown > 0)
                {
                    isInvincible = true;
                }
            }
        }
    }

    // Set gameobject to be inactive
    public virtual void Die()
    {
        gameObject.SetActive(false);
    }

    // Check if current animation is over
    public virtual bool CurrentAnimationOver(int state)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(state);

        if (stateInfo.normalizedTime < 1.0f)
        {
            _animationHasReset = true;
        }

        return (stateInfo.normalizedTime >= 1.0f) && _animationHasReset;
    }
}

using UnityEngine;

// Class to be inherited by all entities, alive or not.
public abstract class Entity : MonoBehaviour
{
    [SerializeField] private EntityData _myEntityData;
    protected float _maxHP;
    protected float _currentHP;
    private float _invincibilityCooldown;
    private Vector3 spawnPoint;

    // Can be made into getters and setters
    public bool isInvincible = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        // Pull from EntityData
        _maxHP = _myEntityData._maxHP;
        spawnPoint = _myEntityData._spawnPoint;

        // Initialise
        // Do an if statement to check if should run PullFromData or default to these values
        _currentHP = _maxHP;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_invincibilityCooldown > 0)
        {
            isInvincible = true;
            _invincibilityCooldown -= Time.deltaTime;

            if (_invincibilityCooldown <= 0 )
            {
                isInvincible = false;
                _invincibilityCooldown = 0;
            }
        }
    }

    public void Respawn()
    {
        _currentHP = _maxHP;
        transform.position = spawnPoint;
    }

    // Do damage without invincibility cooldown
    public void TakeDamage(float damageTaken)
    {
        if (!isInvincible)
        {
            _currentHP -= damageTaken;
            if (_currentHP <= 0)
            {
                Die();
            }
        }
    }

    // Do damage with invincibility cooldown
    public void TakeDamage(float damageTaken, float invincibilityLength)
    {
        if (!isInvincible)
        {
            _currentHP -= damageTaken;
            _invincibilityCooldown += invincibilityLength;
            if (_currentHP <= 0)
            {
                Die();
            }
        }
    }
    
    // Pulls information from an outside area if required
    public void PullFromData()
    {

    }

    // Set gameobject to be inactive or die, depends on what we want
    public void Die()
    {

    }
}

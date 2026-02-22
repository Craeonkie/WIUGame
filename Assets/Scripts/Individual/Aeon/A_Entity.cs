using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] protected float _maxHP;
    [SerializeField] protected float _currentHP;
    [SerializeField] protected float _invincibilityCooldown;
    [SerializeField] protected float _invincibilityMaxCooldown;
    [SerializeField] protected GameObject _model;
    //[SerializeField] protected SkinnedMeshRenderer[] renderers;
    //[SerializeField] protected Material damageMaterial;
    [SerializeField] protected Vector3 spawnPoint;
    [SerializeField] protected bool isInvincible = false;
    [SerializeField] protected bool isDodging = false;
    [SerializeField] protected bool _animationHasReset = false;
    [SerializeField] protected bool _hasDamageFlicker = false;
    [SerializeField] protected AudioClip[] hitAudio;
    [SerializeField] protected AudioClip deathAudio;
    [SerializeField] protected AudioSource audioSource;
    public static System.Action OnDie;
    private MaterialPropertyBlock _propertyBlock;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _currentHP = _maxHP;

        //// Add all materials to the entity
        //if (_hasDamageFlicker)
        //{
        //    _propertyBlock = new MaterialPropertyBlock();
        //    renderers = _model.GetComponentsInChildren<SkinnedMeshRenderer>();
        //    foreach (SkinnedMeshRenderer renderer in renderers)
        //    {
        //        Material[] temp = { damageMaterial };
        //        Material[] newMaterialsList = renderer.materials.Concat(temp).ToArray();
        //        renderer.materials = newMaterialsList;
        //    }
        //}
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Handle invincibility fade from red
        if (_invincibilityCooldown > 0)
        {
            // Handle invincibility end
            isInvincible = true;
            _invincibilityCooldown -= Time.deltaTime;

            //// Apply to all renderers
            //if (_hasDamageFlicker)
            //{
            //    float temp = Mathf.Max(_invincibilityCooldown / _invincibilityMaxCooldown, 0.0f);

            //    foreach (Renderer renderer in renderers)
            //    {
            //        renderer.GetPropertyBlock(_propertyBlock);
            //        _propertyBlock.SetFloat("_Visibility", temp);
            //        renderer.SetPropertyBlock(_propertyBlock);
            //    }
            //}

            if (_invincibilityCooldown <= 0)
            {
                isInvincible = false;
                _invincibilityCooldown = 0;
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
            if (hitAudio.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(hitAudio[Random.Range(0, hitAudio.Length - 1)]);
            }
            if (_currentHP <= 0)
            {
                if (audioSource != null && deathAudio != null)
                {
                    audioSource.PlayOneShot(deathAudio);
                }
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
            _invincibilityMaxCooldown = invincibilityLength;
            _invincibilityCooldown = invincibilityLength;
            if (hitAudio.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(hitAudio[Random.Range(0, hitAudio.Length - 1)]);
            }
            if (_currentHP <= 0)
            {
                if (audioSource != null && deathAudio != null)
                {
                    audioSource.PlayOneShot(deathAudio);
                }
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
        OnDie?.Invoke();
        gameObject.SetActive(false);
    }
}
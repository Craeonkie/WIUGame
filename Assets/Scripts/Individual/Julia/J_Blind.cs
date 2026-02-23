using System.Collections;
using UnityEngine;

public class J_Blind : MonoBehaviour
{
    [SerializeField] private float _durationBeforeInactive;
    
    [SerializeField] private float _damage;
    [SerializeField] private float _stunDuration;

    private void OnEnable()
    {
        StartCoroutine(DisableAfterDuration());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerTag"))
        {
            // TODO: Stun player and slow player
            Debug.Log("Blinded player!");
            other.GetComponent<Entity>().TakeDamage(_damage, 0.0f);
            other.GetComponent<PlayerController>().Stun(_stunDuration);

            J_EffectsManager.Instance.StartDustEffect();
        }
    }

    private IEnumerator DisableAfterDuration()
    {
        yield return new WaitForSeconds(_durationBeforeInactive);
        J_SpawnManager.Instance.Release("CottonBall", gameObject);
    }
}

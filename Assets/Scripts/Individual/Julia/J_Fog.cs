using UnityEngine;
using UnityEngine.Events;

public class J_Fog : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    public UnityEvent OnFogEnter, OnSpawn;

    private void OnTriggerEnter(Collider other)
    {
        // Respawn player
        if (other.CompareTag("PlayerTag"))
        {
            OnFogEnter?.Invoke();
        }
    }

    public void Spawn()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        player.transform.position = _spawnPoint.position;
        OnSpawn?.Invoke();
    }
}

using UnityEngine;

public abstract class Item : MonoBehaviour
{
    // This should be something that goes into an item manager instead...
    // Spawns object in regardless of whether or not it is already in the scene
    public void Spawn(Vector3 newPosition)
    {
        gameObject.SetActive(true);
        transform.position = newPosition;
    }

    // Respawns object if it's not in the scene
    public void SpawnIfNotInScene(Vector3 newPosition)
    {
        if (!gameObject.activeSelf)
        {
            Spawn(newPosition);
        }
    }
}

using UnityEngine;

public class C_Throwable : MonoBehaviour
{
    private GameObject _prefabKey;
    private C_ThrowableSpawner _spawner;

    public static event System.Action pickUpAnItem;

    public void Init(GameObject prefabKey, C_ThrowableSpawner spawner)
    {
        _prefabKey = prefabKey;
        _spawner = spawner;
    }

    public void PickUp()
    {
        pickUpAnItem?.Invoke();
        _spawner.ReturnToPool(_prefabKey, this.gameObject);
    }
}

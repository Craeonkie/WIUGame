using UnityEngine;
using System.Collections.Generic;

public class C_WeaponSpawner : MonoBehaviour
{
    [System.Serializable]
    public class C_Obj
    {
        public GameObject weaponPrefab;
        public int minAmount;
        public int maxAmount;
    }

    [SerializeField] private LayerMask _ObjLayer;
    [SerializeField] private LayerMask _AvoidLayer;
    [SerializeField] private Collider _SpawnArea;
    [SerializeField] private int _DistBetweenObj;
    [SerializeField] private C_Obj[] _Weapons;
    [SerializeField] private int _MaxNumOfAttempts;

    private Bounds _Bound;
    private Dictionary<GameObject, Queue<GameObject>> _pools = new();

    void Start()
    {
        _Bound = _SpawnArea.bounds;

        foreach (var weapon in _Weapons)
            _pools[weapon.weaponPrefab] = new Queue<GameObject>();

        SpawnWeapon();
    }

    void SpawnWeapon()
    {
        if (_Bound == null) return;
        for (int i = 0; i < _Weapons.Length; i++)
        {
            int spawnAmount = Random.Range(_Weapons[i].minAmount, _Weapons[i].maxAmount + 1);
            for (int j = 0; j < spawnAmount; j++)
            {
                int attempts = 0;
                bool positionFound = false;
                while (!positionFound && attempts < _MaxNumOfAttempts)
                {
                    attempts++;
                    Vector3 randomPos = new Vector3(
                        Random.Range(_Bound.min.x, _Bound.max.x),
                        _Bound.center.y,
                        Random.Range(_Bound.min.z, _Bound.max.z)
                    );
                    // make sure its not on obj that dont want to be spawn
                    Collider[] hit = Physics.OverlapSphere(randomPos, _DistBetweenObj, _AvoidLayer);
                    if (hit.Length > 0) continue;
                    // make sure no nearby obj
                    Collider[] nearby = Physics.OverlapSphere(randomPos, _DistBetweenObj, _ObjLayer);
                    if (nearby.Length > 0) continue;

                    GameObject obj = GetFromPool(_Weapons[i].weaponPrefab);
                    obj.transform.position = randomPos;
                    obj.transform.rotation = Quaternion.identity;
                    positionFound = true;
                }
            }
        }
    }

    public void OnWeaponPickedUp(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        _pools[prefab].Enqueue(obj);

        // find the matching C_Obj and respawn one
        foreach (var weapon in _Weapons)
        {
            if (weapon.weaponPrefab == prefab)
            {
                RespawnOne(weapon);
                break;
            }
        }
    }

    private void RespawnOne(C_Obj weapon)
    {
        int attempts = 0;
        while (attempts < _MaxNumOfAttempts)
        {
            attempts++;
            Vector3 randomPos = new Vector3(
                Random.Range(_Bound.min.x, _Bound.max.x),
                _Bound.center.y,
                Random.Range(_Bound.min.z, _Bound.max.z)
            );

            Collider[] hit = Physics.OverlapSphere(randomPos, _DistBetweenObj, _AvoidLayer);
            if (hit.Length > 0) continue;
            Collider[] nearby = Physics.OverlapSphere(randomPos, _DistBetweenObj, _ObjLayer);
            if (nearby.Length > 0) continue;

            GameObject obj = GetFromPool(weapon.weaponPrefab);
            obj.transform.position = randomPos;
            obj.transform.rotation = Quaternion.identity;
            return;
        }
    }

    private void OnEnable()
    {
        //??? +=RespawnOne;
    }

    private void OnDisable()
    {
        //???? -= RespawnOne;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (_pools[prefab].Count > 0)
        {
            var obj = _pools[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }
}
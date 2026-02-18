using UnityEngine;

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
    void Start()
    {
        _Bound = _SpawnArea.bounds;
        SpawnWeapon();
    }

    void Update()
    {
        
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

                    //make sure its not on obj that dont want to be spawn
                    Collider[] hit = Physics.OverlapSphere(randomPos, _DistBetweenObj, _AvoidLayer);

                    if (hit.Length > 0) continue;


                    //make sure no nearby obj
                    Collider[] nearby = Physics.OverlapSphere(randomPos, _DistBetweenObj,_ObjLayer);

                    if (nearby.Length > 0) continue;

                   
                    Instantiate(_Weapons[i].weaponPrefab, randomPos, Quaternion.identity);
                    positionFound = true;
                }
            }
        }
    }
}

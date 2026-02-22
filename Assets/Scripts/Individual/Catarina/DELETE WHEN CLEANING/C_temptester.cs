using UnityEngine;

public class C_temptester : MonoBehaviour
{

    [SerializeField] private float movespeed;
    [SerializeField] private Transform movingobj;
    [SerializeField] private Collider moveBound;
    private Bounds _bound;
    private Vector3 _targetPos;

    void Start()
    {
        _bound = moveBound.bounds;
        _targetPos = GetRandomPosInBounds();
    }

    void Update()
    {
        movingobj.position = Vector3.MoveTowards(movingobj.position, _targetPos, movespeed * Time.deltaTime);

        if (Vector3.Distance(movingobj.position, _targetPos) < 0.1f)
        {
            _targetPos = GetRandomPosInBounds();
        }
    }

    private Vector3 GetRandomPosInBounds()
    {
        return new Vector3(
            Random.Range(_bound.min.x, _bound.max.x),
            Random.Range(_bound.min.y, _bound.max.y),
            Random.Range(_bound.min.z, _bound.max.z)
        );
    }
}
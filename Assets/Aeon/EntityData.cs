using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    public float _maxHP = 100.0f;
    public Vector3 _spawnPoint;
}

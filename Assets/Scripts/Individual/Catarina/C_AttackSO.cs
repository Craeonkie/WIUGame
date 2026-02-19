using UnityEngine;
[CreateAssetMenu(menuName = "Attack/Normal Attack")]

public class C_AttackSO : ScriptableObject
{
    public AnimatorOverrideController animatorOController;
    public float damage;
}

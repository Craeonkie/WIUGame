using UnityEngine;

public class C_CutSceneManager : MonoBehaviour
{
    public enum C_CutSceneType
    {
        START_SCENE,
        START_SCENE_CONTINUING,
        PHASE_1_TRANSITION,
        AIRPLANE_FIRST_TIME,
        PENCIL_FIRST_TIME,
        ENDING
    }

    [SerializeField]private C_CutSceneType _CutSceneType;
    public static event System.Action startSceneActionAction;
    public static event System.Action startSceneContinuingAction;
    public static event System.Action phase1TransitionAction;
    public static event System.Action airplaneFirstTimeAction;
    public static event System.Action pencilFirstTimeAction;
    public static event System.Action endingAction;

    //public void Changin
}

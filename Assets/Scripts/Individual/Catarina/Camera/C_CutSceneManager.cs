using UnityEngine;
using UnityEngine.Playables;

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

    [SerializeField] private C_CutSceneType _CutSceneType;
    public static event System.Action startSceneActionAction;
    public static event System.Action startSceneContinuingAction;
    public static event System.Action phase1TransitionAction;
    public static event System.Action airplaneFirstTimeAction;
    public static event System.Action pencilFirstTimeAction;
    public static event System.Action endingAction;

    [SerializeField] private PlayableDirector startSceneTimeline;
    [SerializeField] private PlayableDirector startSceneContinuingTimeline;
    [SerializeField] private PlayableDirector phase1TransitionTimeline;
    [SerializeField] private PlayableDirector airplaneTimeline;
    [SerializeField] private PlayableDirector pencilTimeline;
    [SerializeField] private PlayableDirector endingTimeline;



    public void PlayCutScene(C_CutSceneType _cutSceneType)
    {
        switch (_cutSceneType)
        {
            case C_CutSceneType.START_SCENE:
                if (startSceneTimeline == null) break;
                startSceneActionAction?.Invoke();
                startSceneTimeline.enabled = true;
                break;
            case C_CutSceneType.START_SCENE_CONTINUING:
                if (startSceneContinuingTimeline == null) break;
                startSceneContinuingAction?.Invoke();
                startSceneContinuingTimeline.enabled = true;
                break;
            case C_CutSceneType.PHASE_1_TRANSITION:
                if (phase1TransitionTimeline == null) break;
                phase1TransitionAction?.Invoke();
                phase1TransitionTimeline.enabled = true;
                break;
            case C_CutSceneType.AIRPLANE_FIRST_TIME:
                if (airplaneTimeline == null) break;
                airplaneFirstTimeAction?.Invoke();
                airplaneTimeline.enabled = true;
                break;
            case C_CutSceneType.PENCIL_FIRST_TIME:
                if (pencilTimeline == null) break;
                pencilFirstTimeAction?.Invoke();

                pencilTimeline.enabled = true;
                break;
            case C_CutSceneType.ENDING:
                if (endingTimeline == null) break;
                endingAction?.Invoke();
                endingTimeline.enabled = true;
                break;

        }
    }

    public void FinishCutScene()
    {
        if (startSceneTimeline != null)
            startSceneTimeline.enabled = false;

        if (startSceneContinuingTimeline != null)
            startSceneContinuingTimeline.enabled = false;

        if (phase1TransitionTimeline != null)
            phase1TransitionTimeline.enabled = false;

        if (airplaneTimeline != null)
            airplaneTimeline.enabled = false;

        if (pencilTimeline != null)
            pencilTimeline.enabled = false;

        if (endingTimeline != null)
            endingTimeline.enabled = false;
    }
}

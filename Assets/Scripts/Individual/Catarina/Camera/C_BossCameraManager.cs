using Unity.Cinemachine;
using UnityEngine;

public class C_BossCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _TopCamera;
    [SerializeField] private CinemachineCamera _catapultCamera;
    [SerializeField] private CinemachineCamera _cutSceneCamera;
    [SerializeField] private CinemachineCamera _AirplaneCamera;

    public static event System.Action onPlayerCameraActivity;
    public static event System.Action offPlayerCameraActivity;

    public enum c_CameraMode
    {
        PLAYER_CAMERA,
        TOP_CAMERA,
        CATAPULT_CAMERA,
        CUT_SCEEN,
        AIRPLANE_CAMERA
    }

    public c_CameraMode _CurCam;

    public void SwitchCamera(c_CameraMode mode)
    {
        ResetAll();
        switch (mode)
        {
            case c_CameraMode.PLAYER_CAMERA:
                onPlayerCameraActivity?.Invoke();
                break;
            case c_CameraMode.TOP_CAMERA:
                if (_TopCamera == null) return;
                offPlayerCameraActivity?.Invoke();
                _TopCamera.Priority = 60;
                break;
            case c_CameraMode.CATAPULT_CAMERA:
                if (_catapultCamera == null) return;
                offPlayerCameraActivity?.Invoke();
                _catapultCamera.Priority = 60;
                break;
            case c_CameraMode.CUT_SCEEN:
                if (_cutSceneCamera == null) return;
                _cutSceneCamera.Priority = 60;
                offPlayerCameraActivity?.Invoke();
                break;
            case c_CameraMode.AIRPLANE_CAMERA:
                if (_AirplaneCamera == null) return;
                _AirplaneCamera.Priority = 60;
                offPlayerCameraActivity?.Invoke();
                break;
        }
        _CurCam = mode;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void ResetAll()
    {
        _catapultCamera.Priority = 10;
        _TopCamera.Priority = 10;
        _cutSceneCamera.Priority = 10;
        _AirplaneCamera.Priority = 10;
    }

    public void OnEnable()
    {
        //playercamera +=SwitchCamera;
        C_Catapult.ExitCatapultMode += SwitchCamera;
        C_Catapult.EnterCatapultMode += SwitchCamera;
        C_FriendBossPhase2.ChangeCameraAnagle += SwitchCamera;
        C_Airplane.ChangeCamera += SwitchCamera;
    }

    public void OnDisable()
    {
        //playercamera -=SwitchCamera;
        C_Catapult.ExitCatapultMode -= SwitchCamera;
        C_Catapult.EnterCatapultMode -= SwitchCamera;
        C_FriendBossPhase2.ChangeCameraAnagle -= SwitchCamera;
        C_Airplane.ChangeCamera -= SwitchCamera;
    }
}

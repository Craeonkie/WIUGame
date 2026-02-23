using Unity.Cinemachine;
using UnityEngine;

public class C_BossCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _TopCamera;
    [SerializeField] private CinemachineCamera _catapultCamera;
    [SerializeField] private CinemachineCamera _cutSceneCamera;
    public static event System.Action onPlayerCameraActivity;
    public static event System.Action offPlayerCameraActivity;

    public enum c_CameraMode
    {
        PLAYER_CAMERA,
        TOP_CAMERA,
        CATAPULT_CAMERA,
        CUT_SCEEN
    }

    public c_CameraMode _CurCam;

    public void SwitchCamera(c_CameraMode mode)
    {
        switch (mode)
        {
            case c_CameraMode.PLAYER_CAMERA:
                onPlayerCameraActivity?.Invoke();
                _catapultCamera.Priority = 10;
                _TopCamera.Priority = 10;
                _cutSceneCamera.Priority = 10;
                break;
            case c_CameraMode.TOP_CAMERA:
                if (_TopCamera == null) return;
                offPlayerCameraActivity?.Invoke();
                _catapultCamera.Priority = 10;
                _TopCamera.Priority = 60;
                _cutSceneCamera.Priority = 10;
                break;
            case c_CameraMode.CATAPULT_CAMERA:
                if (_catapultCamera == null) return;
                offPlayerCameraActivity?.Invoke();
                _catapultCamera.Priority = 60;
                _TopCamera.Priority = 10;
                _cutSceneCamera.Priority = 10;
                break;
            case c_CameraMode.CUT_SCEEN:
                if (_cutSceneCamera == null) return;
                _cutSceneCamera.Priority = 60;
                offPlayerCameraActivity?.Invoke();
                _catapultCamera.Priority = 10;
                _TopCamera.Priority = 10;
                break;
        }
        _CurCam = mode;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void OnEnable()
    {
        //playercamera +=SwitchCamera;
    }

    public void OnDisable()
    {
        //playercamera -=SwitchCamera;
    }
}

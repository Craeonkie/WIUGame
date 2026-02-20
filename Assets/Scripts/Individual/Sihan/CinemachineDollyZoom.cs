using Unity.Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("CinemachineDollyZoom")]
public class CinemachineDollyZoom : CinemachineExtension
{
    public Transform target;

    public float _frustumHeight;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize && target != null)
        {
            float distance = Vector3.Distance(state.GetCorrectedPosition(), target.position);

            float newFOV = 2.0f * Mathf.Atan(_frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;

            LensSettings lens = state.Lens;
            lens.FieldOfView = newFOV;
            state.Lens = lens;
        }
    }

    public void ResetFrustum()
    {
        _frustumHeight = 0;
    }
}
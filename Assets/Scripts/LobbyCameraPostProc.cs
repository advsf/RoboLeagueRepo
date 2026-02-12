using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraPostProcLink : MonoBehaviour
{
    [SerializeField] private UniversalAdditionalCameraData camData;

    private void OnEnable()
    {
        EnablePostProc();
    }

    public void EnablePostProc()
    {
        bool isEnabled = FBPP.GetInt("PostProcessing", 1) == 1;

        camData.renderPostProcessing = isEnabled;
    }
}
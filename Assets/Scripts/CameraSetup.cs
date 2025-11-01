using UnityEngine;

/// <summary>
/// Automatycznie ustawia Main Camera do trybu 2D Orthographic z czarnym tłem.
/// </summary>
public class CameraSetup : MonoBehaviour
{
    [ContextMenu("Setup Orthographic Camera")]
    public void SetupOrthographicCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found!");
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = 10;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        Debug.Log("Camera setup completed!");
    }
}

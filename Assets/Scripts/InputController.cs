using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Obsługuje interakcje użytkownika z LineInstrument:
/// - Klikanie na końce linii (rayOrigin / circleCenter)
/// - Przeciąganie końców linii
/// - Swap: jeśli użytkownik złapie circleMarker, staje się on rayOrigin
/// - Audio playback: na grab i na zmianę pitch
/// </summary>
public class InputController : MonoBehaviour
{
    [SerializeField] private LineInstrument lineInstrument;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float grabRadiusScreen = 0.05f; // % szerokości ekranu
    
    private bool _isDraggingRayOrigin = false;
    private bool _isDraggingCircleCenter = false;
    private Vector3 _dragOffset = Vector3.zero;
    private bool _shouldSwapOnDrop = false;
    private bool _hasGrabbed = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (lineInstrument == null)
            lineInstrument = GetComponent<LineInstrument>();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Sprawdzenie czy użytkownik kliknął na rayOrigin, circleCenter, czy nigdzie
            float rayOriginDist = Vector3.Distance(mouseWorldPos, lineInstrument.GetRayOrigin());
            float circleCenterDist = Vector3.Distance(mouseWorldPos, lineInstrument.GetCircleCenter());
            
            float grabRadius = grabRadiusScreen * mainCamera.orthographicSize * 2f;
            
            if (rayOriginDist < grabRadius)
            {
                _isDraggingRayOrigin = true;
                _shouldSwapOnDrop = false;
                _hasGrabbed = true;
                _dragOffset = lineInstrument.GetRayOrigin() - mouseWorldPos;
                lineInstrument.OnGrab();
            }
            else if (circleCenterDist < grabRadius)
            {
                _isDraggingCircleCenter = true;
                _shouldSwapOnDrop = true;
                _hasGrabbed = true;
                _dragOffset = lineInstrument.GetCircleCenter() - mouseWorldPos;
                lineInstrument.OnGrab();
            }
        }
        
        // Drag
        if (Mouse.current.leftButton.isPressed && (_isDraggingRayOrigin || _isDraggingCircleCenter))
        {
            Vector3 newPos = mouseWorldPos + _dragOffset;
            
            if (_isDraggingRayOrigin)
            {
                lineInstrument.SetRayOrigin(newPos);
            }
            else if (_isDraggingCircleCenter)
            {
                lineInstrument.SetCircleCenter(newPos);
            }
        }
        
        // Release
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_isDraggingCircleCenter && _shouldSwapOnDrop)
            {
                lineInstrument.SwapEndpoints();
            }
            
            if (_hasGrabbed)
            {
                lineInstrument.OnRelease();
                _hasGrabbed = false;
            }
            
            _isDraggingRayOrigin = false;
            _isDraggingCircleCenter = false;
            _shouldSwapOnDrop = false;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = 10f; // Arbitralna głębokość dla 2D
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}

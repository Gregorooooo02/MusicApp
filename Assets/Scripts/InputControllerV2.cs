using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Obsługuje interakcje użytkownika z instrumentami (LineInstrument, PointInstrument, itp).
/// - Klikanie/przeciąganie końców linii
/// - Klikanie/przeciąganie punktów
/// - Detekt grab vs drag
/// </summary>
public class InputControllerV2 : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float grabRadiusScreen = 0.05f; // % szerokości ekranu
    
    private LineInstrument _currentLineInstrument = null;
    private PointInstrument _currentPointInstrument = null;
    
    // State dla linii
    private bool _isDraggingRayOrigin = false;
    private bool _isDraggingCircleCenter = false;
    private bool _shouldSwapOnDrop = false;
    
    // State dla punktu
    private bool _isDraggingPoint = false;
    
    private Vector3 _dragOffset = Vector3.zero;
    private bool _hasGrabbed = false;
    
    // Referencje do wszystkich instrumentów w scenie
    private LineInstrument[] _lineInstruments;
    private PointInstrument[] _pointInstruments;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // Znajdź wszystkie instrumenty w scenie
        RefreshInstruments();
    }

    private void RefreshInstruments()
    {
        _lineInstruments = FindObjectsOfType<LineInstrument>();
        _pointInstruments = FindObjectsOfType<PointInstrument>();
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
            HandlePress(mouseWorldPos);
        }
        
        // Drag
        if (Mouse.current.leftButton.isPressed && (_isDraggingRayOrigin || _isDraggingCircleCenter || _isDraggingPoint))
        {
            HandleDrag(mouseWorldPos);
        }
        
        // Release
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleRelease();
        }
    }

    private void HandlePress(Vector3 mouseWorldPos)
    {
        float grabRadius = grabRadiusScreen * mainCamera.orthographicSize * 2f;
        
        // Sprawdź czy kliknięto na linię
        foreach (LineInstrument line in _lineInstruments)
        {
            if (line == null) continue;
            
            float rayOriginDist = Vector3.Distance(mouseWorldPos, line.GetRayOrigin());
            float circleCenterDist = Vector3.Distance(mouseWorldPos, line.GetCircleCenter());
            
            if (rayOriginDist < grabRadius)
            {
                _currentLineInstrument = line;
                _isDraggingRayOrigin = true;
                _shouldSwapOnDrop = false;
                _hasGrabbed = true;
                _dragOffset = line.GetRayOrigin() - mouseWorldPos;
                line.OnGrab();
                return;
            }
            else if (circleCenterDist < grabRadius)
            {
                _currentLineInstrument = line;
                _isDraggingCircleCenter = true;
                _shouldSwapOnDrop = true;
                _hasGrabbed = true;
                _dragOffset = line.GetCircleCenter() - mouseWorldPos;
                line.OnGrab();
                return;
            }
        }
        
        // Sprawdź czy kliknięto na punkt
        foreach (PointInstrument point in _pointInstruments)
        {
            if (point == null) continue;
            
            float pointDist = Vector3.Distance(mouseWorldPos, point.GetPosition());
            
            if (pointDist < grabRadius)
            {
                _currentPointInstrument = point;
                _isDraggingPoint = true;
                _hasGrabbed = true;
                _dragOffset = point.GetPosition() - mouseWorldPos;
                point.OnGrab();
                return;
            }
        }
    }

    private void HandleDrag(Vector3 mouseWorldPos)
    {
        Vector3 newPos = mouseWorldPos + _dragOffset;
        
        if (_isDraggingRayOrigin && _currentLineInstrument != null)
        {
            _currentLineInstrument.SetRayOrigin(newPos);
        }
        else if (_isDraggingCircleCenter && _currentLineInstrument != null)
        {
            _currentLineInstrument.SetCircleCenter(newPos);
        }
        else if (_isDraggingPoint && _currentPointInstrument != null)
        {
            _currentPointInstrument.SetPosition(newPos);
        }
    }

    private void HandleRelease()
    {
        // Swap endpoints dla linii jeśli potrzeba
        if (_isDraggingCircleCenter && _shouldSwapOnDrop && _currentLineInstrument != null)
        {
            _currentLineInstrument.SwapEndpoints();
        }
        
        // Sygnał release
        if (_hasGrabbed)
        {
            if (_currentLineInstrument != null)
                _currentLineInstrument.OnRelease();
            if (_currentPointInstrument != null)
                _currentPointInstrument.OnRelease();
            _hasGrabbed = false;
        }
        
        // Reset state
        _isDraggingRayOrigin = false;
        _isDraggingCircleCenter = false;
        _isDraggingPoint = false;
        _shouldSwapOnDrop = false;
        _currentLineInstrument = null;
        _currentPointInstrument = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = 10f; // Arbitralna głębokość dla 2D
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}

using UnityEngine;

/// <summary>
/// Wykrywa kolizje między PointInstrument a LineInstrument.
/// Kiedy punkt zbliży się do linii, wymusza vibrato na struny i deformuje linię wizualnie.
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2f;  // Jak blisko punkt musi być do linii
    
    private PointInstrument[] _points;
    private LineInstrument[] _lines;

    private void Start()
    {
        RefreshInstruments();
    }

    private void Update()
    {
        RefreshInstruments();
        DetectInteractions();
    }

    /// <summary>
    /// Odświeża listę instrumentów w scenie.
    /// </summary>
    private void RefreshInstruments()
    {
        _points = FindObjectsByType<PointInstrument>(FindObjectsSortMode.None);
        _lines = FindObjectsByType<LineInstrument>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// Sprawdza czy któryś punkt koliduje z którąś linią.
    /// </summary>
    private void DetectInteractions()
    {
        if (_points == null || _lines == null) 
        {
            Debug.LogWarning("InteractionDetector: _points lub _lines jest null!");
            return;
        }

        Debug.Log($"InteractionDetector: Znalazł {_points.Length} punktów i {_lines.Length} linii");

        foreach (var point in _points)
        {
            foreach (var line in _lines)
            {
                Vector3 pointPos = point.GetPosition();
                float distanceToLine = DistancePointToLineSegment(pointPos, line.GetRayOrigin(), line.GetCircleCenter());
                
                Debug.Log($"Odległość: {distanceToLine:F3}, detectionRadius: {detectionRadius}");
                
                if (distanceToLine < detectionRadius)
                {
                    // Punkt koliduje z linią!
                    Debug.Log("KOLIZJA WYKRYTA!");
                    ApplyVibratoToLine(line, pointPos, distanceToLine);
                    DeformLineAtCollision(line, pointPos);
                }
            }
        }
    }

    /// <summary>
    /// Oblicza najmniejszą odległość między punktem a linią prostą (segment).
    /// </summary>
    private float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineVec = lineEnd - lineStart;
        float lineLength = lineVec.magnitude;
        
        if (lineLength < 0.001f)
            return Vector3.Distance(point, lineStart);
        
        Vector3 lineDir = lineVec.normalized;
        float t = Vector3.Dot(point - lineStart, lineDir);
        t = Mathf.Clamp01(t / lineLength);
        
        Vector3 closestPoint = lineStart + t * lineVec;
        return Vector3.Distance(point, closestPoint);
    }

    /// <summary>
    /// Aplikuje vibrato do struny gdy punkt ją dotknie.
    /// </summary>
    private void ApplyVibratoToLine(LineInstrument line, Vector3 collisionPoint, float distance)
    {
        var vibrator = line.GetComponent<VibratoModulator>();
        if (vibrator == null)
        {
            Debug.Log("ApplyVibratoToLine: Tworzę VibratoModulator");
            vibrator = line.gameObject.AddComponent<VibratoModulator>();
            // WAŻNE: Wymusz Start() jeśli komponent został właśnie dodany
            if (vibrator != null)
            {
                // Wywołaj manualna inicjalizację
                vibrator.GetType().GetMethod("Start", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .Invoke(vibrator, null);
            }
        }

        // Siła vibratto oparta o bliskość (im bliżej, tym silniejsze)
        float strength = 1f - (distance / detectionRadius);
        Debug.Log($"ApplyVibratoToLine: Ustawiam vibrato intensity na {strength}");
        vibrator.SetVibratoIntensity(strength);
    }

    /// <summary>
    /// Deformuje linię w miejscu kolizji z punktem.
    /// </summary>
    private void DeformLineAtCollision(LineInstrument line, Vector3 collisionPoint)
    {
        var deformer = line.GetComponent<LineDeformer>();
        if (deformer == null)
        {
            Debug.Log("DeformLineAtCollision: Tworzę LineDeformer");
            deformer = line.gameObject.AddComponent<LineDeformer>();
            // WAŻNE: Wymusz Start() jeśli komponent został właśnie dodany
            if (deformer != null)
            {
                // Wywołaj manualna inicjalizację
                deformer.GetType().GetMethod("Start", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .Invoke(deformer, null);
            }
        }

        Debug.Log($"DeformLineAtCollision: Ustawiam deformację na {collisionPoint}");
        deformer.SetDeformation(collisionPoint, detectionRadius);
    }
}

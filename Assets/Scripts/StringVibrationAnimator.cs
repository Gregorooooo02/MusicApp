using UnityEngine;

/// <summary>
/// Animuje drganie struny (deformacja LineRenderer) z wieloma harmonicznymi.
/// Struna przymocowana na obu końcach - środek drga maksymalnie.
/// Realistyczne drganie z decay amplitudy — bardziej "strunowate".
/// </summary>
public class StringVibrationAnimator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    [Header("Vibration Settings")]
    [SerializeField] private float vibrationAmplitude = 0.3f;      // Początkowa amplituda
    [SerializeField] private float vibrationFrequency = 15f;       // Hz - główna częstotliwość
    [SerializeField] private float vibrationDuration = 0.5f;       // Jak długo się animuje drganie
    
    [Header("Line Resolution")]
    [SerializeField] private int lineSegments = 20;                // Ilość segmentów linii dla smooth drgania
    
    [Header("Harmonics (dla bardziej naturalnego brzmienia)")]
    [SerializeField] private float harmonic2Strength = 0.4f;       // 2x częstotliwość (40% amplitudy)
    [SerializeField] private float harmonic3Strength = 0.2f;       // 3x częstotliwość (20% amplitudy)
    
    private float _vibrationTimer = 0f;
    private bool _isVibrating = false;
    private bool _needsReset = false;

    private void OnEnable()
    {
        // Ustaw liczbę segmentów gdy komponent się włączy
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = lineSegments;
        }
    }

    private void Update()
    {
        if (_isVibrating)
        {
            _vibrationTimer -= Time.deltaTime;
            if (_vibrationTimer <= 0f)
            {
                _isVibrating = false;
                _needsReset = true;
            }
            else
            {
                UpdateVibration();
            }
        }
        else if (_needsReset)
        {
            // Zresetuj do dwóch punktów
            RestoreLinePositions();
            _needsReset = false;
        }
    }

    /// <summary>
    /// Rozpoczyna animację drgania struny.
    /// </summary>
    public void StartVibration()
    {
        if (lineRenderer == null)
            return;

        // Przechwyć aktualne końce linii
        Vector3 start = lineRenderer.GetPosition(0);
        Vector3 end = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        // Ustaw segmenty
        lineRenderer.positionCount = lineSegments;
        
        // Wypełnij pozycje bazowe
        Vector3[] positions = new Vector3[lineSegments];
        for (int i = 0; i < lineSegments; i++)
        {
            float t = lineSegments > 1 ? i / (float)(lineSegments - 1) : 0.5f;
            positions[i] = Vector3.Lerp(start, end, t);
        }
        lineRenderer.SetPositions(positions);
        
        _vibrationTimer = vibrationDuration;
        _isVibrating = true;
        _needsReset = false;
    }

    private void UpdateVibration()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2)
            return;

        float elapsedTime = vibrationDuration - _vibrationTimer;
        
        // Decay envelope - amplituda słabnie w czasie
        float decayFactor = Mathf.Exp(-2.5f * (elapsedTime / vibrationDuration));
        
        // Główna oscylacja
        float vibration = Mathf.Sin(Time.time * vibrationFrequency * 2f * Mathf.PI) * vibrationAmplitude * decayFactor;
        
        // Harmoniki (dodają naturalności)
        vibration += Mathf.Sin(Time.time * vibrationFrequency * 2f * 2f * Mathf.PI) * vibrationAmplitude * harmonic2Strength * decayFactor;
        vibration += Mathf.Sin(Time.time * vibrationFrequency * 2f * 3f * Mathf.PI) * vibrationAmplitude * harmonic3Strength * decayFactor;
        
        // Pobierz aktualne końce
        Vector3 rayOriginStart = lineRenderer.GetPosition(0);
        Vector3 circleCenterEnd = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        
        // Wektor prostopadły do linii
        Vector3 lineDirection = circleCenterEnd - rayOriginStart;
        Vector3 perpendicular = new Vector3(-lineDirection.y, lineDirection.x, 0).normalized;
        
        // Aplikuj drgania do wszystkich segmentów
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = lineRenderer.positionCount > 1 ? i / (float)(lineRenderer.positionCount - 1) : 0.5f;
            
            // Sine envelope dla struny przytwierdzonej na końcach
            // Maksimum w środku (t=0.5), zero na końcach (t=0 i t=1)
            float stringEnvelope = Mathf.Sin(t * Mathf.PI);
            
            // Pozycja na linii bez drgania
            Vector3 basePosition = Vector3.Lerp(rayOriginStart, circleCenterEnd, t);
            
            // Zastosuj drganie (modulowane sine envelope)
            positions[i] = basePosition + perpendicular * vibration * stringEnvelope;
        }
        
        lineRenderer.SetPositions(positions);
    }

    private void RestoreLinePositions()
    {
        if (lineRenderer == null)
            return;

        // Zresetuj do dwóch punktów (početek i koniec)
        lineRenderer.positionCount = 2;
    }

    public bool IsVibrating() => _isVibrating;
}

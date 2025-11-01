using UnityEngine;

/// <summary>
/// Animuje drganie struny (deformacja LineRenderer) z wieloma harmonicznymi.
/// Struna przymocowana na obu końcach - obie końce drają symetrycznie.
/// Realistyczne drganie z decay amplitudy — bardziej "strunowate".
/// 
/// WAŻNE: Pracuje z 2 pozycjami w LineRenderer i je manipuluje podczas animacji.
/// Musi być wyłączony gdy LineInstrument zmienia pozycje (przeciąganie).
/// </summary>
public class StringVibrationAnimator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    [Header("Vibration Settings")]
    [SerializeField] private float vibrationAmplitude = 0.3f;      // Początkowa amplituda
    [SerializeField] private float vibrationFrequency = 15f;       // Hz - główna częstotliwość
    [SerializeField] private float vibrationDuration = 0.5f;       // Jak długo się animuje drganie
    
    [Header("Harmonics (dla bardziej naturalnego brzmienia)")]
    [SerializeField] private float harmonic2Strength = 0.4f;       // 2x częstotliwość (40% amplitudy)
    [SerializeField] private float harmonic3Strength = 0.2f;       // 3x częstotliwość (20% amplitudy)
    
    private float _vibrationTimer = 0f;
    private bool _isVibrating = false;
    private Vector3 _rayOriginBase;
    private Vector3 _circleCenterBase;

    private void Update()
    {
        if (_isVibrating)
        {
            _vibrationTimer -= Time.deltaTime;
            if (_vibrationTimer <= 0f)
            {
                _isVibrating = false;
                RestoreLinePositions();
            }
            else
            {
                UpdateVibration();
            }
        }
    }

    /// <summary>
    /// Rozpoczyna animację drgania struny.
    /// </summary>
    public void StartVibration()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2)
            return;

        // Przechwyć aktualne końce linii
        _rayOriginBase = lineRenderer.GetPosition(0);
        _circleCenterBase = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        
        _vibrationTimer = vibrationDuration;
        _isVibrating = true;
    }

    /// <summary>
    /// Zatrzymuje animację i przywraca pozycje bazowe.
    /// </summary>
    public void StopVibration()
    {
        _isVibrating = false;
        RestoreLinePositions();
    }

    private void UpdateVibration()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2)
        {
            StopVibration();
            return;
        }

        float elapsedTime = vibrationDuration - _vibrationTimer;
        
        // Decay envelope - amplituda słabnie w czasie
        float decayFactor = Mathf.Exp(-2.5f * (elapsedTime / vibrationDuration));
        
        // Główna oscylacja
        float vibration = Mathf.Sin(Time.time * vibrationFrequency * 2f * Mathf.PI) * vibrationAmplitude * decayFactor;
        
        // Harmoniki (dodają naturalności)
        vibration += Mathf.Sin(Time.time * vibrationFrequency * 2f * 2f * Mathf.PI) * vibrationAmplitude * harmonic2Strength * decayFactor;
        vibration += Mathf.Sin(Time.time * vibrationFrequency * 2f * 3f * Mathf.PI) * vibrationAmplitude * harmonic3Strength * decayFactor;
        
        // Wektor prostopadły do linii (bazując na bazowych pozycjach)
        Vector3 lineDirection = _circleCenterBase - _rayOriginBase;
        Vector3 perpendicular = new Vector3(-lineDirection.y, lineDirection.x, 0).normalized;
        
        // Aplikuj drganie - obie końce drają symetrycznie
        Vector3 rayOriginVibrated = _rayOriginBase + perpendicular * vibration * 0.5f;
        Vector3 circleCenterVibrated = _circleCenterBase + perpendicular * vibration * 0.5f;
        
        lineRenderer.SetPosition(0, rayOriginVibrated);
        if (lineRenderer.positionCount > 1)
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, circleCenterVibrated);
    }

    private void RestoreLinePositions()
    {
        if (lineRenderer == null || lineRenderer.positionCount < 2)
            return;

        // Przywróć bazowe pozycje
        lineRenderer.SetPosition(0, _rayOriginBase);
        if (lineRenderer.positionCount > 1)
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, _circleCenterBase);
    }

    public bool IsVibrating() => _isVibrating;

    /// <summary>
    /// Pauzuje vibrato (ale nie resetuje go).
    /// Używane gdy LineDeformer przejmuje kontrolę nad positionCount.
    /// </summary>
    public void PauseVibration()
    {
        _isVibrating = false;
    }

    /// <summary>
    /// Wznawia vibrato (jeśli było wcześniej uruchomione).
    /// </summary>
    public void ResumeVibration()
    {
        if (_vibrationTimer > 0f)
            _isVibrating = true;
    }
}

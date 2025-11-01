using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reprezentuje instrumenty strunowy (likę strunę ukulele z Tchia).
/// - Początek linii (rayOrigin): punkt na okręgu - obrót tego punktu wybiera dźwięk
/// - Koniec linii (circleCenter): środek okręgu - wyznacza pozycję gry
/// - Długość: określa oktawę dźwięku
/// - Kąt: obrót wokół okręgu (mapowanie do C-B gamy)
/// </summary>
public class LineInstrument : MonoBehaviour
{
    [Header("Line References")]
    public LineRenderer lineRenderer;
    public Transform rayOriginMarker;        // Mały punkt na początku linii
    public Transform circleMarker;           // Okrąg na końcu linii (środek okręgu)
    
    [Header("Sound Mapping")]
    [SerializeField] private float circleRadius = 2f;
    [SerializeField] private float minFrequencyHz = 130.81f; // C3
    [SerializeField] private float maxFrequencyHz = 261.63f; // C4 (octave up)
    
    [Header("Visual Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;
    
    [Header("Audio")]
    private StringAudioSynthesizer _audioSynthesizer;
    private StringVibrationAnimator _vibrationAnimator;
    
    // Internal state
    private Vector3 _rayOriginWorld;   // Pozycja początkowa linii (na obwodzie okręgu)
    private Vector3 _circleCenterWorld; // Pozycja środka okręgu (koniec linii)
    private float _currentPitchClass = 0f; // 0-12 (C do B)
    private float _currentOctave = 3f;     // oktawa
    private float _lastPlayedPitchClass = -1f;
    private float _lastPlayedOctave = -1f;
    
    // Callback na zmianę dźwięku
    public UnityEvent<float> OnPitchChanged = new UnityEvent<float>();
    public UnityEvent<float> OnOctaveChanged = new UnityEvent<float>();
    public UnityEvent OnGrabbed = new UnityEvent();
    public UnityEvent OnReleased = new UnityEvent();

    private void Start()
    {
        InitializeFromTransform();
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        _audioSynthesizer = GetComponent<StringAudioSynthesizer>();
        if (_audioSynthesizer == null)
            _audioSynthesizer = gameObject.AddComponent<StringAudioSynthesizer>();

        _vibrationAnimator = GetComponent<StringVibrationAnimator>();
        if (_vibrationAnimator == null)
        {
            _vibrationAnimator = gameObject.AddComponent<StringVibrationAnimator>();
            _vibrationAnimator.GetType().GetField("lineRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .SetValue(_vibrationAnimator, lineRenderer);
        }
    }

    /// <summary>
    /// Inicjalizuje pozycję linii na podstawie pozycji transformów.
    /// </summary>
    public void InitializeFromTransform()
    {
        if (rayOriginMarker == null || circleMarker == null)
        {
            Debug.LogError("LineInstrument: rayOriginMarker i circleMarker muszą być przypisane!");
            return;
        }
        
        _rayOriginWorld = rayOriginMarker.position;
        _circleCenterWorld = circleMarker.position;
        
        UpdateLineRenderer();
        UpdatePitchFromGeometry();
    }

    /// <summary>
    /// <summary>
    /// Uaktualnia pozycję końca linii (zarazem zmienia oktawę i pitch).
    /// Używane przy przesuwaniu głównego końca linii.
    /// </summary>
    public void SetRayOrigin(Vector3 newRayOrigin)
    {
        _rayOriginWorld = newRayOrigin;
        UpdateLineRenderer();
        UpdatePitchFromGeometry();
        CheckAndPlayNote();
    }

    /// <summary>
    /// Uaktualnia pozycję środka okręgu (koniec linii).
    /// Używane przy przesuwaniu końca linii gdzie jest okrąg.
    /// </summary>
    public void SetCircleCenter(Vector3 newCircleCenter)
    {
        _circleCenterWorld = newCircleCenter;
        UpdateLineRenderer();
    }

    /// <summary>
    /// Jeśli użytkownik złapie koniec linii (circleMarker), 
    /// ta funkcja zamienia go na początek (rayOrigin).
    /// Stary początek staje się środkiem okręgu.
    /// </summary>
    public void SwapEndpoints()
    {
        (_rayOriginWorld, _circleCenterWorld) = (_circleCenterWorld, _rayOriginWorld);
        UpdateLineRenderer();
        UpdatePitchFromGeometry();
    }

    /// <summary>
    /// Oblicza aktualny pitch i oktawę na podstawie geometrii linii.
    /// - Kąt (mierzony od środka okręgu) -> pitch (C-B na godzinach 12-11)
    /// - Długość linii -> oktawa
    /// </summary>
    private void UpdatePitchFromGeometry()
    {
        // Wektor od środka do początku
        Vector3 toRayOrigin = _rayOriginWorld - _circleCenterWorld;
        
        // Kąt od góry (12 o'clock) w kierunku zgodnym z zegarem
        // 0° = 12 o'clock (C), 30° = B, ..., 330° = C#
        float angle = GetAngleFrom12OClock(toRayOrigin);
        
        // Mapowanie kąta na pitch (0-11, gdzie 0=C, 11=B)
        _currentPitchClass = angle / 30f; // 360/12 = 30 per semitone
        _currentPitchClass = Mathf.Clamp(_currentPitchClass, 0f, 11.99f);
        
        // Długość linii -> oktawa (im dłuższa, tym wyższa oktawa)
        float length = toRayOrigin.magnitude;
        _currentOctave = Mathf.Clamp(length / circleRadius, 1f, 5f) + 2f; // Skalowanie do 3-7 oktaw
        
        OnPitchChanged?.Invoke(_currentPitchClass);
        OnOctaveChanged?.Invoke(_currentOctave);
    }

    /// <summary>
    /// Oblicza kąt od góry (12 o'clock), zgodnie z ruchem wskazówek zegara.
    /// 0° = up, 90° = right, 180° = down, 270° = left
    /// </summary>
    private float GetAngleFrom12OClock(Vector3 direction)
    {
        // Wektor do kąta: Atan2(x, y) gdzie y = up
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    /// <summary>
    /// Uaktualnia visual LineRenderer.
    /// </summary>
    private void UpdateLineRenderer()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, _rayOriginWorld);
            lineRenderer.SetPosition(1, _circleCenterWorld);
        }

        // Uaktualnij pozycje markerów
        if (rayOriginMarker != null)
            rayOriginMarker.position = _rayOriginWorld;
        
        if (circleMarker != null)
            circleMarker.position = _circleCenterWorld;
    }

    /// <summary>
    /// Sprawdza czy pitch/oktawa się zmienił i gra notę jeśli tak.
    /// </summary>
    private void CheckAndPlayNote()
    {
        float pitchDiff = Mathf.Abs(_currentPitchClass - _lastPlayedPitchClass);
        float octaveDiff = Mathf.Abs(_currentOctave - _lastPlayedOctave);
        
        // Jeśli pitch lub oktawa się zmienili, graj
        if (pitchDiff > 0.1f || octaveDiff > 0.1f)
        {
            PlayNote();
        }
    }

    /// <summary>
    /// Gra notę na aktualnym pitch i oktawie.
    /// </summary>
    public void PlayNote()
    {
        if (_audioSynthesizer == null) return;

        float freq = GetCurrentFrequencyHz();
        _audioSynthesizer.PlayNote(freq, 0.5f); // 0.5s długość dźwięku
        
        if (_vibrationAnimator != null)
            _vibrationAnimator.StartVibration();
        
        _lastPlayedPitchClass = _currentPitchClass;
        _lastPlayedOctave = _currentOctave;
    }

    /// <summary>
    /// Zatrzymuje granie nocie.
    /// </summary>
    public void StopNote()
    {
        if (_audioSynthesizer != null)
            _audioSynthesizer.StopNote();
    }

    /// <summary>
    /// Sygnał że użytkownik złapał strunę.
    /// </summary>
    public void OnGrab()
    {
        PlayNote();
        OnGrabbed?.Invoke();
    }

    /// <summary>
    /// Sygnał że użytkownik puszczył strunę.
    /// </summary>
    public void OnRelease()
    {
        // Nie wył łączamy od razu - pozwolimy ADSR fade out
        OnReleased?.Invoke();
    }

    /// <summary>
    /// Zwraca częstotliwość w Hz na podstawie aktualnego pitch i oktawy.
    /// </summary>
    public float GetCurrentFrequencyHz()
    {
        // Wzór: f = baseFreq * 2^(semitones/12)
        // baseFreq = C3 (130.81 Hz)
        int semitonesFromC3 = (int)_currentPitchClass + ((int)_currentOctave - 3) * 12;
        float frequency = minFrequencyHz * Mathf.Pow(2f, semitonesFromC3 / 12f);
        return frequency;
    }

    /// <summary>
    /// Zwraca nazwę nuty (np. "C", "C#", "D", itd.).
    /// </summary>
    public string GetNoteNameDebug()
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = Mathf.FloorToInt(_currentPitchClass) % 12;
        return noteNames[noteIndex] + Mathf.FloorToInt(_currentOctave);
    }

    private void OnDrawGizmos()
    {
        if (circleMarker == null) return;

        // Rysuj okrąg (przewodnik dla pitch)
        Gizmos.color = Color.yellow;
        DrawCircle(circleMarker.position, circleRadius, 24);

        // Rysuj linię (jeśli jest rayOriginMarker)
        if (rayOriginMarker != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(rayOriginMarker.position, circleMarker.position);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPos = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPos = center + new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0);
            Gizmos.DrawLine(prevPos, newPos);
            prevPos = newPos;
        }
    }

    public Vector3 GetRayOrigin() => _rayOriginWorld;
    public Vector3 GetCircleCenter() => _circleCenterWorld;
    public float GetCircleRadius() => circleRadius;
    public float GetCurrentPitchClass() => _currentPitchClass;
    public float GetCurrentOctave() => _currentOctave;
}

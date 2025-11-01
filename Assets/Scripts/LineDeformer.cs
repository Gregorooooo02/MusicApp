using UnityEngine;

/// <summary>
/// Deformuje LineRenderer gdy punkt dotknie strunę.
/// Tworzy wizualny "break" w miejscu kolizji - punkt "łamie" strunę.
/// </summary>
public class LineDeformer : MonoBehaviour
{
    [Header("Deformation Settings")]
    [SerializeField] private float deformationStrength = 0.8f;  // Jak daleko "pęka" struna
    [SerializeField] private float deformationSmoothing = 0.6f; // Jak szybko się deformuje
    [SerializeField] private int segmentCount = 6;             // Liczba segmentów dla smooth deformacji
    
    [Header("Vibration During Deformation")]
    [SerializeField] private float vibratoAmplitude = 0.15f;    // Amplituda vibrato
    [SerializeField] private float vibratoFrequency = 8f;       // Hz - szybkość vibrato
    
    private LineRenderer _lineRenderer;
    private VibratoModulator _vibratoModulator;
    private Vector3 _collisionPoint;
    private float _collisionRadius;
    private Vector3[] _basePositions;
    private Vector3[] _deformedPositions;
    private float _currentDeformation = 0f;
    private float _targetDeformation = 0f;

    private void Start()
    {
        // Szukaj LineRenderer na tym GameObject lub dzieciakach
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
            _lineRenderer = GetComponentInChildren<LineRenderer>();
        
        if (_lineRenderer == null)
        {
            Debug.LogError("LineDeformer: Nie znalazł LineRenderer!");
            return;
        }

        // Szukaj VibratoModulator
        _vibratoModulator = GetComponent<VibratoModulator>();
        if (_vibratoModulator == null)
        {
            Debug.Log("LineDeformer: Tworzę VibratoModulator");
            _vibratoModulator = gameObject.AddComponent<VibratoModulator>();
        }

        // Zapisz bazową linię (2 punkty)
        if (_lineRenderer.positionCount < 2)
        {
            Debug.LogError("LineDeformer: LineRenderer ma mniej niż 2 pozycje!");
            return;
        }

        _basePositions = new Vector3[2];
        _basePositions[0] = _lineRenderer.GetPosition(0);
        _basePositions[1] = _lineRenderer.GetPosition(1);
        Debug.Log($"LineDeformer: Zainicjalizowany, bazowe pozycje: {_basePositions[0]}, {_basePositions[1]}");
    }

    private void Update()
    {
        // Płynne przejście deformacji
        _currentDeformation = Mathf.Lerp(_currentDeformation, _targetDeformation, Time.deltaTime / deformationSmoothing);

        // Synchronizuj audio vibrato z deformacją
        if (_vibratoModulator != null)
        {
            _vibratoModulator.SetVibratoIntensity(_currentDeformation);
        }

        if (_currentDeformation > 0.01f)
        {
            ApplyDeformation();
        }
        else if (_lineRenderer != null && _lineRenderer.positionCount != 2)
        {
            // Przywróć bazową linię (2 punkty)
            RestoreBaseLine();
        }

        // Na KOŃCU każdego frame'u - deformacja "wygasa" jeśli nic nowego się nie stanie
        // To pozwala SetDeformation() ustawić _targetDeformation przed tym resetem
        _targetDeformation = Mathf.Lerp(_targetDeformation, 0f, Time.deltaTime / deformationSmoothing);
    }

    /// <summary>
    /// Ustawia punkt deformacji i promień wpływu.
    /// Wywoływane każdy frame gdy jest kolizja.
    /// </summary>
    public void SetDeformation(Vector3 collisionPoint, float radius)
    {
        _collisionPoint = collisionPoint;
        _collisionRadius = radius;
        _targetDeformation = 1f;  // Zawsze 1, będzie się fadeować w Update()
    }

    /// <summary>
    /// Aplikuje deformację linii na podstawie punktu kolizji.
    /// </summary>
    private void ApplyDeformation()
    {
        if (_lineRenderer == null || _basePositions == null || _basePositions.Length < 2)
        {
            Debug.LogError($"ApplyDeformation: LineRenderer={_lineRenderer}, BasePositions={(_basePositions == null ? "null" : _basePositions.Length)}");
            return;
        }

        // Stwórz rozszerzoną linię (segment_count punktów zamiast 2)
        if (_lineRenderer.positionCount != segmentCount)
        {
            _lineRenderer.positionCount = segmentCount;
            _deformedPositions = new Vector3[segmentCount];
        }

        Vector3 lineStart = _basePositions[0];
        Vector3 lineEnd = _basePositions[1];

        // Dla każdego segmentu
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1); // 0 to 1 along the line
            Vector3 basePos = Vector3.Lerp(lineStart, lineEnd, t);

            // Jak daleko jest ten punkt od linii do punktu kolizji
            float distToCollision = Vector3.Distance(basePos, _collisionPoint);

            if (distToCollision < _collisionRadius)
            {
                // Deformuj w kierunku PUNKTU (przyciąganie) zamiast prostopadłego
                Vector3 dirToCollision = (_collisionPoint - basePos).normalized;
                
                // Siła deformacji malejąca wraz z odległością
                float influence = 1f - (distToCollision / _collisionRadius);
                influence = Mathf.Pow(influence, 2f); // Smoother falloff

                Vector3 offset = dirToCollision * deformationStrength * influence * _currentDeformation;
                
                // Dodaj vibrato na topie deformacji (prostopadłe drgania)
                Vector3 perpendicular = new Vector3(-dirToCollision.y, dirToCollision.x, 0).normalized;
                float vibration = Mathf.Sin(Time.time * vibratoFrequency * 2f * Mathf.PI) * vibratoAmplitude * influence * _currentDeformation;
                offset += perpendicular * vibration;
                
                _deformedPositions[i] = basePos + offset;
            }
            else
            {
                _deformedPositions[i] = basePos;
            }

            _lineRenderer.SetPosition(i, _deformedPositions[i]);
        }
    }

    /// <summary>
    /// Przywraca linię do 2 punktów bazowych.
    /// </summary>
    private void RestoreBaseLine()
    {
        if (_lineRenderer == null || _basePositions == null || _basePositions.Length < 2)
            return;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, _basePositions[0]);
        _lineRenderer.SetPosition(1, _basePositions[1]);
    }

    /// <summary>
    /// Uaktualnia bazowe pozycje (gdy struna się porusza).
    /// </summary>
    public void UpdateBasePositions(Vector3 start, Vector3 end)
    {
        // Inicjalizuj jeśli jeszcze nie ma
        if (_basePositions == null)
            _basePositions = new Vector3[2];

        _basePositions[0] = start;
        _basePositions[1] = end;
    }
}

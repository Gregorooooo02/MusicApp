using UnityEngine;

/// <summary>
/// Animuje oddychanie punktu (pulsowanie/skalowanie).
/// Uruchamiane przy kliknięciu lub graniu dźwięku.
/// </summary>
public class PointAnimator : MonoBehaviour
{
    [Header("Breathing Animation")]
    [SerializeField] private float breatheScale = 1.3f;            // Maksymalny rozmiar (1.0 -> 1.3x)
    [SerializeField] private float breatheDuration = 0.3f;         // Czas całej animacji oddychania
    
    private Transform _targetTransform;
    private Vector3 _originalScale;
    private float _breatheTimer = 0f;
    private bool _isBreathing = false;

    private void Start()
    {
        _targetTransform = transform;
        _originalScale = _targetTransform.localScale;
    }

    private void Update()
    {
        if (_isBreathing)
        {
            _breatheTimer -= Time.deltaTime;
            if (_breatheTimer <= 0f)
            {
                _isBreathing = false;
                _targetTransform.localScale = _originalScale;
            }
            else
            {
                UpdateBreathing();
            }
        }
    }

    /// <summary>
    /// Rozpoczyna animację oddychania.
    /// </summary>
    public void StartBreathing()
    {
        _breatheTimer = breatheDuration;
        _isBreathing = true;
    }

    private void UpdateBreathing()
    {
        // Sine wave: 0 -> 1 -> 0 w ciągu breatheDuration
        float progress = 1f - (_breatheTimer / breatheDuration);
        float scale = Mathf.Sin(progress * Mathf.PI);
        
        // Lerp między original a breatheScale
        float scaleFactor = Mathf.Lerp(1f, breatheScale, scale);
        _targetTransform.localScale = _originalScale * scaleFactor;
    }

    public bool IsBreathing() => _isBreathing;
}

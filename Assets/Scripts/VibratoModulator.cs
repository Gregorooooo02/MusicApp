using UnityEngine;

/// <summary>
/// Modulator vibrato dla AudioSource.
/// Gdy punkt dotknie strunę, ustawia vibrato intensity na StringAudioSynthesizer.
/// </summary>
public class VibratoModulator : MonoBehaviour
{
    [Header("Vibrato Settings")]
    [SerializeField] private float attackTime = 0.1f;       // Jak szybko vibrato się "włącza"
    
    private StringAudioSynthesizer _audioSynthesizer;
    private float _vibratoIntensity = 0f;     // 0-1, jak silne vibrato
    private float _targetIntensity = 0f;
    
    private void Start()
    {
        _audioSynthesizer = GetComponent<StringAudioSynthesizer>();
        if (_audioSynthesizer == null)
            _audioSynthesizer = gameObject.AddComponent<StringAudioSynthesizer>();
    }

    private void Update()
    {
        // Płynnie przejście vibratto intensity
        if (_vibratoIntensity < _targetIntensity)
        {
            _vibratoIntensity = Mathf.Lerp(_vibratoIntensity, _targetIntensity, Time.deltaTime / attackTime);
        }
        else if (_vibratoIntensity > _targetIntensity)
        {
            _vibratoIntensity = Mathf.Lerp(_vibratoIntensity, _targetIntensity, Time.deltaTime / (attackTime * 0.5f));
        }

        // Aplikuj na syntetyzator
        if (_audioSynthesizer != null)
            _audioSynthesizer.SetVibratoIntensity(_vibratoIntensity);
    }

    /// <summary>
    /// Ustawia siłę vibratto (0-1).
    /// </summary>
    public void SetVibratoIntensity(float intensity)
    {
        _targetIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>
    /// Resetuje vibrato.
    /// </summary>
    public void ResetVibrato()
    {
        _targetIntensity = 0f;
    }

    public float GetCurrentIntensity() => _vibratoIntensity;
}

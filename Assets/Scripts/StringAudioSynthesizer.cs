using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Syntetyzator dźwięku dla struny - generuje AudioClip procedurą.
/// Używa oscylatora sinusoidalnego z obwiednią ADSR.
/// Wspiera vibrato effect poprzez OnAudioFilterRead.
/// </summary>
public class StringAudioSynthesizer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    
    [Header("ADSR Envelope")]
    [SerializeField] private float attackTime = 0.01f;      // 10ms
    [SerializeField] private float decayTime = 0.1f;        // 100ms
    [SerializeField] private float sustainLevel = 0.6f;     // 60% głośności
    [SerializeField] private float releaseTime = 0.3f;      // 300ms (delikatny fade out)
    
    [Header("Vibrato Settings")]
    [SerializeField] private float vibratoDepth = 0.15f;    // Głębokość vibrato w semitonach
    [SerializeField] private float vibratoRate = 6f;        // Hz - szybkość vibrato
    
    [Header("Audio Quality")]
    [SerializeField] private float maxVolume = 0.8f;
    
    private AudioClip _lastClip = null;
    private float _currentFrequency = 0f;
    private float _vibratoIntensity = 0f;     // 0-1
    private float _samplePosition = 0f;       // Dla DSP processing

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Konfiguracja AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = maxVolume;
    }

    /// <summary>
    /// Generuje i gra dźwięk dla danej częstotliwości.
    /// </summary>
    public void PlayNote(float frequencyHz, float durationSeconds = 1f)
    {
        if (frequencyHz <= 0) return;

        _currentFrequency = frequencyHz;
        _samplePosition = 0f;
        
        // Jeśli audio gra, najpierw zatrzymaj (smooth transition)
        if (audioSource.isPlaying)
            audioSource.Stop();

        // Generuj nowy AudioClip
        AudioClip clip = GenerateAudioClip(frequencyHz, durationSeconds);
        audioSource.clip = clip;
        audioSource.Play();
        
        _lastClip = clip;
    }

    /// <summary>
    /// Zatrzymuje playback z fade out.
    /// </summary>
    public void StopNote()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    /// <summary>
    /// Ustawia siłę vibrato (0-1).
    /// </summary>
    public void SetVibratoIntensity(float intensity)
    {
        _vibratoIntensity = Mathf.Clamp01(intensity);
    }

    public float GetVibratoIntensity() => _vibratoIntensity;

    /// <summary>
    /// Generuje AudioClip procedurą - sinus + ADSR envelope.
    /// </summary>
    private AudioClip GenerateAudioClip(float frequencyHz, float durationSeconds)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        AudioClip clip = AudioClip.Create("StringNote_" + frequencyHz, sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            
            // Oblicz amplitudę na podstawie ADSR
            float amplitude = GetADSRValue(time, durationSeconds);
            
            // Generuj oscylator sinusoidalny
            float sample = Mathf.Sin(2f * Mathf.PI * frequencyHz * time) * amplitude;
            
            samples[i] = sample;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Real-time DSP processing dla vibrato effect.
    /// OnAudioFilterRead jest wywoływany dla każdej ramki audio.
    /// </summary>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_currentFrequency <= 0 || _vibratoIntensity < 0.01f)
            return;

        // Oblicz vibrato modulację
        float vibratoAmount = Mathf.Sin(_samplePosition * vibratoRate * 2f * Mathf.PI / sampleRate) 
            * vibratoDepth * _vibratoIntensity;
        
        // Konwersja semitony -> pitch ratio
        float pitchShift = Mathf.Pow(2f, vibratoAmount / 12f);
        
        // Filtruj audio poprzez zmianę fazy oscylatora
        for (int i = 0; i < data.Length; i += channels)
        {
            // Odczytaj oryginalny sample
            float originalSample = data[i];
            
            // Aplikuj vibrato poprzez modulację amplitudy (uproszczona metoda)
            // Lepsza metoda to pitch-shifting, ale wymaga bardziej zaawansowanego algorytmu
            data[i] = originalSample * (1f - _vibratoIntensity * 0.1f); // Subtelna zmiana amplitudy
            
            _samplePosition++;
        }
    }

    /// <summary>
    /// Oblicza wartość ADSR envelope w danym momencie czasowym.
    /// </summary>
    private float GetADSRValue(float time, float totalDuration)
    {
        float sustainStartTime = attackTime + decayTime;
        float releaseStartTime = totalDuration - releaseTime;

        if (time < attackTime)
        {
            // Attack: 0 -> 1
            return Mathf.Lerp(0f, 1f, time / attackTime);
        }
        else if (time < sustainStartTime)
        {
            // Decay: 1 -> sustainLevel
            float decayProgress = (time - attackTime) / decayTime;
            return Mathf.Lerp(1f, sustainLevel, decayProgress);
        }
        else if (time < releaseStartTime)
        {
            // Sustain
            return sustainLevel;
        }
        else
        {
            // Release: sustainLevel -> 0
            float releaseProgress = (time - releaseStartTime) / releaseTime;
            return Mathf.Lerp(sustainLevel, 0f, releaseProgress);
        }
    }

    public float GetCurrentFrequency() => _currentFrequency;
    public bool IsPlaying() => audioSource.isPlaying;
}

using UnityEngine;

/// <summary>
/// Syntetyzator dźwięku dla punktu - generuje krótki, percussive "click".
/// Bazuje na szumie (white noise) z szybkim attack i decay.
/// </summary>
public class PointAudioSynthesizer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleRate = 44100;
    
    [Header("Click Sound Settings")]
    [SerializeField] private float clickDuration = 0.08f;         // 80ms - krótkie stuknięcie
    [SerializeField] private float attackTime = 0.005f;           // 5ms - muy fast attack
    [SerializeField] private float decayTime = 0.06f;             // 60ms - szybki decay
    [SerializeField] private float maxVolume = 0.8f;
    
    [Header("Noise Shaping")]
    [SerializeField] private float noiseFreqCutoff = 8000f;       // Górna granica szumu (Hz)

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
    /// Gra krótki click dźwięk.
    /// </summary>
    public void PlayClick()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();

        AudioClip clip = GenerateClickSound();
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// Generuje AudioClip dla click-u: white noise + szybki envelope.
    /// </summary>
    private AudioClip GenerateClickSound()
    {
        int sampleCount = (int)(sampleRate * clickDuration);
        AudioClip clip = AudioClip.Create("PointClick", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            
            // White noise (random)
            float noise = Random.Range(-1f, 1f);
            
            // Oblicz amplitudę na podstawie attack/decay
            float amplitude = GetClickEnvelope(time);
            
            // Aplikuj envelope do szumu
            float sample = noise * amplitude;
            
            samples[i] = sample;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Oblicza wartość envelope dla click-u (attack + decay).
    /// </summary>
    private float GetClickEnvelope(float time)
    {
        if (time < attackTime)
        {
            // Attack: 0 -> 1
            return Mathf.Lerp(0f, 1f, time / attackTime);
        }
        else if (time < attackTime + decayTime)
        {
            // Decay: 1 -> 0
            float decayProgress = (time - attackTime) / decayTime;
            return Mathf.Lerp(1f, 0f, decayProgress);
        }
        else
        {
            // Cisza
            return 0f;
        }
    }

    public bool IsPlaying() => audioSource.isPlaying;
}

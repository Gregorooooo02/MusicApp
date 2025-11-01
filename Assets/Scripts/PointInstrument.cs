using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reprezentuje punkt muzyczny - wydaje krótki click dźwięk.
/// Obsługuje interakcje: klikanie (gra click + animacja), przeciąganie.
/// </summary>
public class PointInstrument : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public SpriteRenderer spriteRenderer;
    
    [Header("Audio & Animation")]
    private PointAudioSynthesizer _audioSynthesizer;
    private PointAnimator _animator;
    
    // Events
    public UnityEvent OnClicked = new UnityEvent();
    public UnityEvent OnGrabbed = new UnityEvent();
    public UnityEvent OnReleased = new UnityEvent();

    private void Start()
    {
        InitializeAudio();
        InitializeAnimator();
    }

    private void InitializeAudio()
    {
        _audioSynthesizer = GetComponent<PointAudioSynthesizer>();
        if (_audioSynthesizer == null)
            _audioSynthesizer = gameObject.AddComponent<PointAudioSynthesizer>();
    }

    private void InitializeAnimator()
    {
        _animator = GetComponent<PointAnimator>();
        if (_animator == null)
            _animator = gameObject.AddComponent<PointAnimator>();
    }

    /// <summary>
    /// Gra click dźwięk i uruchamia animację oddychania.
    /// </summary>
    public void PlayClick()
    {
        if (_audioSynthesizer != null)
            _audioSynthesizer.PlayClick();

        if (_animator != null)
            _animator.StartBreathing();

        OnClicked?.Invoke();
    }

    /// <summary>
    /// Sygnał że użytkownik złapał punkt.
    /// </summary>
    public void OnGrab()
    {
        PlayClick();
        OnGrabbed?.Invoke();
    }

    /// <summary>
    /// Sygnał że użytkownik puszczył punkt.
    /// </summary>
    public void OnRelease()
    {
        OnReleased?.Invoke();
    }

    /// <summary>
    /// Zmienia pozycję punktu (przy przeciąganiu).
    /// </summary>
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// Zwraca pozycję punktu w świecie.
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

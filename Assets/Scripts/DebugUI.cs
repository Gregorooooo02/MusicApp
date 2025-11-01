using UnityEngine;
using TMPro;

/// <summary>
/// Wyświetla debug info o aktualnym pitch i frequency.
/// Podłączaj się do onPitchChanged i onOctaveChanged z LineInstrument.
/// </summary>
public class DebugUI : MonoBehaviour
{
    [SerializeField] private LineInstrument lineInstrument;
    [SerializeField] private TextMeshProUGUI debugText;

    private void Update()
    {
        if (lineInstrument == null || debugText == null)
            return;
        
        string note = lineInstrument.GetNoteNameDebug();
        float freq = lineInstrument.GetCurrentFrequencyHz();
        float pitch = lineInstrument.GetCurrentPitchClass();
        float octave = lineInstrument.GetCurrentOctave();
        
        debugText.text = $"Note: {note}\nFreq: {freq:F2} Hz\nPitch Class: {pitch:F2}\nOctave: {octave:F2}";
    }
}

# MusicApp — LineInstrument + PointInstrument Prototype (Etap 1-2)

Interaktywna aplikacja do tworzenia muzyki poprzez geometryczne kształty. 
- **LineInstrument** — struna muzyczna z mechaniką kołową (jak Tchia)
- **PointInstrument** — punkt z krótkim click dźwiękiem i animacją oddychania

## 🎯 Zrealizowane Etapy

### Etap 1: LineInstrument ✅
- ✅ Wizualizacja struny (linia + markery)
- ✅ Mapowanie geometrii na pitch (kąt → nuta, długość → oktawa)
- ✅ Interakcje: przesuwanie końców, swap endpoints
- ✅ Synteza audio (oscylator sinusoidalny + ADSR envelope)
- ✅ Animacja drgania struny z harmonicznymi (realistyczne, "strunowate")

### Etap 2: PointInstrument ✅
- ✅ Click dźwięk (white noise + fast attack/decay)
- ✅ Animacja oddychania (pulsowanie ~0.3s)
- ✅ Interakcje: klikanie, przeciąganie

### Etap 3: Interakcje Punkt-Struna ✅
- ✅ Detekcja kolizji między punktem a linią
- ✅ Vibrato effect na dźwięku struny (modulacja amplitudy)
- ✅ Deformacja linii w miejscu kolizji (wizualny efekt "łamania")

## 🚀 Setup i Uruchomienie

### Opcja 1: Automatyczne Setup (najszybsze)

1. **Otwórz scenę** `Assets/Scenes/SampleScene.unity`
2. **W Hierarchy**, kliknij prawy przycisk na dowolnym GameObject
3. **W Inspector**, szukaj komponenty `LineInstrumentSetup` lub `CameraSetup`
4. Prawy przycisk → **"Setup Orthographic Camera"** (jeśli po raz pierwszy)
5. Prawy przycisk → **"Setup Line Instrument"** (struna)
6. Prawy przycisk → **"Setup Point Instrument"** (punkt)

### Opcja 2: Manualne Setup

Jeśli automatyczne nie zadziała, zobacz README z poprzedniej wersji (setup manualne).

## 🎮 Sterowanie

| Akcja | Opis |
|-------|------|
| **Kliknij + Drag na linię** | Obracanie struny (zmiana nuty) lub przesunięcie |
| **Kliknij + Drag na punkt** | Przesunięcie punktu |
| **Puszczenie circleMarker na linii** | Swap endpoints |
| **Punkt zbliża się do linii** | Vibrato na dźwięku + deformacja linii |
| **Debug UI** | Wyświetla aktualną nutę i Hz (jeśli setup) |

## 🎚️ Parametry do Dostrojenia (w Inspectorze)

### StringVibrationAnimator:
- `vibrationAmplitude` (0.3) — mocność drgania
- `vibrationFrequency` (15 Hz) — szybkość drgania
- `vibrationDuration` (0.5s) — jak długo drga

### StringAudioSynthesizer:
- `vibratoDepth` (0.15) — głębokość vibrato w semitonach
- `vibratoRate` (6 Hz) — szybkość modulacji vibrato

### InteractionDetector:
- `detectionRadius` (0.5) — jak blisko punkt musi być do linii aby "łamać" ją

### LineDeformer:
- `deformationStrength` (0.2) — jak daleko "pęka" struna
- `deformationSmoothing` (0.3) — szybkość deformacji
- `segmentCount` (20) — liczba segmentów dla smooth deformacji

### PointAnimator:
- `breatheScale` (1.3) — maksymalny rozmiar przy oddychaniu
- `breatheDuration` (0.3s) — czas animacji

### PointAudioSynthesizer:
- `clickDuration` (0.08s) — długość click dźwięku
- `attackTime` (0.005s) — szybkość wejścia
- `decayTime` (0.06s) — szybkość wyjścia

## 📋 Struktura Kodu

```
Assets/Scripts/
├── LineInstrument.cs              # Struna (geometria + audio + vibration)
├── LineInstrumentSetup.cs         # Helper do setuppingu
├── StringAudioSynthesizer.cs      # Generator dźwięku struny (sinus + ADSR + vibrato)
├── StringVibrationAnimator.cs     # Animacja drgania struny
├── PointInstrument.cs             # Punkt (click + oddychanie)
├── PointAudioSynthesizer.cs       # Generator click dźwięku
├── PointAnimator.cs               # Animacja oddychania
├── InteractionDetector.cs         # Detekcja kolizji punkt-struna
├── VibratoModulator.cs            # Modulator vibrato dla struny
├── LineDeformer.cs                # Deformacja linii w miejscu kolizji
├── InputControllerV2.cs           # Obsługuje LineInstrument + PointInstrument
├── InputController.cs             # (stara wersja, można usunąć)
├── DebugUI.cs                     # Wyświetlanie info
└── CameraSetup.cs                 # Ustawienie kamery
```

## 📊 Mapowanie Dźwięku

### LineInstrument (struna):
- **Pitch:** Kąt od góry (12 o'clock = C, każde 30° = półton)
- **Oktawa:** Długość linii (dłuższa = niższa)
- **Dźwięk:** Sinus oscylator z ADSR envelope (300ms delikatny fade out)

### PointInstrument (punkt):
- **Dźwięk:** White noise + szybki click (80ms)
- **Attack:** 5ms, Decay: 60ms
- **Animacja:** Skalowanie 1.0 → 1.3 → 1.0 (0.3s)

## 🎵 Następne Kroki (Etap 4+)

1. **PlaneInstrument:** Perkusja (kształt → instrument)
   - Triangle, Rectangle, Pentagon, Hexagon, Circle
   - Rozmiar → pitch mapping

2. **Shader:** Zmiana koloru przy nakładaniu elementów

3. **Pozostałe interakcje:**
   - Punkt na płaszczyznę → reverb
   - Płaszczyzna na płaszczyznę → layering perkusji
   - Punkt na punkt → forbidden (show error feedback)

4. **Dodatkowe ulepszenia:**
   - Możliwość tworzenia wielu strun/punktów (prawy przycisk myszy)
   - Ograniczenie ruchu do ram okna aplikacji
   - Undo stack (Backspace)

## ⚠️ Znane Problemy / TODO

- [ ] InputControllerV2 może konflikować ze starym InputController (można usunąć stary)
- [ ] Deprecation warnings w LineInstrumentSetup (FindObjectOfType itp.) — na 2022.3+ zamienić na nowe API
- [ ] Vibrato effect w OnAudioFilterRead jest uproszczony (modulacja amplitudy zamiast pitch-shiftu)
- [ ] LineDeformer zmienia positionCount (może kolidować z StringVibrationAnimator)
- [ ] Brak możliwości tworzenia wielu strun/punktów prawym przyciskiem
- [ ] Brak ograniczenia ruchu do ram okna
- [ ] Brak undo stack (Backspace)
- [ ] Brak efektów dla pozostałych interakcji (point-plane, plane-plane)

## 📝 Notatki

- LineInstrument używa 2 pozycji dla smooth drgania struny (bezpieczne, bez desynchronizacji)
- LineDeformer tworzy do 20 segmentów dla smooth deformacji linii przy kolizji
- PointInstrument click jest generowany procedurą (white noise)
- InputControllerV2 automatycznie odnajduje wszystkie instrumenty w scenie
- InteractionDetector sprawdza kolizje punkt-struna co frame (Update)
- Vibrato w StringAudioSynthesizer jest real-time DSP effect (OnAudioFilterRead)
- Wszystkie pozycje są w world space (3D w 2D płaszczyźnie z=0)

---

**Autor:** MusicApp Dev  
**Data:** 1.11.2025  
**Wersja:** 0.3 (LineInstrument + PointInstrument + Interakcje Punkt-Struna) 

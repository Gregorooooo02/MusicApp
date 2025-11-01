using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script do automatycznego setuppingu struktury LineInstrument w scenie.
/// Uruchom to w Edit->Play lub za pomocą przycisku w edytorze.
/// Po uruchomieniu możesz ten skrypt usunąć.
/// </summary>
public class LineInstrumentSetup : MonoBehaviour
{
    [ContextMenu("Setup Line Instrument")]
    public void SetupLineInstrument()
    {
        // Wyczyszczenie poprzednich instancji
        DestroyAllLineInstruments();
        
        // Tworzenie głównego obiektu
        GameObject root = new GameObject("LineInstrumentRoot");
        root.transform.position = Vector3.zero;

        // Tworzenie LineRenderer object
        GameObject lineRendererObj = new GameObject("LineRendererDisplay");
        lineRendererObj.transform.SetParent(root.transform);
        lineRendererObj.transform.localPosition = Vector3.zero;
        
        LineRenderer lr = lineRendererObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = Color.white;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, new Vector3(-2, 0, 0));
        lr.SetPosition(1, new Vector3(0, 0, 0));
        lr.sortingOrder = 0;

        // Tworzenie RayOriginMarker (mały okrąg)
        GameObject rayOriginMarkerObj = new GameObject("RayOriginMarker");
        rayOriginMarkerObj.transform.SetParent(root.transform);
        rayOriginMarkerObj.transform.localPosition = new Vector3(-2, 0, 0);
        
        SpriteRenderer rayOriginSprite = rayOriginMarkerObj.AddComponent<SpriteRenderer>();
        rayOriginSprite.sprite = CreateCircleSprite(0.3f);
        rayOriginSprite.color = Color.white;
        rayOriginSprite.sortingOrder = 1;

        // Tworzenie CircleMarker (większy okrąg, oznaczające środek)
        GameObject circleMarkerObj = new GameObject("CircleMarker");
        circleMarkerObj.transform.SetParent(root.transform);
        circleMarkerObj.transform.localPosition = new Vector3(0, 0, 0);
        
        SpriteRenderer circleMarkerSprite = circleMarkerObj.AddComponent<SpriteRenderer>();
        circleMarkerSprite.sprite = CreateCircleSprite(0.5f);
        circleMarkerSprite.color = Color.white;
        circleMarkerSprite.sortingOrder = 1;

        // Dodanie LineInstrument komponentu
        LineInstrument lineInstr = root.AddComponent<LineInstrument>();
        lineInstr.lineRenderer = lr;
        lineInstr.rayOriginMarker = rayOriginMarkerObj.transform;
        lineInstr.circleMarker = circleMarkerObj.transform;

        // Dodanie AudioSource dla syntezy
        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio

        // Dodanie StringAudioSynthesizer
        StringAudioSynthesizer synth = root.AddComponent<StringAudioSynthesizer>();

        // Dodanie StringVibrationAnimator
        StringVibrationAnimator vibration = root.AddComponent<StringVibrationAnimator>();

        // Dodanie InputController
        InputController input = root.AddComponent<InputController>();
        input.GetType().GetField("lineInstrument", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(input, lineInstr);
        input.GetType().GetField("mainCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(input, Camera.main);

        Debug.Log("LineInstrument setup completed!");
    }

    [ContextMenu("Setup Debug UI")]
    public void SetupDebugUI()
    {
        // Tworzenie Canvas
        GameObject canvasObj = new GameObject("DebugCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Tworzenie TextMeshProUGUI
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(canvasObj.transform);
        
        RectTransform rectTrans = textObj.AddComponent<RectTransform>();
        rectTrans.anchorMin = Vector2.zero;
        rectTrans.anchorMax = Vector2.zero;
        rectTrans.offsetMin = new Vector2(10, 10);
        rectTrans.offsetMax = new Vector2(400, 200);
        
        TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = "Debug Info\nNote: ...\nFreq: ...";
        textComp.fontSize = 36;

        // Dodanie DebugUI
        DebugUI debugUI = canvasObj.AddComponent<DebugUI>();
        debugUI.GetType().GetField("debugText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(debugUI, textComp);
        
        // Znalezienie LineInstrument w scenie
        LineInstrument lineInstr = FindObjectOfType<LineInstrument>();
        if (lineInstr != null)
        {
            debugUI.GetType().GetField("lineInstrument", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(debugUI, lineInstr);
        }

        Debug.Log("Debug UI setup completed!");
    }

    private Sprite CreateCircleSprite(float radius)
    {
        // Tworzenie tekstury koła
        int size = (int)(radius * 64);
        if (size < 2) size = 2;
        if (size > 512) size = 512;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radiusPixels = radius * 32;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                float distToCenter = Vector2.Distance(pixelPos, center);
                colors[y * size + x] = (distToCenter <= radiusPixels) ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 32);
    }

    private void DestroyAllLineInstruments()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "LineInstrumentRoot")
            {
                DestroyImmediate(obj);
            }
        }
    }
}

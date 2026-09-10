using UnityEngine;
using System.Collections;

public class PoopEffect : MonoBehaviour
{
    public Transform spawnAnchor;
    public GameOverController gameOverController;
    public float explodeTime = 3f;

    Sprite poopSprite;
    bool walkSignalReceived = false;
    bool poopActive = false;

    void Awake()
    {
        poopSprite = GeneratePoopSprite();
    }

    public void PlayPoop()
    {
        StartCoroutine(PoopRoutine());
    }

    public void NotifyWalkButtonPressed()
    {
        walkSignalReceived = true;
    }

    public bool IsPoopActive() => poopActive;

    IEnumerator PoopRoutine()
    {
        poopActive = true;
        walkSignalReceived = false;

        var anchor = spawnAnchor != null ? spawnAnchor : transform;
        var go = new GameObject("Poop");
        go.transform.position = anchor.position + new Vector3(0f, 0.02f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = poopSprite;
        sr.sortingOrder = 5;

        Vector3 targetScale = new Vector3(0.42f, 0.42f, 1f);
        go.transform.localScale = Vector3.zero;

        float t = 0f;
        float dur = 0.25f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float overshoot = Mathf.Sin(p * Mathf.PI * 0.5f) * 1.15f - p * p * 0.15f;
            go.transform.localScale = targetScale * Mathf.Max(overshoot, 0f);
            yield return null;
        }
        go.transform.localScale = targetScale;

        float waitT = 0f;
        while (waitT < explodeTime)
        {
            if (walkSignalReceived)
                break;
            waitT += Time.deltaTime;
            yield return null;
        }

        if (walkSignalReceived)
        {
            float ct = 0f;
            float cdur = 0.15f;
            Vector3 startScale = go.transform.localScale;
            while (ct < cdur)
            {
                ct += Time.deltaTime;
                go.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ct / cdur);
                yield return null;
            }
            Destroy(go);
        }
        else
        {
            Destroy(go);
            if (gameOverController != null)
                gameOverController.TriggerGameOver();
        }

        poopActive = false;
    }

    Sprite GeneratePoopSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        Color brown = new Color(0.36f, 0.22f, 0.10f, 1f);
        Color outline = new Color(0.15f, 0.09f, 0.04f, 1f);

        void DrawCircle(float cx, float cy, float r)
        {
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= r) pixels[y * size + x] = brown;
                else if (dist <= r + 1.5f) pixels[y * size + x] = outline;
            }
        }

        DrawCircle(size * 0.5f, size * 0.26f, size * 0.30f);
        DrawCircle(size * 0.5f, size * 0.46f, size * 0.24f);
        DrawCircle(size * 0.5f, size * 0.63f, size * 0.18f);
        DrawCircle(size * 0.5f, size * 0.78f, size * 0.11f);

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.15f), 100f);
    }
}

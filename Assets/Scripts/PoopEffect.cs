using UnityEngine;
using System.Collections;

public class PoopEffect : MonoBehaviour
{
    public Transform spawnAnchor;

    Sprite poopSprite;

    void Awake()
    {
        poopSprite = GeneratePoopSprite();
    }

    public void PlayPoop()
    {
        StartCoroutine(PoopRoutine());
    }

    IEnumerator PoopRoutine()
    {
        var anchor = spawnAnchor != null ? spawnAnchor : transform;
        var go = new GameObject("Poop");
        go.transform.position = anchor.position + new Vector3(0f, 0.02f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = poopSprite;
        sr.sortingOrder = 5;

        Vector3 targetScale = new Vector3(0.35f, 0.35f, 1f);
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

        yield return new WaitForSeconds(2.5f);

        float fadeT = 0f;
        float fadeDur = 0.6f;
        var startColor = sr.color;
        while (fadeT < fadeDur)
        {
            fadeT += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeT / fadeDur);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, a);
            yield return null;
        }
        Destroy(go);
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

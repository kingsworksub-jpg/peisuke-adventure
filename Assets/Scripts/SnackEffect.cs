using UnityEngine;
using System.Collections;

public class SnackEffect : MonoBehaviour
{
    public Transform spawnAnchor;

    Sprite snackSprite;

    void Awake()
    {
        snackSprite = GenerateSnackSprite();
    }

    public void PlaySnack()
    {
        StartCoroutine(SnackRoutine());
    }

    IEnumerator SnackRoutine()
    {
        var anchor = spawnAnchor != null ? spawnAnchor : transform;
        var go = new GameObject("Snack");
        go.transform.position = anchor.position + new Vector3(0f, 0.35f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = snackSprite;
        sr.sortingOrder = 5;

        Vector3 targetScale = new Vector3(0.4f, 0.4f, 1f);
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

        yield return new WaitForSeconds(1.2f);

        float fadeT = 0f;
        float fadeDur = 0.4f;
        while (fadeT < fadeDur)
        {
            fadeT += Time.deltaTime;
            var c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, fadeT / fadeDur);
            sr.color = c;
            yield return null;
        }
        Destroy(go);
    }

    Sprite GenerateSnackSprite()
    {
        int w = 32, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        Color white = new Color(0.96f, 0.94f, 0.88f, 1f);
        Color outline = new Color(0.75f, 0.68f, 0.55f, 1f);

        float r = h * 0.5f - 1f;
        float cy = h * 0.5f;
        float leftCx = r + 1f, rightCx = w - r - 1f;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float px = x + 0.5f, py = y + 0.5f;
            bool inside = false, onOutline = false;
            if (px >= leftCx && px <= rightCx)
            {
                float dy = Mathf.Abs(py - cy);
                if (dy <= r) inside = true;
                else if (dy <= r + 1.2f) onOutline = true;
            }
            else
            {
                float cx = px < leftCx ? leftCx : rightCx;
                float dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                if (dist <= r) inside = true;
                else if (dist <= r + 1.2f) onOutline = true;
            }
            if (inside) pixels[y * w + x] = white;
            else if (onOutline) pixels[y * w + x] = outline;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}

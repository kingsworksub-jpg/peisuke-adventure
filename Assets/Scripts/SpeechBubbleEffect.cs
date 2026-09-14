using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpeechBubbleEffect : MonoBehaviour
{
    public Transform anchor;
    public float holdDuration = 1.5f;
    public float fadeDuration = 0.4f;

    const int TexWidth = 320;
    const int TexHeight = 220;
    const float CoreMinX = 54f;
    const float CoreMaxX = 300f;
    const float CoreMinY = 66f;
    const float CoreMaxY = 200f;
    const float CoreRadius = 30f;
    const float BumpRadius = 24f;
    const int BorderThickness = 6;

    Sprite bubbleSprite;

    void Awake()
    {
        bubbleSprite = GenerateBubbleSprite();
    }

    public void Say(string text)
    {
        StartCoroutine(SayRoutine(text));
    }

    IEnumerator SayRoutine(string text)
    {
        var anchorT = anchor != null ? anchor : transform;

        var canvasGo = new GameObject("SpeechBubble", typeof(Canvas), typeof(CanvasGroup));
        canvasGo.transform.SetParent(anchorT, false);
        canvasGo.transform.localPosition = new Vector3(0.35f, 0.78f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.0028f;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 20;
        var canvasRT = canvasGo.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(TexWidth, TexHeight);

        var canvasGroup = canvasGo.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasGo.transform, false);
        var bgRT = bgGo.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGo.GetComponent<Image>();
        bgImg.sprite = bubbleSprite;
        bgImg.type = Image.Type.Simple;
        bgImg.color = Color.white;
        bgImg.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(canvasGo.transform, false);
        var textRT = textGo.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(CoreMinX / TexWidth, CoreMinY / TexHeight);
        textRT.anchorMax = new Vector2(CoreMaxX / TexWidth, CoreMaxY / TexHeight);
        textRT.offsetMin = new Vector2(10, 6);
        textRT.offsetMax = new Vector2(-10, -6);
        var textComp = textGo.GetComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 64;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.black;
        textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComp.verticalOverflow = VerticalWrapMode.Overflow;
        textComp.resizeTextForBestFit = true;
        textComp.resizeTextMinSize = 20;
        textComp.resizeTextMaxSize = 64;
        textComp.raycastTarget = false;

        float t = 0f;
        float dur = 0.25f;
        while (t < dur)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / dur);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        float ft = 0f;
        while (ft < fadeDuration)
        {
            ft += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - ft / fadeDuration);
            yield return null;
        }

        Destroy(canvasGo);
    }

    static Sprite GenerateBubbleSprite()
    {
        int w = TexWidth, h = TexHeight;
        int b = BorderThickness;
        float bumpR = BumpRadius;

        var bumps = new List<Vector2>();
        for (float x = CoreMinX + bumpR * 0.55f; x <= CoreMaxX - bumpR * 0.55f; x += bumpR * 1.1f)
        {
            bumps.Add(new Vector2(x, CoreMaxY));
            bumps.Add(new Vector2(x, CoreMinY));
        }
        for (float y = CoreMinY + bumpR * 0.55f; y <= CoreMaxY - bumpR * 0.55f; y += bumpR * 1.1f)
        {
            bumps.Add(new Vector2(CoreMinX, y));
            bumps.Add(new Vector2(CoreMaxX, y));
        }
        bumps.Add(new Vector2(CoreMinX, CoreMinY));
        bumps.Add(new Vector2(CoreMaxX, CoreMinY));
        bumps.Add(new Vector2(CoreMinX, CoreMaxY));
        bumps.Add(new Vector2(CoreMaxX, CoreMaxY));

        var trail = new (Vector2 pos, float r)[]
        {
            (new Vector2(CoreMinX - 16, CoreMinY - 22), 14f),
            (new Vector2(CoreMinX - 32, CoreMinY - 40), 9f),
            (new Vector2(CoreMinX - 42, CoreMinY - 54), 5.5f),
        };

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color32[w * h];
        Color32 borderColor = new Color32(30, 30, 30, 255);
        Color32 fillColor = new Color32(255, 255, 255, 255);
        Color32 clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                var p = new Vector2(px, py);

                bool outer = InsideRoundedRect(px, py, CoreMinX, CoreMinY, CoreMaxX, CoreMaxY, CoreRadius);
                bool inner = InsideRoundedRect(px, py, CoreMinX + b, CoreMinY + b, CoreMaxX - b, CoreMaxY - b, Mathf.Max(0, CoreRadius - b));

                foreach (var bp in bumps)
                {
                    float d = Vector2.Distance(p, bp);
                    if (d <= bumpR) outer = true;
                    if (d <= bumpR - b) inner = true;
                }

                foreach (var t in trail)
                {
                    float d = Vector2.Distance(p, t.pos);
                    if (d <= t.r) outer = true;
                    if (d <= Mathf.Max(0f, t.r - b * 0.6f)) inner = true;
                }

                Color32 c = clear;
                if (outer) c = inner ? fillColor : borderColor;
                pixels[y * w + x] = c;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    static bool InsideRoundedRect(float x, float y, float minX, float minY, float maxX, float maxY, float r)
    {
        if (x < minX || x > maxX || y < minY || y > maxY) return false;
        if (x < minX + r && y < minY + r) return Vector2.Distance(new Vector2(x, y), new Vector2(minX + r, minY + r)) <= r;
        if (x > maxX - r && y < minY + r) return Vector2.Distance(new Vector2(x, y), new Vector2(maxX - r, minY + r)) <= r;
        if (x < minX + r && y > maxY - r) return Vector2.Distance(new Vector2(x, y), new Vector2(minX + r, maxY - r)) <= r;
        if (x > maxX - r && y > maxY - r) return Vector2.Distance(new Vector2(x, y), new Vector2(maxX - r, maxY - r)) <= r;
        return true;
    }
}

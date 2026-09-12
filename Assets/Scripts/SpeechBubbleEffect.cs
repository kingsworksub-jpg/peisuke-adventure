using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubbleEffect : MonoBehaviour
{
    public Transform anchor;
    public float holdDuration = 1.5f;
    public float fadeDuration = 0.4f;

    const int TexWidth = 300;
    const int TexHeight = 170;
    const int TailHeight = 44;
    const float CornerRadius = 30f;
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
        canvasGo.transform.localPosition = new Vector3(0f, 1.13f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.0045f;

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
        textRT.anchorMin = new Vector2(0f, (float)TailHeight / TexHeight);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.offsetMin = new Vector2(18, 6);
        textRT.offsetMax = new Vector2(-18, -14);
        var textComp = textGo.GetComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 32;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.black;
        textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComp.verticalOverflow = VerticalWrapMode.Overflow;
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
        int w = TexWidth, h = TexHeight, tailH = TailHeight;
        float r = CornerRadius;
        int b = BorderThickness;

        Vector2 tailBaseL = new Vector2(w * 0.40f, tailH);
        Vector2 tailBaseR = new Vector2(w * 0.56f, tailH);
        Vector2 tailTip = new Vector2(w * 0.30f, 0f);

        Vector2 centroid = (tailBaseL + tailBaseR + tailTip) / 3f;
        Vector2 innerL = Vector2.Lerp(tailBaseL, centroid, 0.34f);
        Vector2 innerR = Vector2.Lerp(tailBaseR, centroid, 0.34f);
        Vector2 innerTip = Vector2.Lerp(tailTip, centroid, 0.34f);

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

                bool outer = InsideRoundedRect(px, py, 0, tailH, w, h, r)
                    || (py < tailH + 2f && PointInTriangle(px, py, tailBaseL, tailBaseR, tailTip));

                Color32 c = clear;
                if (outer)
                {
                    bool inner = InsideRoundedRect(px, py, b, tailH + b, w - b, h - b, Mathf.Max(0, r - b))
                        || (py < tailH + 2f && PointInTriangle(px, py, innerL, innerR, innerTip));
                    c = inner ? fillColor : borderColor;
                }
                pixels[y * w + x] = c;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100f);
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

    static bool PointInTriangle(float px, float py, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 p = new Vector2(px, py);
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}

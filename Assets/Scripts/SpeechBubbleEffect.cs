using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubbleEffect : MonoBehaviour
{
    public Transform anchor;
    public float holdDuration = 1.5f;
    public float fadeDuration = 0.4f;

    public void Say(string text)
    {
        StartCoroutine(SayRoutine(text));
    }

    IEnumerator SayRoutine(string text)
    {
        var anchorT = anchor != null ? anchor : transform;

        var canvasGo = new GameObject("SpeechBubble", typeof(Canvas), typeof(CanvasGroup));
        canvasGo.transform.SetParent(anchorT, false);
        canvasGo.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.01f;

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        var canvasRT = canvasGo.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(260, 90);

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
        bgImg.color = new Color(1f, 1f, 1f, 0.95f);

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(canvasGo.transform, false);
        var textRT = textGo.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 10);
        textRT.offsetMax = new Vector2(-10, -10);
        var textComp = textGo.GetComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 34;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.black;

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
}

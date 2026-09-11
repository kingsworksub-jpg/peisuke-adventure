using UnityEngine;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    public Transform player;
    public float radius = 1.2f;
    public float fadeSpeed = 4f;
    public Image icon;

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (icon != null && icon.sprite == null)
            icon.sprite = IconSprites.GenerateCloudBubble();
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector2.Distance(player.position, transform.position);
        bool inRange = dist <= radius;

        float target = inRange ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);
        canvasGroup.blocksRaycasts = inRange;
        canvasGroup.interactable = inRange;
    }
}

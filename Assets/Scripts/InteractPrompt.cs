using UnityEngine;
using UnityEngine.EventSystems;

public class InteractPrompt : MonoBehaviour
{
    public Transform player;
    public float radius = 1.2f;
    public float fadeSpeed = 4f;
    public float tapRadius = 0.5f;

    public Sprite[] frames;
    public float frameDuration = 0.08f;

    public PeisukeStats snackStats;
    public SnackEffect snackEffect;

    public SpeechBubbleEffect speechBubble;
    public string speechMessage;

    SpriteRenderer sr;
    int frameIndex = 0;
    float frameTimer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        var c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector2.Distance(player.position, transform.position);
        bool inRange = dist <= radius;

        var c = sr.color;
        float target = inRange ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * Time.deltaTime);
        sr.color = c;

        if (inRange && frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % frames.Length;
                sr.sprite = frames[frameIndex];
            }
        }

        if (inRange && c.a > 0.5f && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var cam = Camera.main;
            if (cam == null) return;
            Vector3 worldPt = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPt.z = transform.position.z;
            if (Vector2.Distance(worldPt, transform.position) <= tapRadius)
                OnTapped();
        }
    }

    void OnTapped()
    {
        if (snackStats != null)
        {
            snackStats.CollectSnack();
            if (snackEffect != null) snackEffect.PlaySnack();
        }
        if (speechBubble != null)
        {
            speechBubble.Say(speechMessage);
        }
    }
}

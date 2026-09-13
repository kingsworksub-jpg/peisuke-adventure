using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InteractPrompt : MonoBehaviour
{
    static readonly List<InteractPrompt> all = new List<InteractPrompt>();

    public Transform player;
    public Transform detectionCenter;
    public float radius = 1.2f;
    public float fadeSpeed = 4f;
    public float tapRadius = 0.65f;
    public float iconScale = 1.5f;

    public Sprite[] frames;
    public float frameDuration = 1.0f;

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
        transform.localScale = new Vector3(iconScale, iconScale, 1f);
    }

    void OnEnable()
    {
        all.Add(this);
    }

    void OnDisable()
    {
        all.Remove(this);
    }

    float DistanceToPlayer()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        return Vector2.Distance(player.position, center);
    }

    void Update()
    {
        if (player == null) return;
        float dist = DistanceToPlayer();
        bool inRange = dist <= radius;

        bool isNearest = inRange;
        if (inRange)
        {
            foreach (var other in all)
            {
                if (other == this || other.player == null) continue;
                float otherDist = other.DistanceToPlayer();
                if (otherDist <= other.radius && otherDist < dist)
                {
                    isNearest = false;
                    break;
                }
            }
        }

        var c = sr.color;
        float target = isNearest ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * Time.deltaTime);
        sr.color = c;

        if (isNearest && frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % frames.Length;
                sr.sprite = frames[frameIndex];
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            var cam = Camera.main;
            Vector3 worldPt = cam != null ? cam.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
            worldPt.z = transform.position.z;
            float tapDist = Vector2.Distance(worldPt, transform.position);

            if (isNearest && c.a > 0.5f && !overUI && tapDist <= tapRadius)
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

using UnityEngine;
using System.Collections;

public class TopDownWalker : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Animator animator;
    public Transform visual;
    public LayerMask obstacleLayer;
    public float collisionRadius = 0.25f;

    const float FrontScale = 0.727f;
    const float BackScale = 0.682f;
    const float SideScale = 1f;

    const float JumpHeight = 0.45f;
    const float JumpDuration = 0.45f;

    const float IdleBobAmplitude = 0.035f;
    const float IdleBobSpeed = 2.2f;

    int facingDir = 0; // 0=Down, 1=Up, 2=Left, 3=Right
    Vector2 touchDir = Vector2.zero;
    bool isJumping = false;

    public void SetTouchDirection(Vector2 dir)
    {
        touchDir = dir;
    }

    public void Jump()
    {
        if (!isJumping)
            StartCoroutine(JumpRoutine());
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;
        float t = 0f;
        while (t < JumpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / JumpDuration);
            float arc = Mathf.Sin(p * Mathf.PI);

            float baseScale = facingDir == 0 ? FrontScale : facingDir == 1 ? BackScale : SideScale;
            float squash = 1f + arc * 0.12f;
            visual.localPosition = new Vector3(0f, arc * JumpHeight, 0f);
            visual.localScale = new Vector3(baseScale / squash, baseScale * squash, 1f);

            yield return null;
        }
        visual.localPosition = Vector3.zero;
        isJumping = false;
    }

    bool IsBlocked(Vector2 worldPos)
    {
        return Physics2D.OverlapCircle(worldPos, collisionRadius, obstacleLayer) != null;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v);
        if (touchDir.sqrMagnitude > 0.01f)
            dir = touchDir;

        bool moving = dir.sqrMagnitude > 0.01f;
        animator.SetBool("Moving", moving);

        if (moving)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                facingDir = dir.x > 0 ? 3 : 2; // 3=Right, 2=Left
            else
                facingDir = dir.y > 0 ? 1 : 0; // 1=Up, 0=Down
            animator.SetInteger("FacingDir", facingDir);

            Vector2 norm = dir.normalized;
            Vector2 delta = norm * moveSpeed * Time.deltaTime;
            Vector2 pos = transform.position;

            Vector2 afterX = pos + new Vector2(delta.x, 0f);
            if (delta.x != 0f && !IsBlocked(afterX))
                pos = afterX;

            Vector2 afterY = pos + new Vector2(0f, delta.y);
            if (delta.y != 0f && !IsBlocked(afterY))
                pos = afterY;

            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }

        if (!isJumping)
        {
            float scale = facingDir == 0 ? FrontScale : facingDir == 1 ? BackScale : SideScale;
            visual.localScale = new Vector3(scale, scale, 1f);

            if (moving)
            {
                visual.localPosition = Vector3.zero;
            }
            else
            {
                float bob = Mathf.Sin(Time.time * IdleBobSpeed) * IdleBobAmplitude;
                visual.localPosition = new Vector3(0f, bob, 0f);
            }
        }
    }
}

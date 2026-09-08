using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DPadController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public TopDownWalker target;
    public RectTransform padArea;
    public Image upImage, downImage, leftImage, rightImage;

    const float ActiveAlpha = 0.6f;
    const float InactiveAlpha = 0.35f;
    const float Deadzone = 20f;

    Vector2 currentDir = Vector2.zero;

    public void OnPointerDown(PointerEventData eventData) => UpdateDirection(eventData);
    public void OnDrag(PointerEventData eventData) => UpdateDirection(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        currentDir = Vector2.zero;
        target.SetTouchDirection(Vector2.zero);
        UpdateVisuals();
    }

    void UpdateDirection(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(padArea, eventData.position, eventData.pressEventCamera, out var local);

        if (local.sqrMagnitude < Deadzone * Deadzone)
        {
            currentDir = Vector2.zero;
        }
        else
        {
            float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
            if (angle > -45f && angle <= 45f) currentDir = Vector2.right;
            else if (angle > 45f && angle <= 135f) currentDir = Vector2.up;
            else if (angle > 135f || angle <= -135f) currentDir = Vector2.left;
            else currentDir = Vector2.down;
        }

        target.SetTouchDirection(currentDir);
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        SetAlpha(upImage, currentDir == Vector2.up);
        SetAlpha(downImage, currentDir == Vector2.down);
        SetAlpha(leftImage, currentDir == Vector2.left);
        SetAlpha(rightImage, currentDir == Vector2.right);
    }

    void SetAlpha(Image img, bool active)
    {
        if (img == null) return;
        var c = img.color;
        c.a = active ? ActiveAlpha : InactiveAlpha;
        img.color = c;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class DPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 direction;
    public TopDownWalker target;

    public void OnPointerDown(PointerEventData eventData)
    {
        target.SetTouchDirection(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        target.SetTouchDirection(Vector2.zero);
    }
}

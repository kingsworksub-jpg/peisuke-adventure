using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverRetryButton : MonoBehaviour, IPointerDownHandler
{
    public GameOverController controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.Retry();
    }
}

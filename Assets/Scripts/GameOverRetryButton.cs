using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverRetryButton : MonoBehaviour, IPointerClickHandler
{
    public GameOverController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        controller.Retry();
    }
}

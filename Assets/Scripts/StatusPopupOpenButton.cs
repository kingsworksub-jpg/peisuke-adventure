using UnityEngine;
using UnityEngine.EventSystems;

public class StatusPopupOpenButton : MonoBehaviour, IPointerDownHandler
{
    public StatusPopupController controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.Toggle();
    }
}

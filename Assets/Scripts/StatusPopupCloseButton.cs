using UnityEngine;
using UnityEngine.EventSystems;

public class StatusPopupCloseButton : MonoBehaviour, IPointerDownHandler
{
    public StatusPopupController controller;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.Close();
    }
}

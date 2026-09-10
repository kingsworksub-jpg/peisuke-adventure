using UnityEngine;
using UnityEngine.EventSystems;

public class StatusPopupOpenButton : MonoBehaviour, IPointerClickHandler
{
    public StatusPopupController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        controller.Toggle();
    }
}

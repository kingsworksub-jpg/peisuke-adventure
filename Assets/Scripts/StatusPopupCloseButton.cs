using UnityEngine;
using UnityEngine.EventSystems;

public class StatusPopupCloseButton : MonoBehaviour, IPointerClickHandler
{
    public StatusPopupController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        controller.Close();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class WalkButton : MonoBehaviour, IPointerDownHandler
{
    public PeisukeStats stats;
    public PoopEffect poopEffect;

    public void OnPointerDown(PointerEventData eventData)
    {
        stats.GoForWalk();
        if (poopEffect != null)
            poopEffect.NotifyWalkButtonPressed();
    }
}

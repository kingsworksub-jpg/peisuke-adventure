using UnityEngine;
using UnityEngine.EventSystems;

public class WalkButton : MonoBehaviour, IPointerClickHandler
{
    public PeisukeStats stats;

    public void OnPointerClick(PointerEventData eventData)
    {
        stats.GoForWalk();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class ExamineButton : MonoBehaviour, IPointerDownHandler
{
    public PeisukeStats stats;
    public SnackEffect snackEffect;

    public void OnPointerDown(PointerEventData eventData)
    {
        stats.CollectSnack();
        if (snackEffect != null)
            snackEffect.PlaySnack();
    }
}

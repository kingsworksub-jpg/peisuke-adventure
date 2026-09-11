using UnityEngine;
using UnityEngine.EventSystems;

public class FireplaceExamine : MonoBehaviour, IPointerDownHandler
{
    public SpeechBubbleEffect speechBubble;

    public void OnPointerDown(PointerEventData eventData)
    {
        speechBubble.Say("(あっちっち)");
    }
}

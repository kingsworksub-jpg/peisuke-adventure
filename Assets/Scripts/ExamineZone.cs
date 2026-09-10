using UnityEngine;

public class ExamineZone : MonoBehaviour
{
    public Transform player;
    public GameObject examineButton;
    public float radius = 1.2f;

    void Update()
    {
        if (player == null || examineButton == null) return;
        float dist = Vector2.Distance(player.position, transform.position);
        bool inRange = dist <= radius;
        if (examineButton.activeSelf != inRange)
            examineButton.SetActive(inRange);
    }
}

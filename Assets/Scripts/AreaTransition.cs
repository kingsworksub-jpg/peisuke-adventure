using UnityEngine;

public class AreaTransition : MonoBehaviour
{
    public Transform player;
    public Camera mainCam;

    public float hutBottomBoundary = -4.6f;
    public float kaidoTopBoundary = -10.5f;

    void Update()
    {
        if (player == null)
        {
            var walker = FindObjectOfType<TopDownWalker>();
            if (walker != null) player = walker.transform;
            return;
        }

        if (mainCam == null) mainCam = Camera.main;

        float y = player.position.y;

        // Moving from Hut down to Kaido
        if (y < hutBottomBoundary && y > -8f)
        {
            player.position = new Vector3(player.position.x, -12.2f, player.position.z);
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, -13.5f, -10f);
            }
        }
        // Moving from Kaido up to Hut
        else if (y > kaidoTopBoundary && y < -9f)
        {
            player.position = new Vector3(player.position.x, -3.5f, player.position.z);
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 0f, -10f);
            }
        }
    }
}

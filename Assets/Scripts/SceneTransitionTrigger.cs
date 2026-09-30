using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    public string targetSceneName;
    public float triggerRadius = 1.2f;

    Transform player;

    void Update()
    {
        if (player == null)
        {
            var walker = FindObjectOfType<TopDownWalker>();
            if (walker != null) player = walker.transform;
            return;
        }

        if (Vector2.Distance(transform.position, player.position) <= triggerRadius)
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}

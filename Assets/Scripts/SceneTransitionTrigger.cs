using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    public string targetSceneName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<TopDownWalker>() != null)
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    public GameObject explosionFlash;
    public GameObject gameOverPanel;

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        if (explosionFlash != null)
        {
            explosionFlash.SetActive(true);
            var rt = explosionFlash.GetComponent<RectTransform>();
            var img = explosionFlash.GetComponent<Image>();
            rt.localScale = Vector3.zero;

            float t = 0f;
            float dur = 0.35f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / dur);
                rt.localScale = Vector3.one * p;
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}

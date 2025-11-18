using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SwitchScene : MonoBehaviour
{
    [Header("UI")]
    public Image fadeImage;

    [Header("Fade")]
    public float fadeDuration = 1.2f;

    [Header("Next Scene")]
    public string nextSceneName;

    private bool started = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !other.isTrigger)
        {
            StartCoroutine(FadeRoutine());
        }
    }
    IEnumerator FadeRoutine()
    {
        float t = 0f;

        Color fadeColor = fadeImage.color;
        fadeColor.a = 0f;
        fadeImage.color = fadeColor;

        while (t < 1f)
        {
            // ʱ���ƽ�
            t += Time.deltaTime / fadeDuration;

            // UI ��������Ļ���룺SmoothStep
            float uiSmooth = Mathf.SmoothStep(0, 1, t);

            fadeColor.a = Mathf.Lerp(0f, 1f, uiSmooth);
            fadeImage.color = fadeColor;
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}

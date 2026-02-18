using System.Collections;
using UnityEngine;

public class timeControl : MonoBehaviour
{
    public float targetTime;
    public float ogTime = 1f;

    public AnimationCurve timeCurve;
    [Tooltip("过渡时长（秒）")]
    public float transitionDuration = 0.5f;

    private Coroutine _transitionRoutine;
    private float _defaultFixedDeltaTime;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionTimeScale(ogTime, targetTime));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionTimeScale(Time.timeScale, ogTime));
    }

    private IEnumerator TransitionTimeScale(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float curveValue = timeCurve != null && timeCurve.length > 0
                ? timeCurve.Evaluate(t)
                : t;
            float scale = Mathf.Lerp(from, to, curveValue);
            SetTimeScale(scale);
            yield return null;
        }

        SetTimeScale(to);
        _transitionRoutine = null;
    }

    /// <summary>
    /// 同步设置 timeScale 与 fixedDeltaTime，避免物理补帧造成卡顿
    /// </summary>
    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }
}

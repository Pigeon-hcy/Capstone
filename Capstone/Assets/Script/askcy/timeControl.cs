using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class timeControl : MonoBehaviour
{
    public float targetTime;
    public float ogTime = 1f;

    public AnimationCurve timeCurve;
    [Tooltip("过渡时长（秒）")]
    public float transitionDuration = 0.5f;

    private Coroutine _transitionRoutine;
    private float _defaultFixedDeltaTime;

    public Volume volumeSLow;
    public Volume volumeFast;

    public float weight;
    public float transitionWeightDuration = 1f;
    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        if(targetTime < 1f)
        {
            volumeSLow = GameObject.FindGameObjectWithTag("Slow").GetComponent<Volume>();
        }else
        {
            volumeFast = GameObject.FindGameObjectWithTag("Fast").GetComponent<Volume>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);
        
        StartCoroutine(TransitionWeight(weight, 1f, transitionWeightDuration));

        _transitionRoutine = StartCoroutine(TransitionTimeScale(ogTime, targetTime));

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        StartCoroutine(TransitionWeight(weight, 0f, transitionWeightDuration));

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

    private IEnumerator TransitionWeight(float from, float to, float duration)
    {
        ///从from到to的过渡,修改weight
        /// 使用lerp
        if (duration <= 0f)
        {
            weight = to;
            if (volumeSLow != null) volumeSLow.weight = weight;
            if (volumeFast != null) volumeFast.weight = weight;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weight = Mathf.Lerp(from, to, t);
            if (volumeSLow != null) volumeSLow.weight = weight;
            if (volumeFast != null) volumeFast.weight = weight;
            yield return null;
        }

        weight = to;
        if (volumeSLow != null) volumeSLow.weight = weight;
        if (volumeFast != null) volumeFast.weight = weight;
    }
}

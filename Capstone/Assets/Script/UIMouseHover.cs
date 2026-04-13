using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMouseHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("立体旋转")]
    [SerializeField] private float rotationRange = 15f;
    [SerializeField] private float rotationSmooth = 8f;

    [Header("悬停缩放")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Idle 晃动")]
    [SerializeField] private bool enableIdle = true;
    [SerializeField] private float idleZRange = 3f;
    [SerializeField] private float idleZSpeed = 1.5f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Camera canvasCamera;
    private Vector3 originalScale;
    private Quaternion targetRotation;
    private bool isHovering;
    private float animTimer;
    private bool isAnimating;
    private float baseWidth;
    private float baseHeight;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        originalScale = rectTransform.localScale;
        baseWidth = rectTransform.rect.width;
        baseHeight = rectTransform.rect.height;
        targetRotation = Quaternion.identity;

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = parentCanvas.worldCamera;
    }

    void Update()
    {
        if (isAnimating)
        {
            animTimer += Time.deltaTime;
            float t = Mathf.Clamp01(animTimer / duration);
            float curveValue = scaleCurve.Evaluate(t);

            if (isHovering)
                rectTransform.localScale = Vector3.LerpUnclamped(originalScale, originalScale * scaleMultiplier, curveValue);
            else
                rectTransform.localScale = Vector3.LerpUnclamped(originalScale * scaleMultiplier, originalScale, curveValue);

            if (t >= 1f)
                isAnimating = false;
        }

        if (isHovering)
        {
            Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(canvasCamera, rectTransform.position);
            Vector2 mouseScreen = Input.mousePosition;
            Vector2 offset = mouseScreen - screenCenter;

            float normalizedX = Mathf.Clamp(offset.x / (baseWidth * 0.5f), -1f, 1f);
            float normalizedY = Mathf.Clamp(offset.y / (baseHeight * 0.5f), -1f, 1f);

            targetRotation = Quaternion.Euler(-normalizedY * rotationRange, normalizedX * rotationRange, 0f);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.deltaTime * rotationSmooth);
        }
        else if (enableIdle && !isAnimating)
        {
            float tiltZ = Mathf.Sin(Time.time * idleZSpeed) * idleZRange;
            targetRotation = Quaternion.Euler(0f, 0f, tiltZ);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.deltaTime * rotationSmooth);
        }
        else if (!isAnimating)
        {
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * rotationSmooth);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        animTimer = 0f;
        isAnimating = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        animTimer = 0f;
        isAnimating = true;
        targetRotation = Quaternion.identity;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMouseHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    ISelectHandler, IDeselectHandler
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

    [Header("材质效果")]
    [SerializeField] private Material rainbowMaterial;

    [Header("点击VFX + 加载关卡")]
    private GameObject vfxPrefab;

    private RectTransform rectTransform;
    private Image buttonImage;
    private Canvas parentCanvas;
    private Camera canvasCamera;
    private bool isLoadingScene;
    private Vector3 originalScale;
    private Quaternion targetRotation;
    private bool isHovering;
    private float animTimer;
    private bool isAnimating;
    private float baseWidth;
    private float baseHeight;
    private bool dimensionsCached;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();

        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = GetComponentInChildren<Image>();

        if (buttonImage != null && rainbowMaterial != null)
            buttonImage.material = rainbowMaterial;
    }

    void Start()
    {
        originalScale = rectTransform.localScale;
        targetRotation = Quaternion.identity;
        
        canvasCamera = Camera.main;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = parentCanvas.worldCamera;
        var ps = GetComponentInChildren<ParticleSystem>();
        vfxPrefab = ps != null ? ps.gameObject : null;
        
    }

    void Update()
    {
        if (isAnimating)
        {
            animTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(animTimer / duration);
            float curveValue = scaleCurve.Evaluate(t);

            if (isHovering)
                rectTransform.localScale = Vector3.LerpUnclamped(originalScale, originalScale * scaleMultiplier, curveValue);
            else
                rectTransform.localScale = Vector3.LerpUnclamped(originalScale * scaleMultiplier, originalScale, curveValue);

            if (t >= 1f)
                isAnimating = false;
        }

        if (isHovering && dimensionsCached)
        {
            Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(canvasCamera, rectTransform.position);
            Vector2 mouseScreen = Input.mousePosition;
            Vector2 offset = mouseScreen - screenCenter;

            float normalizedX = Mathf.Clamp(offset.x / (baseWidth * 0.5f), -1f, 1f);
            float normalizedY = Mathf.Clamp(offset.y / (baseHeight * 0.5f), -1f, 1f);

            targetRotation = Quaternion.Euler(-normalizedY * rotationRange, normalizedX * rotationRange, 0f);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.unscaledDeltaTime * rotationSmooth);
        }
        else if (enableIdle && !isAnimating)
        {
            float tiltZ = Mathf.Sin(Time.unscaledTime * idleZSpeed) * idleZRange;
            targetRotation = Quaternion.Euler(0f, 0f, tiltZ);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRotation, Time.unscaledDeltaTime * rotationSmooth);
        }
        else if (!isAnimating)
        {
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, Quaternion.identity, Time.unscaledDeltaTime * rotationSmooth);
        }
    }

    public void Setup(float rotRange, float scaleMult, bool idle, Material mat)
    {
        rotationRange = rotRange;
        scaleMultiplier = scaleMult;
        enableIdle = idle;
        if (mat != null)
        {
            rainbowMaterial = mat;
            if (buttonImage != null)
                buttonImage.material = mat;
        }
    }

    private void CacheDimensions()
    {
        baseWidth = rectTransform.rect.width;
        baseHeight = rectTransform.rect.height;
        dimensionsCached = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!dimensionsCached) CacheDimensions();
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

    public void OnSelect(BaseEventData eventData)
    {
        if (!dimensionsCached) CacheDimensions();
        isHovering = true;
        animTimer = 0f;
        isAnimating = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isHovering = false;
        animTimer = 0f;
        isAnimating = true;
        targetRotation = Quaternion.identity;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLoadingScene) return;
        if (vfxPrefab == null) return;

        StartCoroutine(PlayVFXAndLoadScene());
        Debug.Log("PlayVFXAndLoadScene");
    }

    private IEnumerator PlayVFXAndLoadScene()
    {
        
        
        
        ParticleSystem ps = vfxPrefab.GetComponent<ParticleSystem>();
        ps.Play();
        if (ps != null)
        {
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        //yield return new WaitForSeconds(delayAfterVFX);

        
        
    }
}

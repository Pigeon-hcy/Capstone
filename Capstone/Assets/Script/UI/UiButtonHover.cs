using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UiButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerMoveHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleSpeed = 12f;

    [Header("Tilt")]
    [SerializeField] private float tiltAmount = 10f;
    [SerializeField] private float tiltSmoothSpeed = 10f;

    [Header("Outline")]
    [SerializeField] private Color outlineHoverColor = Color.white;
    [SerializeField] private float outlineHoverSize = 6f;

    private RectTransform rectTransform;
    private Outline outline;

    private Vector3 originalScale;
    private Quaternion targetRotation;

    private float hoverAmount;
    private bool hovering;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        originalScale = rectTransform.localScale;

        outline.enabled = true;

        Color startColor = outlineHoverColor;
        startColor.a = 0f;

        outline.effectColor = startColor;
        outline.effectDistance = Vector2.zero;

        targetRotation = Quaternion.identity;
    }

    private void Update()
    {
        float scaleT = 1f - Mathf.Exp(-scaleSpeed * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-tiltSmoothSpeed * Time.deltaTime);

        // Hover animation
        float targetHover = hovering ? 1f : 0f;

        hoverAmount = Mathf.Lerp(
            hoverAmount,
            targetHover,
            scaleT
        );

        // Scale
        Vector3 targetScale = Vector3.Lerp(
            originalScale,
            originalScale * hoverScale,
            hoverAmount
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            scaleT
        );

        // Rotation
        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            targetRotation,
            rotT
        );

        // Outline fade
        Color outlineColor = outlineHoverColor;
        outlineColor.a = hoverAmount;

        outline.effectColor = outlineColor;

        float outlineSize = Mathf.Lerp(
            0f,
            outlineHoverSize,
            hoverAmount
        );

        outline.effectDistance = new Vector2(
            outlineSize,
            outlineSize
        );
    }

    public void OnPointerEnter(PointerEventData eventData) => hovering = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        targetRotation = Quaternion.identity;
    }

    public void OnSelect(BaseEventData eventData) => hovering = true;

    public void OnDeselect(BaseEventData eventData)
    {
        hovering = false;
        targetRotation = Quaternion.identity;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float normalizedX = Mathf.Clamp(
            localPos.x / (width * 0.5f),
            -1f,
            1f
        );

        float normalizedY = Mathf.Clamp(
            localPos.y / (height * 0.5f),
            -1f,
            1f
        );

        // Softer tilt response
        normalizedX *= 0.7f;
        normalizedY *= 0.7f;

        float rotX = -normalizedY * tiltAmount;
        float rotY = normalizedX * tiltAmount;

        targetRotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    private void OnDisable()
    {
        hovering = false;

        hoverAmount = 0f;

        rectTransform.localScale = originalScale;
        rectTransform.localRotation = Quaternion.identity;

        targetRotation = Quaternion.identity;

        EventSystem.current?.SetSelectedGameObject(null);
    }
}
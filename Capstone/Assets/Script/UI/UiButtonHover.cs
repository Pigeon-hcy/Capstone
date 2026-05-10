using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerMoveHandler
{
    [Header("Scale")]
    public float hoverScale = 1.1f;
    public float scaleSpeed = 10f;

    [Header("Tilt")]
    public float tiltAmount = 10f;
    public float tiltSmoothSpeed = 8f;

    [Header("Outline")]
    public Color outlineHoverColor = Color.white;
    public float outlineHoverSize = 8f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Quaternion targetRotation;

    private bool hovering = false;
    private Vector2 localMousePosition;

    private Outline outline;
    private Vector2 originalOutlineSize;
    private Color originalOutlineColor;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        targetRotation = Quaternion.identity;

        // Get or add outline
        outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        originalOutlineSize = outline.effectDistance;
        originalOutlineColor = outline.effectColor;

        // Start disabled
        outline.enabled = false;
    }

    void Update()
    {
        // Scale
        Vector3 targetScale = hovering
            ? originalScale * hoverScale
            : originalScale;

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        // Rotation
        rectTransform.localRotation = Quaternion.Lerp(
            rectTransform.localRotation,
            targetRotation,
            Time.deltaTime * tiltSmoothSpeed
        );
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        // Enable outline
        outline.enabled = true;
        outline.effectColor = outlineHoverColor;
        outline.effectDistance = new Vector2(
            outlineHoverSize,
            outlineHoverSize
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;

        targetRotation = Quaternion.identity;

        // Disable outline
        outline.enabled = false;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition
        );

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float normalizedX = Mathf.Clamp(
            localMousePosition.x / (width / 2),
            -1f,
            1f
        );

        float normalizedY = Mathf.Clamp(
            localMousePosition.y / (height / 2),
            -1f,
            1f
        );

        float rotX = -normalizedY * tiltAmount;
        float rotY = normalizedX * tiltAmount;

        targetRotation = Quaternion.Euler(rotX, rotY, 0);
    }

   
}
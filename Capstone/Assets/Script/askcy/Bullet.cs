using UnityEngine;
using HUDIndicator;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float xSpeed = 10f;
    public float ySpeed = 0f;

    public Vector3 startPoint;
    public bool isActive = false;

    [Header("HUD 离屏指示器（可选）")]
    public IndicatorOffScreen offscreenIndicator;

    public float indicatorTimer = 1f;
    void Start()
    {
        startPoint = transform.position;
        if (offscreenIndicator == null)
            offscreenIndicator = GetComponentInChildren<IndicatorOffScreen>(true);
    }

    void Update()
    {
        if (isActive)
        {
            transform.Translate(Vector2.right * xSpeed * Time.deltaTime, Space.World);
            transform.Translate(Vector2.up * ySpeed * Time.deltaTime, Space.World);
        }
    }

    public void launch()
    {
        transform.position = startPoint;
        isActive = true;
        StartCoroutine(ShowIndicator());
    }

    public void reset()
    {
        isActive = false;
        transform.position = startPoint;
    }

    IEnumerator ShowIndicator()
    {
        if (offscreenIndicator == null) yield break;
        offscreenIndicator.enabled = true;
        yield return new WaitForSeconds(indicatorTimer);
        offscreenIndicator.enabled = false;
    }




}

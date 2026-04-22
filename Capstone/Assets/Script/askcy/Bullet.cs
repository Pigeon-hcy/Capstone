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

    public GameObject Bullet3D;
    public float indicatorTimer = 1f;

    public SpriteRenderer Bullet2D_SpriteRenderer;

    private Transform _bullet3DTransform;

    
    void Start()
    {
        startPoint = transform.position;
        if (offscreenIndicator == null)
            offscreenIndicator = GetComponentInChildren<IndicatorOffScreen>(true);

        EnsureBullet3DChild();
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

        EnsureBullet3DChild();

        _playMissile();
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
    public void _playMissile()
    {
        AudioManager.Instance.fmodPlayMissile();
    }

    private void EnsureBullet3DChild()
    {
        if (_bullet3DTransform != null) return;

        // If Bullet3D is already a child in-scene, use it. Otherwise treat it as a prefab and instantiate it as child.
        if (Bullet3D != null && Bullet3D.transform != null && Bullet3D.transform.IsChildOf(transform))
        {
            _bullet3DTransform = Bullet3D.transform;
        }
        else if (Bullet3D != null)
        {
            var prefabLocalRot = Bullet3D.transform.localRotation;
            var prefabLocalScale = Bullet3D.transform.localScale;
            var instance = Instantiate(Bullet3D, transform);
            instance.name = Bullet3D.name;
            _bullet3DTransform = instance.transform;

            // Keep the prefab's original local rotation/scale.
            _bullet3DTransform.localRotation = prefabLocalRot;
            _bullet3DTransform.localScale = prefabLocalScale;
        }

        if (_bullet3DTransform != null)
        {
            _bullet3DTransform.localPosition = Vector3.zero; // xyz = 0,0,0
        }
    }




}

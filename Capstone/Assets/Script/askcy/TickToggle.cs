using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class TickToggle : MonoBehaviour
{
    public float resetTime = 5f;
    private float timer = 0f;

    public bool isUsed = false;
    public MMF_Player OnUseEffect;

    public SpriteRenderer spriteRenderer;
    public Sprite usedSprite;
    public Sprite normalSprite;

    public GameObject targetGrid;
    public float openTime = 5f;
    public float openTimer;

    public bool isOpen = false;

    public GameObject progressBar;

    void Start()
    {
        targetGrid.SetActive(false);
        progressBar.SetActive(false);
        timer = resetTime;
        isUsed = false;
        openTimer = openTime;
        isOpen = false;

    }

    void Update()
    {
        if (isUsed && resetTime > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isUsed = false;
                timer = resetTime;
            }
        }

        if (isUsed)
        {
            spriteRenderer.sprite = usedSprite;
        }
        else
        {
            spriteRenderer.sprite = normalSprite;
        }

        if (isOpen)
        {
            openTimer -= Time.deltaTime;
            if (openTimer <= 0)
            {
                isOpen = false;
                openTimer = openTime;
                EndOpen();
            }
            else if (progressBar != null)
            {
                progressBar.GetComponent<MMProgressBar>().UpdateBar(openTimer / openTime, 0, 1);
            }
        }

        if(!isOpen)
        {
            progressBar.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isUsed) return;

        if (other.CompareTag("Player"))
        {
            isUsed = true;
            if (OnUseEffect != null)
            {
                OnUseEffect.PlayFeedbacks();
            }
            ToggleOpen();
        }
    }

    public void ToggleOpen()
    {
        targetGrid.SetActive(true);
        isOpen = true;
        openTimer = openTime;
        progressBar.SetActive(true);
    }

    public void EndOpen()
    {
        targetGrid.SetActive(false);
        progressBar.SetActive(false);
    }
}

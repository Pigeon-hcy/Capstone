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
    public static float openTimer;

    public static bool isOpen = false;

    /// <summary>当前处于打开状态、负责扣时间的那个实例；只有它会执行 openTimer 倒数。</summary>
    private static TickToggle _currentOpenInstance;

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

        // 只有“当前打开”的那一个实例用 Time.deltaTime 扣时间，保证按现实秒计时
        // FOR JERRY'S AUDIO - TICK TOCK BLOCK ACTIVE
        if (isOpen && _currentOpenInstance == this)
        {
            openTimer -= Time.deltaTime;
            if (openTimer <= 0)
            {
                isOpen = false;
                openTimer = openTime;
                _currentOpenInstance = null;
                EndOpen();
            }
            else if (progressBar != null)
            {
                progressBar.GetComponent<MMProgressBar>().UpdateBar(openTimer / openTime, 0, 1);
            }
        }

        if (!isOpen)
        {
            progressBar.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isUsed) return;

        if (other.CompareTag("Player"))
        {
            if(isOpen == true)
            {
                openTimer = openTime;
            }

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
        openTimer = openTime;  // openTime 单位为秒，与 Time.deltaTime 一致
        _currentOpenInstance = this;
        progressBar.SetActive(true);
    }

    public void EndOpen()
    {
        targetGrid.SetActive(false);
        progressBar.SetActive(false);
    }

    public void ResetOnPlayerDeath()
    {
        timer = resetTime;
        isUsed = false;
        if (_currentOpenInstance == this)
        {
            EndOpen();
            _currentOpenInstance = null;
        }
        isOpen = false;
        openTimer = openTime;
    }
}

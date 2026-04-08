using UnityEngine;
using System.Collections;

public class AttackTrigger : MonoBehaviour
{
    public Bullet[] bullets;
    public float attackInterval = 1f;

    public float resetTime = 5f;
    public float resetTimer = 0f;
    public bool isResetting = false;

    private Coroutine _launchCoroutine;

    void Start()
    {
        // 不在这里调用 bullet.reset()，否则会在 Bullet.Start() 记录起点之前把子弹移到 startPoint（此时为默认 0,0,0）
        resetTimer = resetTime;
        isResetting = false;
        // 等一帧让所有 Bullet.Start() 先执行并记录 startPoint，再禁用子弹
        StartCoroutine(DisableBulletsAfterInit());
    }

    private IEnumerator DisableBulletsAfterInit()
    {
        yield return null; // 等一帧，确保 Bullet.Start() 已执行并记录了 startPoint
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null) bullet.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isResetting)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0)
            {
                isResetting = false;
                resetTimer = resetTime;
                foreach (Bullet bullet in bullets)
                {
                    bullet.reset();
                    if (bullet != null) bullet.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isResetting) return;

        _pauseMissile();
        _launchCoroutine = StartCoroutine(LaunchBullets());
        isResetting = true;
        resetTimer = resetTime;
    }

    private IEnumerator LaunchBullets()
    {
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null)
            {
                bullet.gameObject.SetActive(true);
                bullet.launch();
                //_playMissile();
            }
            yield return new WaitForSeconds(attackInterval);
        }
        _launchCoroutine = null;
        yield return null;
    }

    public void ResetTrapOnPlayerDeath()
    {
        if (_launchCoroutine != null)
        {
            StopCoroutine(_launchCoroutine);
            _launchCoroutine = null;
        }
        isResetting = false;
        resetTimer = 0f;
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null)
            {
                bullet.reset();
                bullet.gameObject.SetActive(false);
            }
        }
    }
    public void _playMissile()
    {
        AudioManager.Instance.fmodPlayMissile();
    }
    public void _pauseMissile()
    {
        AudioManager.Instance.fmodPauseMissile();
    }
}

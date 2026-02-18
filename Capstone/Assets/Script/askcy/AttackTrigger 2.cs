using UnityEngine;
using System.Collections;

public class AttackTrigger : MonoBehaviour
{
    public Bullet[] bullets;
    public float attackInterval = 1f;

    public float resetTime = 5f;
    public float resetTimer = 0f;
    public bool isResetting = false;

    void Start()
    {
        // 不在这里调用 bullet.reset()，否则会在 Bullet.Start() 记录起点之前把子弹移到 startPoint（此时为默认 0,0,0）
        resetTimer = resetTime;
        isResetting = false;
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
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isResetting) return;

        StartCoroutine(LaunchBullets());
        isResetting = true;
        resetTimer = resetTime;
    }

    private IEnumerator LaunchBullets()
    {
        foreach (Bullet bullet in bullets)
        {
            bullet.launch();
            yield return new WaitForSeconds(attackInterval);
        }
        yield return null;
    }
}

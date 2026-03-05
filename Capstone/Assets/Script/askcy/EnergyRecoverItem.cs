using UnityEngine;
using QFramework;
using SkateGame;
using MoreMountains.Feedbacks;

public class EnergyRecoverItem : MonoBehaviour, IController
{
    public float resetTime = 5f;
    private float timer = 0f;

    public bool isUsed = false;
    public MMF_Player OnUseEffect;
    private IEnergySystem energySystem;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    public SpriteRenderer spriteRenderer;
    public Sprite usedSprite;
    public Sprite normalSprite;

    void Start()
    {
        energySystem = this.GetSystem<IEnergySystem>();
        timer = resetTime;
        isUsed = false;
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isUsed) return;

        if(other.CompareTag("Player"))
        {
            playEnergy();
            isUsed = true;
            if(OnUseEffect != null)
            {
                OnUseEffect.PlayFeedbacks();
            }
            energySystem.ResetEnergy();
        }
    }

    // FOR JERRY'S AUDIO - ENERGY RECOVER
    public void playEnergy()
    {
        AudioManager.Instance.fmodPlayEnergy();
    }
}

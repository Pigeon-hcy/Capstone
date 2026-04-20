using UnityEngine;
using UnityEngine.UI;
using QFramework;
using MoreMountains.Feedbacks;
using TMPro;

namespace SkateGame
{
    public class UiTrickList : ViewerControllerBase
    {
        public TextMeshProUGUI tricksText;

        public Sprite[] gradeSprites = new Sprite[5];
        public Image gradeImage;

        public Sprite[] frameSprites = new Sprite[5];
        public Image frameImage;

        public Sprite[] decorationSprites = new Sprite[5];
        public Image decorationImage;

        private IPlayerModel playerModel;

        private float score = 0f;
        private int gradeIndex = 4;

        // --------------------------
        // Grade-up animation
        // --------------------------
        private bool playPopAnim = false;
        private float animTime = 0f;
        private const float animDuration = 0.15f;
        private Vector3 baseScale = Vector3.one;
        private const float popScale = 1.3f;
        public MMF_Player levelUpEffect;
        public MMF_Player gainScoreEffect;

        public MMF_Player startScreenEffect;
        public bool isGameStarted = false;


        protected override void InitializeController()
        {
            playerModel = this.GetModel<IPlayerModel>();

            gradeIndex = 4;
            score = 0f;

            UpdateAllSprites();
            gradeImage.fillAmount = 0f;

            baseScale = frameImage.transform.localScale;

            this.RegisterEvent<EnemyKilledEvent>(_ => OnEnemyKilled())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        protected override void OnRealTimeUpdate()
        {
            UpdateScore(Time.deltaTime);
            UpdatePopAnimation(Time.deltaTime);

            if (!isGameStarted && Input.anyKeyDown)
            {
                isGameStarted = true;
                startScreenEffect.PlayFeedbacks();
            }
        }

        // Continuously update score from speed/decay, then sync grade and fill bar
        private void UpdateScore(float dt)
        {
            float speed = Mathf.Abs(playerModel.PushSpeed.Value);
            score = score + (playerModel.Config.Value.scoringSpeedGainRate * speed - playerModel.Config.Value.scoringDecayPerSecond) * dt;
            score = Mathf.Clamp(score, 0f, 100f);
            int newGrade = CalculateGrade((int)score);
            if (newGrade != gradeIndex)
            {
                bool upgraded = newGrade < gradeIndex;
                gradeIndex = newGrade;
                UpdateAllSprites();
                if (upgraded)
                {
                    TriggerPopAnimation();
                    // FOR JERRY'S AUDIO - D/C/B/A/S RANK (gradeIndex: 4=D, 3=C, 2=B, 1=A, 0=S)
                    gainScoreEffect.PlayFeedbacks();
                }
            }

            gradeImage.fillAmount = GetFillInGrade(score);
        }

        private void OnEnemyKilled()
        {
            score += playerModel.Config.Value.scoringKillBonus;
        }

        // Returns normalized fill (0-1) within the current grade band
        private float GetFillInGrade(float s)
        {
            if (s >= 100f) return 1f;
            if (s >= 80f)  return (s - 80f) / 20f;
            if (s >= 60f)  return (s - 60f) / 20f;
            if (s >= 20f)  return (s - 20f) / 40f;
            return s / 20f;
        }

        private int CalculateGrade(int s)
        {
            if (s >= 100) return 0;
            if (s >= 80)  return 1;
            if (s >= 60)  return 2;
            if (s >= 20)  return 3;
            return 4;
        }

        private void UpdateAllSprites()
        {
            gradeImage.sprite = gradeSprites[gradeIndex];
            frameImage.sprite = frameSprites[gradeIndex];
            decorationImage.sprite = decorationSprites[gradeIndex];

            frameImage.enabled = true;
            decorationImage.enabled = true;
        }

        // -----------------------------
        // Grade-up animation
        // -----------------------------
        private void TriggerPopAnimation()
        {
            levelUpEffect.PlayFeedbacks();
            playPopAnim = true;
            animTime = 0f;
        }

        private void UpdatePopAnimation(float dt)
        {
            if (!playPopAnim) return;

            animTime += dt;
            float t = animTime / animDuration;

            if (t >= 1f)
            {
                playPopAnim = false;
                frameImage.transform.localScale = baseScale;
                decorationImage.transform.localScale = baseScale;
                gradeImage.transform.localScale = baseScale;
                return;
            }

            // Ease: pop then return
            float scaleFactor = Mathf.Lerp(popScale, 1f, t);
            Vector3 scaled = baseScale * scaleFactor;

            frameImage.transform.localScale = scaled;
            decorationImage.transform.localScale = scaled;
            gradeImage.transform.localScale = scaled;
        }
    }
}

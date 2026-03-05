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

        public float decayPerSecond = 0.1f;

        private ITrickListModel trickModel;
        private IPlayerModel playerModel;
        private ITrickSystem trickSystem;

        private float sum = 0f;
        private int gradeIndex = 4;
        private float fill = 0f;

        private int groundedFrame = 0;
        private const int groundedNeed = 3;

        // --------------------------
        //升级动画
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
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();

            gradeIndex = 4;
            fill = 0f;

            UpdateAllSprites();
            gradeImage.fillAmount = 0f;

            baseScale = frameImage.transform.localScale;
        }

        protected override void OnRealTimeUpdate()
        {
            HandleLandingDetection();
            UpdateFill(Time.deltaTime);

            // 处理升级动画
            UpdatePopAnimation(Time.deltaTime);

            if (!isGameStarted && Input.anyKeyDown)
            {
                isGameStarted = true;
                startScreenEffect.PlayFeedbacks();
            }
        }

        private void HandleLandingDetection()
        {
            if (playerModel == null) return;

            if (playerModel.IsGrounded.Value)
            {
                groundedFrame++;
                if (groundedFrame == groundedNeed)
                    OnLanding();
            }
            else
            {
                groundedFrame = 0;
            }
        }

        private void UpdateFill(float dt)
        {
            fill -= decayPerSecond * dt;

            if (fill <= 0f)
            {
                fill = 0f;

                if (gradeIndex < 4)
                {
                    gradeIndex++;
                    fill = 1f;
                    UpdateAllSprites();
                }
            }

            gradeImage.fillAmount = fill;
        }

        private void OnLanding()
        {
            tricksText.text = "";

            int added = trickSystem.SumOfScore();
            sum += added;

            int newGrade = CalculateGrade((int)sum);

            if (newGrade < gradeIndex)
            {
                gradeIndex = newGrade;
                fill = 1f;

                UpdateAllSprites();
                TriggerPopAnimation();
                // FOR JERRY'S AUDIO - D/C/B/A/S RANK (gradeIndex: 4=D, 3=C, 2=B, 1=A, 0=S)
                gainScoreEffect.PlayFeedbacks();
            }

            trickSystem.RemoveAllTricks();
        }

        private int CalculateGrade(int s)
        {
            if (s >= 100) return 0;
            if (s >= 80) return 1;
            if (s >= 60) return 2;
            if (s >= 20) return 3;
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
        // 升级动画
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

            // Easing：先大后回
            float scaleFactor = Mathf.Lerp(popScale, 1f, t);

            Vector3 scaled = baseScale * scaleFactor;

            frameImage.transform.localScale = scaled;
            decorationImage.transform.localScale = scaled;
            gradeImage.transform.localScale = scaled;
        }
    }
}

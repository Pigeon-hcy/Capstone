using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;

namespace SkateGame
{
    public class UiTrickList : ViewerControllerBase
    {
        public TextMeshProUGUI tricksText;

        // ★ Fill 用（保持不动）
        public Sprite[] gradeSprites = new Sprite[5];
        public Image gradeImage;

        // ★ 主框（等级框）
        public Sprite[] frameSprites = new Sprite[5];
        public Image frameImage;

        // ★ 装饰框（随等级变化）
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

        protected override void InitializeController()
        {
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();

            gradeIndex = 4;
            fill = 0f;

            UpdateAllSprites();
            gradeImage.fillAmount = fill;
        }

        protected override void OnRealTimeUpdate()
        {
            HandleLandingDetection();
            UpdateFill(Time.deltaTime);
        }

        //---------------------------------------------
        // 落地检测
        //---------------------------------------------
        private void HandleLandingDetection()
        {
            if (playerModel == null) return;

            bool grounded = playerModel.IsGrounded.Value;

            if (grounded)
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

        //---------------------------------------------
        // Fill 衰减
        //---------------------------------------------
        private void UpdateFill(float dt)
        {
            fill -= decayPerSecond * dt;

            if (fill <= 0f)
            {
                fill = 0f;

                // 掉级
                if (gradeIndex < 4)
                {
                    gradeIndex++;
                    fill = 1f;

                    UpdateAllSprites();
                }
            }

            gradeImage.fillAmount = fill;
        }

        //---------------------------------------------
        // 落地触发（只升不降）
        //---------------------------------------------
        private void OnLanding()
        {
            tricksText.text = "";
            sum += trickSystem.SumOfScore();

            int newGrade = CalculateGrade((int)sum);

            if (newGrade < gradeIndex)
            {
                gradeIndex = newGrade;
                fill = 1f;

                UpdateAllSprites();
            }

            trickSystem.RemoveAllTricks();
        }

        //---------------------------------------------
        // 分数 → 等级
        //---------------------------------------------
        private int CalculateGrade(int s)
        {
            if (s >= 100) return 0; 
            if (s >= 80) return 1;
            if (s >= 60) return 2;
            if (s >= 20) return 3;
            return 4; 
        }

        //---------------------------------------------
        // ★ 主框 + 装饰框 + Fill 图 同步更新
        //---------------------------------------------
        private void UpdateAllSprites()
        {
            // fill 用图
            gradeImage.sprite = gradeSprites[gradeIndex];

            // 主框
            frameImage.sprite = frameSprites[gradeIndex];
            frameImage.enabled = true;

            // 装饰框（你的需求：逻辑和 frame 一样）
            decorationImage.sprite = decorationSprites[gradeIndex];
            decorationImage.enabled = true;
        }
    }
}

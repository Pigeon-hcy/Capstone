using UnityEngine;
using UnityEngine.UI;
using QFramework;
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
        private int gradeIndex = 4; // S=0, A=1, B=2, C=3, D=4
        private float fill = 0f;

        private int groundedFrame = 0;
        private const int groundedNeed = 3;

        //-------------------------------------------------------
        // 初始化
        //-------------------------------------------------------
        protected override void InitializeController()
        {
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();

            gradeIndex = 4; 
            fill = 0f;

            UpdateAllSprites();
            gradeImage.fillAmount = 0f;
        }

        //-------------------------------------------------------
        // 实时检测（基类 Update 调用）
        //-------------------------------------------------------
        protected override void OnRealTimeUpdate()
        {
            HandleLandingDetection();
            UpdateFill(Time.deltaTime);
        }

        //-------------------------------------------------------
        // 落地检测（稳定 3 帧）
        //-------------------------------------------------------
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

        //-------------------------------------------------------
        // fill 衰减与掉级
        //-------------------------------------------------------
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

        //-------------------------------------------------------
        // 落地：加分 + 升级（不降）
        //-------------------------------------------------------
        private void OnLanding()
        {
            tricksText.text = "";

            
            int added = trickSystem.SumOfScore();
            sum += added;

            int newGrade = CalculateGrade((int)sum);

            if (newGrade < gradeIndex)   // ★ 升级
            {
                gradeIndex = newGrade;
                fill = 1f;
                UpdateAllSprites();
            }

            trickSystem.RemoveAllTricks();
        }
       

        //-------------------------------------------------------
        // 分数 → 等级
        //-------------------------------------------------------
        private int CalculateGrade(int s)
        {
            if (s >= 100) return 0;
            if (s >= 80)  return 1;
            if (s >= 60)  return 2;
            if (s >= 20)  return 3;
            return 4;
        }

        //-------------------------------------------------------
        // 同步更新：Fill 图 + 主框 + 装饰框
        //-------------------------------------------------------
        private void UpdateAllSprites()
        {
            // Fill Sprite（等级内部）
            gradeImage.sprite = gradeSprites[gradeIndex];

            // 主框
            frameImage.sprite = frameSprites[gradeIndex];
            frameImage.enabled = true;

            // 装饰框
            decorationImage.sprite = decorationSprites[gradeIndex];
            decorationImage.enabled = true;
        }
    }
}

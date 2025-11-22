using UnityEngine;
using UnityEngine.UI;
using System.Text;
using QFramework;
using TMPro;

namespace SkateGame
{
    public class UiTrickList : ViewerControllerBase
    {
        public TextMeshProUGUI tricksText;     
        public Sprite[] gradeSprites = new Sprite[5];
        public Image gradeImage;

        public float decayPerSecond = 0.1f; //fill掉落速度（每秒掉多少）

        private ITrickListModel trickModel;
        private IPlayerModel playerModel;
        private ITrickSystem trickSystem;

        private float sum = 0f;

        private int gradeIndex = 4; // S=0, A=1, B=2, C=3, D=4
        private float fill = 0f;

        // 落地检测（稳定）
        private int groundedFrame = 0;
        private const int groundedNeed = 3;

        protected override void InitializeController()
        {
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();

            gradeIndex = 4;
            fill = 0f;
            DisplayGrade();
        }

        //--------------------------------------------------
        //              ★ 实时检测（每帧执行）
        //--------------------------------------------------
        protected override void OnRealTimeUpdate()
        {
            HandleLandingDetection();   // 落地检测
            UpdateFill(Time.deltaTime); // ★ 正确平滑UI衰减
        }

        //--------------------------------------------------
        //              落地检测（从空中->地面）
        //--------------------------------------------------
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

        //--------------------------------------------------
        //              UI fill 平滑衰减（每帧）
        //--------------------------------------------------
        private void UpdateFill(float dt)
        {
            fill -= decayPerSecond * dt;

            if (fill <= 0f)
            {
                fill = 0f;

                // 掉级逻辑
                if (gradeIndex < 4) // D=4
                {
                    gradeIndex++;
                    fill = 1f;
                    DisplayGrade();
                }
            }

            gradeImage.fillAmount = fill;
        }

        //--------------------------------------------------
        //              落地时触发（只升不降）
        //--------------------------------------------------
        private void OnLanding()
        {
            tricksText.text = "";
            sum += trickSystem.SumOfScore();

            int newGrade = CalculateGrade((int)sum);

            // 只处理升级（降级由 fill 控制）
            if (newGrade < gradeIndex)
            {
                gradeIndex = newGrade;
                fill = 1f;
                DisplayGrade();
            }

            trickSystem.RemoveAllTricks();
        }

        //--------------------------------------------------
        //              分数 → 等级
        //--------------------------------------------------
        private int CalculateGrade(int s)
        {
            if (s >= 100) return 0;
            if (s >= 80) return 1;
            if (s >= 60) return 2;
            if (s >= 20) return 3;
            return 4;
        }

        //--------------------------------------------------
        //              显示等级图片
        //--------------------------------------------------
        private void DisplayGrade()
        {
            gradeImage.sprite = gradeSprites[gradeIndex];
            gradeImage.enabled = true;
        }
    }
}

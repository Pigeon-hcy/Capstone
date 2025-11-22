using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Linq;
using QFramework;
using TMPro;
namespace SkateGame
{
    public class UiTrickList : ViewerControllerBase
    {
        public TextMeshProUGUI tricksText;     
        public TextMeshProUGUI gradeText;
        public Sprite[] gradeSprites = new Sprite[5];
        public Image gradeImage;
        [Header("分数减少设置")]
        public float decreaseRatePerSecond = 10f;
        [Header("Fill衰减设置")]
        public float maxSumForFill = 100f; // fillAmount为1时的最大分数值
        
        private ITrickListModel trickModel;
        private IPlayerModel playerModel;
        private ITrickSystem trickSystem;
        private float sum = 0f;
        private float accumulatedDecrease = 0f; // 累积的减少量（用于平滑衰减）

        protected override void InitializeController()
        {
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();
            
            // 初始化时间跟踪
            lastUpdateTime = Time.time;
            
            if (trickModel != null)
            {
                RefreshUI();
                DisplayGrade();
                
                // 注册事件监听
                this.RegisterEvent<TrickListChangedEvent>(OnTrickListChanged)
                    .UnRegisterWhenGameObjectDestroyed(gameObject);
                
            }
        }
        
        private void OnTrickListChanged(TrickListChangedEvent evt)
        {
            
            RefreshUI();
        }

        protected override void OnRealTimeUpdate()
        {
            
            // 计算时间差
            float currentTime = Time.time;
            float deltaTime = currentTime - lastUpdateTime;
            
            // 每秒减少1，确保不小于0
            float decreaseAmount = 1f * deltaTime;
            sum = Mathf.Max(0f, sum - decreaseAmount);
            
            // 更新最后更新时间
            lastUpdateTime = currentTime;
            gradeImage.fillAmount = sum%20/20;
            
            if (playerModel != null && playerModel.IsGrounded.Value)
            {
                tricksText.text = "";
                sum += trickSystem.SumOfScore();
                
             
                // 根据分数更新等级图片
                DisplayGrade();
                
                trickSystem.RemoveAllTricks();
            }
            
        }
        
        /// <summary>
        /// 根据分数计算等级
        /// </summary>
        private int CalculateGrade(int score)
        {
            int index;
            switch (score)
            {
                case >= 100:
                    index=0;
                    break;
                case >= 80:
                     index=1;
                    break;
                case >= 60:
                   index=2;
                    break;
                case >= 20:
                    index=3;
                    break;
                case >= 15:
                    index=4;
                    break;
                case >= 10:
                    index=4;
                    break;
                default:
                    index=4;
                    break;
            }
            
            Debug.Log($"UiTrickList: 分数 {score} -> 等级索引 {index}");

            return index;
        }
        
    

       
        public void RefreshUI()
        {
            if (tricksText == null || trickModel == null || trickModel.TrickList.Value.Count == 0) return;
            
            StringBuilder sb = new StringBuilder();
            
            foreach (var trick in trickModel.TrickList.Value)
            {
                sb.Append(trick.GetStateName());
                sb.Append("   ");
                sb.Append(trick.ScoreValue);
                sb.Append("\n\n");  // 两个换行符，形成空行
            }
            
            tricksText.text = sb.ToString();
            
        }

        public void DisplayGrade()
        {
            if (gradeImage == null || gradeSprites == null || gradeSprites.Length == 0) return;
            
            // 使用当前总分计算等级索引（将float转换为int）
            int gradeIndex = CalculateGrade(Mathf.RoundToInt(sum));
            
            // 确保索引在有效范围内
            if (gradeIndex >= 0 && gradeIndex < gradeSprites.Length)
            {
                gradeImage.sprite = gradeSprites[gradeIndex];
                gradeImage.enabled = true;
            }
            
        }
    }
}
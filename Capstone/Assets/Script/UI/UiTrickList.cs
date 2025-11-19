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
        public float decreaseRatePerSecond = 15f; // 默认每秒减少15分，类似鬼泣风格
        
        private ITrickListModel trickModel;
        private IPlayerModel playerModel;
        private ITrickSystem trickSystem;
        private int sum = 0;
        private float lastUpdateTime = 0f;
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
            if (sum > 0)
            {
                float currentTime = Time.time;
                float deltaTime = currentTime - lastUpdateTime;
                
                if (deltaTime > 0 && lastUpdateTime > 0)
                {
                    // 累积减少量（每帧都累积，实现平滑连续衰减）
                    accumulatedDecrease += deltaTime * decreaseRatePerSecond;
                    
                    // 当累积减少量达到0.1分或更多时，减少分数（阈值低，衰减更平滑）
                    if (accumulatedDecrease >= 0.1f)
                    {
                        int decreaseAmount = Mathf.FloorToInt(accumulatedDecrease);
                        if (decreaseAmount > 0)
                        {
                            int oldSum = sum;
                            sum = Mathf.Max(0, sum - decreaseAmount);
                            accumulatedDecrease -= decreaseAmount; // 保留小数部分
                            
                            // 如果分数发生变化，更新等级图片
                            if (sum != oldSum)
                            {
                                DisplayGrade();
                            }
                        }
                    }
                }
                
                lastUpdateTime = currentTime;
            }
            else
            {
                // sum为0时，重置累积减少量
                accumulatedDecrease = 0f;
                lastUpdateTime = Time.time;
            }
            
            // 检测落地，清空技巧列表
            if (playerModel != null && playerModel.IsGrounded.Value)
            {
                tricksText.text = "";
                sum += trickSystem.SumOfScore();
                
                // 重置累积减少量（落地时重新开始计时）
                accumulatedDecrease = 0f;
                
                // 根据分数更新等级图片
                DisplayGrade();
                
                // 重置更新时间
                lastUpdateTime = Time.time;
                
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
        
        /// <summary>
        /// 重置总分（可在Inspector中调用）
        /// </summary>
        [ContextMenu("重置总分")]
        public void ResetSum()
        {
            sum = 0;
            Debug.Log("UiTrickList: 总分已重置为 0");
            DisplayGrade();
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
            
            // 使用当前总分计算等级索引
            int gradeIndex = CalculateGrade(sum);
            
            // 确保索引在有效范围内
            if (gradeIndex >= 0 && gradeIndex < gradeSprites.Length)
            {
                gradeImage.sprite = gradeSprites[gradeIndex];
                gradeImage.enabled = true;
            }
        }
    }
}
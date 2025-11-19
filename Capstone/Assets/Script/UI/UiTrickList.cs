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
        private ITrickListModel trickModel;
        private IPlayerModel playerModel;
        private ITrickSystem trickSystem;
        private int sum = 0;

        protected override void InitializeController()
        {
            trickModel = this.GetModel<ITrickListModel>();
            playerModel = this.GetModel<IPlayerModel>();
            trickSystem = this.GetSystem<ITrickSystem>();
            
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
            // 检测落地，清空技巧列表
            
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
                case >= 40:
                    index=3;
                    break;
                case >= 20:
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
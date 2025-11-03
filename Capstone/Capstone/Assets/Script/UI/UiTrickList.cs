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
                
                // 根据分数计算等级
                char grade = CalculateGrade(sum);
                gradeText.text = grade.ToString();
                
                trickSystem.RemoveAllTricks();
            }
        }
        
        /// <summary>
        /// 根据分数计算等级
        /// </summary>
        private char CalculateGrade(int score)
        {
            char grade;
            switch (score)
            {
                case >= 100:
                    grade = 'S'; // 最高等级
                    break;
                case >= 80:
                    grade = 'A';
                    break;
                case >= 60:
                    grade = 'B';
                    break;
                case >= 40:
                    grade = 'C';
                    break;
                case >= 20:
                    grade = 'D';
                    break;
                case >= 10:
                    grade = 'E';
                    break;
                default:
                    grade = 'F'; // 最低等级
                    break;
            }
            
            Debug.Log($"UiTrickList: 分数 {score} -> 等级 {grade}");
            return grade;
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
                sb.Append(trick.TrickName);
                sb.Append("   ");
                sb.Append(trick.ScoreValue);
                sb.Append("\n\n");  // 两个换行符，形成空行
            }
            
            tricksText.text = sb.ToString();
            
        }

        public void DisplayGrade()
        {
            if (gradeText == null || trickModel == null) return;
            
            // 使用当前总分计算等级
            char currentGrade = CalculateGrade(sum);
            gradeText.text = currentGrade.ToString();
        }
    }
}
using UnityEngine;
using TMPro;

namespace SkateGame
{
    /// <summary>
    /// 挂在通关 UI 的 Text 上，每次激活（navigator.SetActive(true)）时自动读取当前关卡最佳成绩。
    /// </summary>
    public class UiBestTime : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string prefix = "Best ";
        [SerializeField] private string noRecordText = "--:--.--";

        private void OnEnable()
        {
            if (label == null) label = GetComponent<TextMeshProUGUI>();
            if (label == null) return;

            var levelModel = GameApp.Interface.GetModel<ILevelModel>();
            var bestTimeSystem = GameApp.Interface.GetSystem<IBestTimeSystem>();
            if (levelModel == null || bestTimeSystem == null)
            {
                label.text = prefix + noRecordText;
                return;
            }

            float? best = bestTimeSystem.GetBestTime(levelModel.CurrentLevelIndex);
            label.text = prefix + (best.HasValue ? Format(best.Value) : noRecordText);
        }

        // 和 UiTimer.cs 相同的 mm:ss.ff 格式
        private static string Format(float elapsed)
        {
            int m = (int)(elapsed / 60f);
            int s = (int)(elapsed % 60f);
            int ms = (int)((elapsed * 100f) % 100f);
            return $"{m:00}:{s:00}.{ms:00}";
        }
    }
}

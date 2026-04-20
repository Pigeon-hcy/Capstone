using QFramework;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SkateGame
{
    public interface IBestTimeSystem : ISystem
    {
        /// <summary>
        /// 尝试更新关卡最佳成绩；无记录或 elapsed 更小时写盘并返回 true
        /// </summary>
        bool TrySaveBestTime(int levelIndex, float elapsed);

        /// <summary>
        /// 返回关卡最佳成绩；若无记录返回 null
        /// </summary>
        float? GetBestTime(int levelIndex);
    }

    // JsonUtility 不支持 Dictionary，用两个平行 list 落盘
    [System.Serializable]
    public class BestTimeData
    {
        public List<int> levelIndices = new List<int>();
        public List<float> bestTimes = new List<float>();
    }

    public class BestTimeSystem : AbstractSystem, IBestTimeSystem
    {
        private IBestTimeModel bestTimeModel;
        private const string SAVE_FILE_NAME = "BestTimes.json";

        protected override void OnInit()
        {
            bestTimeModel = this.GetModel<IBestTimeModel>();
            LoadFromDisk();
        }

        public bool TrySaveBestTime(int levelIndex, float elapsed)
        {
            if (levelIndex < 0) return false;

            if (bestTimeModel.BestTimes.TryGetValue(levelIndex, out float existing) && elapsed >= existing)
            {
                Debug.Log($"BestTimeSystem: 关卡 {levelIndex} 未破纪录 current={elapsed:F3}s best={existing:F3}s");
                return false;
            }

            bestTimeModel.BestTimes[levelIndex] = elapsed;
            SaveToDisk();
            Debug.Log($"BestTimeSystem: 关卡 {levelIndex} 新纪录 {elapsed:F3}s");
            return true;
        }

        public float? GetBestTime(int levelIndex)
        {
            if (bestTimeModel.BestTimes.TryGetValue(levelIndex, out float t)) return t;
            return null;
        }

        private void LoadFromDisk()
        {
            string filePath = GetSaveFilePath();
            bestTimeModel.BestTimes.Clear();

            if (!File.Exists(filePath))
            {
                Debug.Log("BestTimeSystem: 存档文件不存在，使用空记录");
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                BestTimeData data = JsonUtility.FromJson<BestTimeData>(json);

                if (data == null || data.levelIndices == null || data.bestTimes == null) return;

                int count = Mathf.Min(data.levelIndices.Count, data.bestTimes.Count);
                for (int i = 0; i < count; i++)
                {
                    bestTimeModel.BestTimes[data.levelIndices[i]] = data.bestTimes[i];
                }
                Debug.Log($"BestTimeSystem: 已加载 {count} 条最佳成绩");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BestTimeSystem: 加载存档失败: {e.Message}");
            }
        }

        private void SaveToDisk()
        {
            try
            {
                BestTimeData data = new BestTimeData();
                foreach (var kv in bestTimeModel.BestTimes)
                {
                    data.levelIndices.Add(kv.Key);
                    data.bestTimes.Add(kv.Value);
                }

                string json = JsonUtility.ToJson(data, true);
                string filePath = GetSaveFilePath();
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, json);
                Debug.Log($"BestTimeSystem: 存档已保存到 {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BestTimeSystem: 保存存档失败: {e.Message}");
            }
        }

        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }
    }
}

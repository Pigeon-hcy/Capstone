using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Collections.Generic;
using TMPro;

namespace SkateGame
{
    public class LevelManager : ViewerControllerBase
    {
        [Header("关卡列表（顺序 = 关卡顺序）")]
        public List<Level> levelList;

        private ILevelModel levelModel;
        private ILevelSystem levelSystem;
        private ILevelProgressModel levelProgressModel;
        private bool isLoadingLevel = false; // 防止重复加载

        public GameObject allLevelCanvas;
        public GameObject basicCanvas;
        private GameObject levelItemPrefab;
        private Transform groupForButton;
        protected override void InitializeController()
        {
            levelItemPrefab = Resources.Load<GameObject>("LevelItem");
            Debug.Log(levelItemPrefab != null ? "prefab:有" : "prefab:无");
            GameObject levelCanvas = GameObject.Find("LevelCanvas");
            Debug.Log(levelCanvas != null ? "levelCanvas:有" : "levelCanvas:无");
            groupForButton = levelCanvas.transform.Find("AllSelection/groupforButton");
            Debug.Log(groupForButton != null ? "groupForButton:有" : "groupForButton:无");

            isLoadingLevel = false;
            
            levelModel = this.GetModel<ILevelModel>();
            levelSystem = this.GetSystem<ILevelSystem>();
            levelProgressModel = this.GetModel<ILevelProgressModel>();
            this.GetSystem<ILevelProgressSystem>(); // 确保进度已加载

            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            levelModel.LevelList.Clear();
            foreach (Level lvl in levelList)
            {
                levelSystem.AddLevel(lvl);
            }

            // 根据当前场景名称同步 CurrentLevelIndex
            SyncCurrentLevelIndex(currentSceneName);

            InitializeButtons();
            RefreshButtonStatesOnShow();
            BindingLevelInfoToPrefab();
        }

        /// <summary>
        /// 根据场景名称同步当前关卡索引
        /// </summary>
        private void SyncCurrentLevelIndex(string sceneName)
        {
            for (int i = 0; i < levelModel.LevelList.Count; i++)
            {
                if (levelModel.LevelList[i].SceneName == sceneName)
                {
                    levelModel.CurrentLevelIndex = i;
                    levelModel.CurrentLevelName = levelModel.LevelList[i].Name;
                    Debug.Log($"LevelManager: 同步当前关卡索引为 {i} (场景: {sceneName})");
                    return;
                }
            }
            
            // 如果找不到匹配的场景，默认设置为 0
            if (levelModel.LevelList.Count > 0)
            {
                levelModel.CurrentLevelIndex = 0;
                levelModel.CurrentLevelName = levelModel.LevelList[0].Name;
                Debug.LogWarning($"LevelManager: 未找到场景 '{sceneName}' 对应的关卡，默认设置为索引 0");
            }
        }

        protected override void OnRealTimeUpdate()
        {
            // 可以在这里添加实时更新逻辑
        }

        private void InitializeButtons()
        {
            for (int i = 0; i < levelList.Count; i++)
            {
                int index = i;
                Level lvl = levelList[i];

                if (lvl.button != null)
                {
                    lvl.button.onClick.RemoveAllListeners();
                    lvl.button.onClick.AddListener(() => OnLevelButtonClick(index));
                }
            }
            UpdateButtonStates();
        }

        /// <summary>
        /// 根据 PassedLevelIndex 更新按钮状态：已通关及之前的关卡可点击，否则不可点击并变暗
        /// </summary>
        private void UpdateButtonStates()
        {
            int passedIndex = levelProgressModel?.PassedLevelIndex ?? -1;
            for (int i = 0; i < levelList.Count; i++)
            {
                Level lvl = levelList[i];
                if (lvl.button != null)
                {
                    bool canClick = i <= passedIndex;
                    lvl.button.interactable = canClick;
                   
                }
            }
        }

        public void ShowAllLevelCanvas()
        {
            basicCanvas.SetActive(false);
            if (allLevelCanvas != null)
            {
                allLevelCanvas.gameObject.SetActive(true);
            }
            RefreshButtonStatesOnShow();
            BindingLevelInfoToPrefab();
        }
        
        public void HideAllLevelCanvas()
        {
            if (allLevelCanvas != null)
            {
                allLevelCanvas.gameObject.SetActive(false);
            }
            basicCanvas.SetActive(true);
        }

       

        private void OnLevelButtonClick(int index)
        {
            LoadLevel(index);
        }

        public void LoadLevel(int index)
        {
            if (isLoadingLevel)
            {
                return;
            }
            isLoadingLevel = true;
            levelSystem.LoadLevel(index);
        }

        public void LoadNextLevel()
        {
            int next = levelModel.CurrentLevelIndex + 1;
            if (next < levelModel.LevelList.Count)
            {
                LoadLevel(next);
            }
            else
            {
                Debug.Log("LoadNextLevel: 已经是最后一关");
            }
        }

        public void LoadPreviousLevel()
        {
            int prev = levelModel.CurrentLevelIndex - 1;
            if (prev >= 0)
            {
                LoadLevel(prev);
            }
            else
            {
                Debug.Log("LevelManager: 已经是第一关");
            }
        }

        public void ReloadCurrentLevel()
        {
            LoadLevel(levelModel.CurrentLevelIndex);
        }

        
        public void RefreshButtons()
        {
            this.GetSystem<ILevelProgressSystem>(); // 确保进度已加载
            InitializeButtons();
        }

        /// <summary>
        /// 打开关卡选择时刷新按钮状态（进度可能已更新）
        /// </summary>
        public void RefreshButtonStatesOnShow()
        {
            UpdateButtonStates();
        }

        public void BindingLevelInfoToPrefab(){
            if (levelItemPrefab == null || groupForButton == null) return;
           //delete existed old items
            for (int i = groupForButton.childCount - 1; i >= 0; i--)
            {
                Destroy(groupForButton.GetChild(i).gameObject);
            }
            for (int i = 0; i < levelList.Count; i++)
            {
                int index = i;
                GameObject item = Instantiate(levelItemPrefab, groupForButton);
                item.transform.localScale = Vector3.one;
                levelItem levelItem = item.GetComponent<levelItem>();
                levelItem.setUp(levelList[i]);
                levelItem.button.onClick.AddListener(() => OnLevelButtonClick(index));
                levelItem.GetComponentInChildren<Image>().sprite = levelList[i].image;
                levelItem.button.GetComponentInChildren<TextMeshProUGUI>().text = levelList[i].Name;
                
                
            }
        }
    }
}


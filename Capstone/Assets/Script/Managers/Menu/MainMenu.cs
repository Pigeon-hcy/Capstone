using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QFramework;
using SkateGame;

public class MainMenu : MonoBehaviour, IBelongToArchitecture, ICanGetModel, ICanGetSystem
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    public List<Level> levelList = new List<Level>();

    public GameObject levelSelectCanvas;
    public GameObject defaultCanvas;

    
    [Header("主界面背景：按进度显示对应 Image。0: 1-1~1-3, 1: 1-4~1-7, 2: 1-8~1-10")]
    public List<Image> backgroundImageList = new List<Image>();

    /// <summary>
    /// Get background index from player progress. 1-1~1-3 -> 0, 1-4~1-7 -> 1, 1-8~1-10 -> 2.
    /// </summary>
    private int GetBackgroundIndexByProgress(int passedLevelIndex)
    {
        if (passedLevelIndex < 0) return 0;
        if (passedLevelIndex <= 2) return 0;   // 1-1 to 1-3
        if (passedLevelIndex <= 6) return 1;  // 1-4 to 1-7
        return 2;                              // 1-8 to 1-10
    }

    private void Start()
    {
        ApplyBackgroundByProgress();
    }

    private void ApplyBackgroundByProgress()
    {
        if (backgroundImageList == null || backgroundImageList.Count == 0)
            return;
        this.GetSystem<ILevelProgressSystem>(); // ensure progress loaded
        var levelProgressModel = this.GetModel<ILevelProgressModel>();
        int passed = levelProgressModel != null ? levelProgressModel.PassedLevelIndex : -1;
        int bgIndex = GetBackgroundIndexByProgress(passed);
        bgIndex = Mathf.Clamp(bgIndex, 0, backgroundImageList.Count - 1);
        for (int i = 0; i < backgroundImageList.Count; i++)
        {
            if (backgroundImageList[i] != null)
                backgroundImageList[i].gameObject.SetActive(i == bgIndex);
        }
    }
    private void EnsureLevelModelPopulated()
    {
        var levelModel = this.GetModel<ILevelModel>();
        if (levelModel.LevelList != null && levelModel.LevelList.Count > 0) return;
        if (levelList == null || levelList.Count == 0) return;
        var levelSystem = this.GetSystem<ILevelSystem>();
        var levelProgressModel = this.GetModel<ILevelProgressModel>();
        foreach (var lvl in levelList)
            levelSystem.AddLevel(lvl);
        int passed = levelProgressModel != null ? levelProgressModel.PassedLevelIndex : 0;
        levelModel.CurrentLevelIndex = Mathf.Clamp(passed, 0, levelModel.LevelList.Count - 1);
    }

    /// <summary>
    /// 继续当前关卡：加载当前进度对应的关卡场景。按钮绑定此方法即可。
    /// </summary>
    public void OnClickContinueCurrentLevel()
    {
        EnsureLevelModelPopulated();
        var levelModel = this.GetModel<ILevelModel>();
        if (levelModel.LevelList == null || levelModel.LevelList.Count == 0) return;
        int idx = Mathf.Clamp(levelModel.CurrentLevelIndex, 0, levelModel.LevelList.Count - 1);
        string sceneName = levelModel.LevelList[idx].SceneName;
        if (string.IsNullOrEmpty(sceneName)) return;
        if (GameStateController.Instance != null)
            GameStateController.Instance.EnterInGame();
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickStart()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.EnterInGame();
        SceneManager.LoadScene("New_1-1");
    }

    public void ShowLevelSelectCanvas()
    {
        EnsureLevelModelPopulated();
        levelSelectCanvas.SetActive(true);
        defaultCanvas.SetActive(false);
    }

    public void HideLevelSelectCanvas()
    {
        levelSelectCanvas.SetActive(false);
        defaultCanvas.SetActive(true);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

}

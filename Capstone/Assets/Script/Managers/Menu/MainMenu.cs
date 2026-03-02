using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using QFramework;
using SkateGame;

public class MainMenu : MonoBehaviour, IBelongToArchitecture, ICanGetModel, ICanGetSystem
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    public List<Level> levelList = new List<Level>();

    public GameObject levelSelectCanvas;
    public GameObject defaultCanvas;

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
}

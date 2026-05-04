using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using SkateGame;

public class MainMenu : MonoBehaviour, IBelongToArchitecture, ICanGetModel, ICanGetSystem
{
    public IArchitecture GetArchitecture() => GameApp.Interface;

    public GameObject levelSelectCanvas;
    public GameObject defaultCanvas;
    public LevelManager levelManager;

    
    [Header("主界面背景：按最近一次通关关卡显示对应 Image。0: 1-1~1-3, 1: 1-4~1-7, 2: 1-8~1-10")]
    public List<Image> backgroundImageList = new List<Image>();

    /// <summary>
    /// Get background index from last cleared level. 1-1~1-3 -> 0, 1-4~1-7 -> 1, 1-8~1-10 -> 2.
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
        int lastCleared = levelProgressModel != null ? levelProgressModel.LastClearedLevelIndex : -1;
        int bgIndex = GetBackgroundIndexByProgress(lastCleared);
        bgIndex = Mathf.Clamp(bgIndex, 0, backgroundImageList.Count - 1);
        for (int i = 0; i < backgroundImageList.Count; i++)
        {
            if (backgroundImageList[i] != null)
                backgroundImageList[i].gameObject.SetActive(i == bgIndex);
        }
    }
    /// <summary>
    /// 继续当前关卡：加载当前进度对应的关卡场景。按钮绑定此方法即可。
    /// </summary>
    // FOR JERRY'S AUDIO - UI CLICK
    public void OnClickContinueCurrentLevel()
    {
        if (levelManager == null) return;
        var levelModel = this.GetModel<ILevelModel>();
        if (levelModel.LevelList == null || levelModel.LevelList.Count == 0) return;
        int idx = Mathf.Clamp(levelModel.CurrentLevelIndex, 0, levelModel.LevelList.Count - 1);
        levelManager.LoadLevel(idx);
    }

    // FOR JERRY'S AUDIO - UI CLICK
    public void OnClickStart()
    {
        if (levelManager == null) return;
        levelManager.LoadLevel(0);
    }

    public void ShowLevelSelectCanvas()
    {
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

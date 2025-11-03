using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using MoreMountains.Feedbacks;
using Febucci.UI;
namespace SkateGame
{
    /// <summary>
    /// 对话查看器
    /// 负责显示对话内容并处理交互
    /// </summary>
    public class DialogueViewer : ViewerControllerBase
    {
        [Header("对话设置")]
        public string NameForDialogue;
        
        [Header("UI组件")]
        public TextMeshProUGUI text;
        public Image image;
        public TextAnimator_TMP textAnimatorPlayer;

        [Header("对话列表")]
        public List<DialogueObj> dialogueList = new List<DialogueObj>();
        
        [Header("点击按钮")]
        public Button clickButton;
        public MMF_Player clickButtonFadeIn;
        public MMF_Player clickButtonFadeOut;
        
        [Header("选项按钮")]
        public Button[] buttons = new Button[3];
        public MMF_Player[] buttonFadeIns = new MMF_Player[3];
        public MMF_Player[] buttonFadeOuts = new MMF_Player[3];
        
        [Header("对话框动画")]
        public MMF_Player mmfPlayersFadeIn;
        public MMF_Player mmfPlayersFadeOut;
        
        // 私有变量
        private List<DialogueObj> ThisDialogueList;
        private int current = 0;
        private int lastCurrent = -1; // 记录上一次的对话索引
        private IDialogueSystem dialogueSystem;
        private IDialogueModel dialogueModel;
        
        protected override void InitializeController()
        {
            base.InitializeController();
            
            // 获取系统和模型
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            dialogueModel = this.GetModel<IDialogueModel>();
            
            // 从system匹配对话列表
            ThisDialogueList = dialogueSystem.Match(NameForDialogue);
            
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
            {
                Debug.LogError($"DialogueViewer [{NameForDialogue}]: 未匹配到对话列表！");
                return;
            }
            
            // 设置按钮点击事件
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(Click);
            }
            
            Debug.Log($"DialogueViewer [{NameForDialogue}]: 初始化完成，共 {ThisDialogueList.Count} 条对话");
        }
        
        protected override void OnRealTimeUpdate()
        {
            // 实时更新显示
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
                return;
            
            // 确保索引有效
            if (current < 0 || current >= ThisDialogueList.Count)
                current = 0;
            
            // 检测对话是否改变
            bool dialogueChanged = (current != lastCurrent);
            
            // 如果对话变了
            if (dialogueChanged)
            {
                // 播放对话特效
                if (mmfPlayersFadeIn != null)
                    mmfPlayersFadeIn.PlayFeedbacks();
                
                // 更新文本（使用 TextAnimator 打字机效果）
                if (textAnimatorPlayer != null)
                    textAnimatorPlayer.SetText(ThisDialogueList[current].text);
                
                // 更新图片
                if (image != null)
                    image.sprite = ThisDialogueList[current].image;
                
                // 判断当前对话是否有选项
                bool hasChoices = ThisDialogueList[current].hasChoices;
                
                if (hasChoices)
                {
                    // 有选项：显示选项按钮，播放选项按钮特效
                    if (clickButton != null)
                        clickButton.gameObject.SetActive(false);
                    
                    for (int i = 0; i < 3; i++)
                    {
                        if (buttons != null && i < buttons.Length && buttons[i] != null)
                        {
                            buttons[i].gameObject.SetActive(true);
                            if (buttonFadeIns != null && i < buttonFadeIns.Length && buttonFadeIns[i] != null)
                                buttonFadeIns[i].PlayFeedbacks();
                        }
                    }
                    
                    // 更新选项按钮跳转
                    ClickWithOptions();
                }
                else
                {
                    // 无选项：显示普通按钮，播放普通按钮特效
                    if (clickButton != null)
                    {
                        clickButton.gameObject.SetActive(true);
                        if (clickButtonFadeIn != null)
                            clickButtonFadeIn.PlayFeedbacks();
                    }
                    
                    for (int i = 0; i < 3; i++)
                    {
                        if (buttons != null && i < buttons.Length && buttons[i] != null)
                            buttons[i].gameObject.SetActive(false);
                    }
                }
                
                lastCurrent = current;
            }
            
            EndDialogue();
        }
        
        /// <summary>
        /// 点击切换对话
        /// </summary>
        public void Click()
        {
            if (dialogueModel.TableForDialogue.ContainsKey(NameForDialogue))
            {
                int totalCount = dialogueModel.TableForDialogue[NameForDialogue].Count;
                current = (current + 1) % totalCount;
                Debug.Log($"DialogueViewer [{NameForDialogue}]: 切换到第 {current + 1}/{totalCount} 条");
            }
        }
        
        /// <summary>
        /// 设置选项按钮跳转
        /// </summary>
        public void ClickWithOptions()
        {
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
                return;
                
            var currentJumpingList = ThisDialogueList[current].indexForJump;
            
            for (int i = 0; i < 3; i++)
            {
                if (buttons != null && i < buttons.Length && buttons[i] != null)
                {
                    int index = i; // 捕获索引
                    buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = ThisDialogueList[current].buttonTexts[i];
                    buttons[i].onClick.RemoveAllListeners();
                    buttons[i].onClick.AddListener(() => 
                    {
                        if (currentJumpingList != null && index < currentJumpingList.Length)
                        {
                            current = currentJumpingList[index];
                            Debug.Log($"DialogueViewer [{NameForDialogue}]: 选项 {index + 1} 跳转到第 {current + 1} 条");
                        }
                    });
                }
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(Click);
            }
        }
        /// <summary>
        /// 结束对话，如果是最后一条则隐藏对话框
        /// </summary>
        public void EndDialogue()
        {
            if (dialogueModel.TableForDialogue.ContainsKey(NameForDialogue))
            {
                if (current == dialogueModel.TableForDialogue[NameForDialogue].Count - 1)
                {
                    // 隐藏对话框GameObject
                    mmfPlayersFadeOut.PlayFeedbacks();
                    this.gameObject.SetActive(false);
                    Debug.Log($"DialogueViewer [{NameForDialogue}]: 对话结束，已隐藏");
                }
            }
        }
    }
}

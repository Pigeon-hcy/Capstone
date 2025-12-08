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
        public int current = 0;
        private int lastCurrent = -1; // 记录上一次的对话索引
        private IDialogueSystem dialogueSystem;
        private IDialogueModel dialogueModel;
        public int CurrentIndex => current;
        public IReadOnlyList<DialogueObj> CurrentDialogueList => ThisDialogueList;
        public bool IsDialogueFinished => ThisDialogueList != null && ThisDialogueList.Count > 0 && current >= ThisDialogueList.Count - 1;
        
        // 玩家控制相关（用于对话时强制速度为0）
        private PlayerController playerController;
        private Rigidbody2D playerRb;
        private Vector2 savedVelocity; // 保存对话前的速度
        
        protected override void InitializeController()
        {
            base.InitializeController();
            
            // 获取系统和模型
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            dialogueModel = this.GetModel<IDialogueModel>();
            
            // 获取玩家控制器和 Rigidbody2D
            playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerRb = playerController.GetComponent<Rigidbody2D>();
            }
            
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
            
            // 初始化时立即显示第一条对话
            current = 0;
            lastCurrent = -1; // 确保第一次会触发更新
            UpdateDialogueDisplay();
            
            Debug.Log($"DialogueViewer [{NameForDialogue}]: 初始化完成，共 {ThisDialogueList.Count} 条对话");
        }
        
        /// <summary>
        /// 当对话UI激活时，强制玩家速度为0
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            FreezePlayerMovement();
        }
        
        /// <summary>
        /// 当对话UI禁用时，恢复玩家速度
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            UnfreezePlayerMovement();
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
            
            // 如果对话变了，更新显示
            if (dialogueChanged)
            {
                UpdateDialogueDisplay();
                lastCurrent = current;
            }
            
            // 对话进行中，持续强制玩家速度为0
            if (gameObject.activeSelf && playerRb != null)
            {
                // 强制水平速度为0，保持垂直速度（允许重力作用）
                Vector2 currentVelocity = playerRb.linearVelocity;
                playerRb.linearVelocity = new Vector2(0f, currentVelocity.y);
            }
        }
        
        /// <summary>
        /// 更新对话显示（文本、图片、按钮等）
        /// </summary>
        private void UpdateDialogueDisplay()
        {
            if (ThisDialogueList == null || ThisDialogueList.Count == 0 || current < 0 || current >= ThisDialogueList.Count)
                return;
            
            // 播放对话特效
            if (mmfPlayersFadeIn != null)
                mmfPlayersFadeIn.PlayFeedbacks();
            
            // 更新文本
            string dialogueText = ThisDialogueList[current].text;
            
            // 如果使用 TextAnimator，设置动画文本
            if (textAnimatorPlayer != null)
            {
                textAnimatorPlayer.SetText(dialogueText);
            }
            
            // 同时直接更新 TextMeshProUGUI（确保文本显示）
            if (text != null)
            {
                text.text = dialogueText;
            }
            
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
        }
        
        /// <summary>
        /// 冻结玩家移动（对话开始时调用）
        /// </summary>
        private void FreezePlayerMovement()
        {
            if (playerRb != null)
            {
                // 保存当前速度
                savedVelocity = playerRb.linearVelocity;
                // 强制水平速度为0，保持垂直速度（允许重力作用，防止玩家掉下去）
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
                Debug.Log($"DialogueViewer [{NameForDialogue}]: 冻结玩家移动");
            }
        }
        
        /// <summary>
        /// 恢复玩家移动（对话结束时调用）
        /// </summary>
        private void UnfreezePlayerMovement()
        {
            if (playerRb != null)
            {
                // 恢复水平速度（垂直速度由物理系统控制）
                playerRb.linearVelocity = new Vector2(savedVelocity.x, playerRb.linearVelocity.y);
                Debug.Log($"DialogueViewer [{NameForDialogue}]: 恢复玩家移动");
            }
        }
        
        /// <summary>
        /// 点击切换对话
        /// </summary>
        public void Click()
        {
           
            if (current >= ThisDialogueList.Count - 1)
            {
                // 对话结束，关闭对话框并让NPC消失
                EndDialogue();
                return;
            }
            
            // 继续切换到下一条
            current++;
        
        }
        
        /// <summary>
        /// 结束对话：关闭对话框并让NPC（父物体）消失
        /// </summary>
        private void EndDialogue()
        {
            // 播放关闭动画（如果有）
            if (mmfPlayersFadeOut != null)
            {
                mmfPlayersFadeOut.PlayFeedbacks();
                // 等待动画播放完成后再隐藏
                StartCoroutine(HideAfterFadeOut());
            }
            else
            {
                // 没有关闭动画，直接隐藏
                HideDialogueAndNPC();
            }
        }
        
        /// <summary>
        /// 等待淡出动画完成后隐藏
        /// </summary>
        private System.Collections.IEnumerator HideAfterFadeOut()
        {
            // 等待淡出动画播放完成（可以根据实际动画时长调整）
            if (mmfPlayersFadeOut != null)
            {
                yield return new WaitForSeconds(mmfPlayersFadeOut.TotalDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.5f); // 默认等待0.5秒
            }
            
            HideDialogueAndNPC();
        }
        
        /// <summary>
        /// 隐藏对话框和NPC（父物体）
        /// </summary>
        private void HideDialogueAndNPC()
        {
            // 先隐藏对话框
            this.gameObject.SetActive(false);
            
            // 获取canvas的父物体（NPC）
            Transform parent = transform.parent;
            if (parent != null)
            {
                // 让NPC消失
                parent.gameObject.SetActive(false);
                Debug.Log($"DialogueViewer [{NameForDialogue}]: NPC已消失");
            }
            else
            {
                Debug.LogWarning($"DialogueViewer [{NameForDialogue}]: 未找到父物体（NPC）");
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
        
     
        /// <summary>
        /// 结束对话，如果是最后一条则隐藏对话框
        /// </summary>
        
    }
}

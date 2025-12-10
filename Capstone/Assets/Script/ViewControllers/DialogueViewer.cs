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
        private int lastCurrent = -1;
        private IDialogueSystem dialogueSystem;
        private IDialogueModel dialogueModel;

        public int CurrentIndex => current;
        public IReadOnlyList<DialogueObj> CurrentDialogueList => ThisDialogueList;
        public bool IsDialogueFinished => ThisDialogueList != null && ThisDialogueList.Count > 0 && current >= ThisDialogueList.Count - 1;
        
        // 玩家控制相关
        private PlayerController playerController;
        private Rigidbody2D playerRb;
        private Vector2 savedVelocity;

        protected override void InitializeController()
        {
            base.InitializeController();
            
            // 获取系统与模型
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            dialogueModel = this.GetModel<IDialogueModel>();
            
            // 获取玩家
            playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                playerRb = playerController.GetComponent<Rigidbody2D>();
            
            // 匹配对话列表
            ThisDialogueList = dialogueSystem.Match(NameForDialogue);
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
            {
                Debug.LogError($"DialogueViewer [{NameForDialogue}]: 未匹配到对话列表！");
                return;
            }
            
            // 点击按钮绑定
            if (clickButton != null)
                clickButton.onClick.AddListener(Click);

            // 初始对话显示
            current = 0;
            lastCurrent = -1;
            UpdateDialogueDisplay();
        }

        // 对话开启时调用
        protected override void OnEnable()
        {
            base.OnEnable();
            FreezePlayerMovementAndInput();
        }

        // 对话关闭时调用
        protected override void OnDisable()
        {
            base.OnDisable();
            UnfreezePlayerMovementAndInput();
        }

        protected override void OnRealTimeUpdate()
        {
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
                return;

            if (current < 0 || current >= ThisDialogueList.Count)
                current = 0;

            if (current != lastCurrent)
            {
                UpdateDialogueDisplay();
                lastCurrent = current;
            }

            // 强制水平速度为 0，避免玩家滑动
            if (gameObject.activeSelf && playerRb != null)
            {
                Vector2 v = playerRb.linearVelocity;
                playerRb.linearVelocity = new Vector2(0, v.y);
            }
        }

        // 更新对话内容显示
        private void UpdateDialogueDisplay()
        {
            if (ThisDialogueList == null || ThisDialogueList.Count == 0)
                return;

            if (mmfPlayersFadeIn != null)
                mmfPlayersFadeIn.PlayFeedbacks();

            string dialogueText = ThisDialogueList[current].text;

            if (textAnimatorPlayer != null)
                textAnimatorPlayer.SetText(dialogueText);

            if (text != null)
                text.text = dialogueText;

            if (image != null)
                image.sprite = ThisDialogueList[current].image;

            bool hasChoices = ThisDialogueList[current].hasChoices;

            if (hasChoices)
            {
                if (clickButton != null)
                    clickButton.gameObject.SetActive(false);

                for (int i = 0; i < 3; i++)
                {
                    buttons[i].gameObject.SetActive(true);
                    buttonFadeIns[i]?.PlayFeedbacks();
                }

                ClickWithOptions();
            }
            else
            {
                if (clickButton != null)
                {
                    clickButton.gameObject.SetActive(true);
                    clickButtonFadeIn?.PlayFeedbacks();
                }

                for (int i = 0; i < 3; i++)
                    buttons[i].gameObject.SetActive(false);
            }
        }

        // 冻结玩家移动 + 禁止射击
        private void FreezePlayerMovementAndInput()
        {
            if (playerRb != null)
            {
                savedVelocity = playerRb.linearVelocity;
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
            }

            if (playerController != null)
            {
                playerController.disableInput = true;  // 禁止射击/瞄准
            }
        }

        // 恢复玩家移动 + 恢复射击
        private void UnfreezePlayerMovementAndInput()
        {
            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(savedVelocity.x, playerRb.linearVelocity.y);
            }

            if (playerController != null)
            {
                playerController.disableInput = false;  // 恢复射击/瞄准
            }
        }

        // 点击对话继续
        public void Click()
        {
            if (current >= ThisDialogueList.Count - 1)
            {
                EndDialogue();
                return;
            }

            current++;
        }

        private void EndDialogue()
        {
            if (mmfPlayersFadeOut != null)
            {
                mmfPlayersFadeOut.PlayFeedbacks();
                StartCoroutine(HideAfterFadeOut());
            }
            else
            {
                HideDialogueAndNPC();
            }
        }

        private System.Collections.IEnumerator HideAfterFadeOut()
        {
            yield return new WaitForSeconds(mmfPlayersFadeOut != null ? mmfPlayersFadeOut.TotalDuration : 0.5f);
            HideDialogueAndNPC();
        }

        private void HideDialogueAndNPC()
        {
            this.gameObject.SetActive(false);

            Transform parent = transform.parent;
            if (parent != null)
                parent.gameObject.SetActive(false);
        }

        // 设置选项按钮跳转
        public void ClickWithOptions()
        {
            var currentJumpingList = ThisDialogueList[current].indexForJump;

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = ThisDialogueList[current].buttonTexts[i];
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() =>
                {
                    current = currentJumpingList[index];
                });
            }
        }
    }
}

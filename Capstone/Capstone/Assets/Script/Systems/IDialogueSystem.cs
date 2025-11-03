using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace SkateGame
{
    /// <summary>
    /// 对话系统接口
    /// </summary>
    public interface IDialogueSystem : ISystem
    {
        /// <summary>
        /// 匹配对话列表
        /// </summary>
        List<DialogueObj> Match(string name);
    }

    /// <summary>
    /// 对话系统实现
    /// </summary>
    public class DialogueSystem : AbstractSystem, IDialogueSystem
    {
        private IDialogueModel dialogueModel;
        
        protected override void OnInit()
        {
            dialogueModel = this.GetModel<IDialogueModel>();
            
            // 自动捕捉全局所有DialogueViewer，提取list和name放入dictionary
            InitDialogues();
        }
        
        /// <summary>
        /// 初始化：查找所有DialogueViewer，提取它们的list和name放到dictionary中
        /// </summary>
        private void InitDialogues()
        {
            // 查找场景中所有的DialogueViewer
            var dialogueViewers = Object.FindObjectsByType<DialogueViewer>(FindObjectsSortMode.None);
            
            if (dialogueViewers == null || dialogueViewers.Length == 0)
            {
                Debug.Log("DialogueSystem: 场景中没有找到DialogueViewer");
                return;
            }
            
            Debug.Log($"DialogueSystem: 找到 {dialogueViewers.Length} 个DialogueViewer，开始自动加载");
            
            // 遍历所有DialogueViewer，提取name和dialogueList
            foreach (var viewer in dialogueViewers)
            {
                if (viewer != null && !string.IsNullOrEmpty(viewer.NameForDialogue))
                {
                    if (viewer.dialogueList != null && viewer.dialogueList.Count > 0)
                    {
                        // 直接放入model的TableForDialogue
                        if (!dialogueModel.TableForDialogue.ContainsKey(viewer.NameForDialogue))
                        {
                            dialogueModel.TableForDialogue.Add(viewer.NameForDialogue, viewer.dialogueList);
                            Debug.Log($"DialogueSystem: 自动加载对话 [{viewer.NameForDialogue}]，共 {viewer.dialogueList.Count} 条");
                        }
                        else
                        {
                            Debug.LogWarning($"DialogueSystem: 对话名称 [{viewer.NameForDialogue}] 重复");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 匹配对话列表
        /// </summary>
        public List<DialogueObj> Match(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("DialogueSystem.Match: 对话名称为空");
                return null;
            }
            
            // 从model的TableForDialogue获取
            if (dialogueModel.TableForDialogue.ContainsKey(name))
            {
                var matchedList = dialogueModel.TableForDialogue[name];
                Debug.Log($"DialogueSystem.Match: 匹配到对话 [{name}]");
                return matchedList;
            }
            
            Debug.LogWarning($"DialogueSystem.Match: 未找到对话 [{name}]");
            return null;
        }
    }
}

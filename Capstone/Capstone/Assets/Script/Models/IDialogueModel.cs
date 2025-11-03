using System.Collections.Generic;
using QFramework;

namespace SkateGame
{
    /// <summary>
    /// 对话模型接口 - 只存储数据
    /// </summary>
    public interface IDialogueModel : IModel
    {
        /// <summary>
        /// 对话列表
        /// </summary>
        List<DialogueObj> DialogueList { get; set; }
        
        /// <summary>
        /// 对话表：键为对话名称，值为对话列表
        /// </summary>
        Dictionary<string, List<DialogueObj>> TableForDialogue { get; set; }
    }

    /// <summary>
    /// 对话模型实现 - 只存储数据，不写方法
    /// </summary>
    public class DialogueModel : AbstractModel, IDialogueModel
    {
        public List<DialogueObj> DialogueList { get; set; }
        public Dictionary<string, List<DialogueObj>> TableForDialogue { get; set; }
        
        protected override void OnInit()
        {
            DialogueList = new List<DialogueObj>();
            TableForDialogue = new Dictionary<string, List<DialogueObj>>();
        }
    }
}



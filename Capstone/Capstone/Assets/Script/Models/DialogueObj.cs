using System;
using UnityEngine;

namespace SkateGame
{
    /// <summary>
    /// 对话对象，包含单个对话的文本和图片
    /// </summary>
    [Serializable]
    public class DialogueObj
    {
        [Header("对话内容")]
        [TextArea(3, 10)]
        public string text;
        
        [Header("对话图片")]
        public Sprite image;
        
        [Header("选项跳转")]
        public bool hasChoices;
        public int[] indexForJump;
        public string[] buttonTexts;
        
        public DialogueObj()
        {
            text = "";
            image = null;
            hasChoices = false;
            indexForJump = new int[3];
            buttonTexts = new string[3];
        }
        
        public DialogueObj(string text, Sprite image)
        {
            this.text = text;
            this.image = image;
            hasChoices = false;
            indexForJump = new int[3];
            buttonTexts = new string[3];
        }
    }
}



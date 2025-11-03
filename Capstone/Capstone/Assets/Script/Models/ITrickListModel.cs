using QFramework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace SkateGame
{
    public interface ITrickListModel : IModel
    {
        BindableProperty<List<TrickState>> TrickList { get; }
        BindableProperty<char> Grade { get; }
        
    }

    public class TrickListModel : AbstractModel, ITrickListModel
    {
        public BindableProperty<List<TrickState>> TrickList { get; } = new BindableProperty<List<TrickState>>(new List<TrickState>());
        public BindableProperty<char> Grade { get; } = new BindableProperty<char>('D');

        protected override void OnInit()
        {
            // 初始化逻辑
        }
    
      

        
    }
}
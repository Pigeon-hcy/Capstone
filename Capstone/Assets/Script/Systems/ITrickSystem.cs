using QFramework;
using UnityEngine;
using System.Collections.Generic;

namespace SkateGame
{
    public interface ITrickSystem : ISystem
    {
        void AddTrick(TrickState trick);
        void RemoveAllTricks();
        BindableProperty<List<TrickState>> TrickList { get; }
        void printTrickList();
    }

    public class TrickSystem : AbstractSystem, ITrickSystem, ICanSendEvent
    {
        private ITrickListModel trickListModel;

        public BindableProperty<List<TrickState>> TrickList => trickListModel.TrickList;

        protected override void OnInit()
        {
            trickListModel = this.GetModel<ITrickListModel>();
        }

        public void AddTrick(TrickState trick)
        {
            if (trick == null) return;
            trickListModel.TrickList.Value.Add(trick);
            this.SendEvent(new TrickListChangedEvent { LatestTrick = trick });
        }

        public void RemoveAllTricks()
        {
            trickListModel.TrickList.Value.Clear();
        }

        public void printTrickList()
        {
            foreach (TrickState trick in trickListModel.TrickList.Value)
                Debug.Log("System print trick list: " + trick.GetType().Name);
        }
    }
}

using QFramework;
using System.Collections.Generic;

namespace SkateGame
{
    public interface ITrickListModel : IModel
    {
        BindableProperty<List<TrickState>> TrickList { get; }
    }

    public class TrickListModel : AbstractModel, ITrickListModel
    {
        public BindableProperty<List<TrickState>> TrickList { get; } = new BindableProperty<List<TrickState>>(new List<TrickState>());

        protected override void OnInit() { }
    }
}

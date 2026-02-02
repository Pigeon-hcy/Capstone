using QFramework;
using UnityEngine;

namespace SkateGame
{
	public interface IInputModel : IModel
	{
		BindableProperty<Vector2> Move { get; }
		BindableProperty<bool> JumpStart { get; }
		BindableProperty<bool> Grind { get; }
		BindableProperty<bool> GrindStart { get; }
		BindableProperty<bool> SwitchItem { get; }
		BindableProperty<bool> Grab { get; }
		BindableProperty<bool> GrabStart { get; }
		BindableProperty<bool> Dash { get; }
		BindableProperty<bool> DashStart { get; }
		BindableProperty<bool> Push { get; }
		BindableProperty<bool> PushStart { get; }
		BindableProperty<bool> Brake { get; }
		BindableProperty<bool> BrakeStart { get; }
		BindableProperty<bool> ShootStart { get; }
		BindableProperty<bool> ShootEnd { get; }
	}

	public class InputModel : AbstractModel, IInputModel
	{
		public BindableProperty<Vector2> Move { get; } = new BindableProperty<Vector2>(Vector2.zero);
		public BindableProperty<bool> JumpStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Grind { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> GrindStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> SwitchItem { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Grab { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> GrabStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Dash { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> DashStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Slam { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> SlamStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Push { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> PushStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> Brake { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> BrakeStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> ShootStart { get; } = new BindableProperty<bool>(false);
		public BindableProperty<bool> ShootEnd { get; } = new BindableProperty<bool>(false);
		protected override void OnInit()
		{
		}
	}
}



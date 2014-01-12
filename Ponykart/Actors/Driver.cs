using Mogre;
using Ponykart.Players;
using PonykartParsers;

namespace Ponykart.Actors {
	public class Driver : LThing {

		public Kart Kart { get; set; }
		public Player Player { get; set; }

		public Driver(ThingBlock block, ThingDefinition def) : base(block, def) {
		}

		public void AttachToKart(Kart kart, Vector3 offset) {
			kart.Driver = this;
			this.Kart = kart;

			this.RootNode.Parent.RemoveChild(this.RootNode);
			kart.RootNode.AddChild(this.RootNode);

			this.RootNode.Position = offset;
			this.RootNode.Orientation = Quaternion.IDENTITY;
			this.RootNode.SetInitialState();
		}

		public override void ChangeAnimation(string animationName) {
			ChangeAnimation(animationName, AnimationBlendingTransition.BlendSwitch);
		}

		public void ChangeAnimation(string animationName, AnimationBlendingTransition transition, float duration = 0.2f) {
			ModelComponents[0].AnimationBlender.Blend(animationName, transition, duration, true);
		}

		public void ChangeAnimation(DriverAnimation anim, AnimationBlendingTransition transition = AnimationBlendingTransition.BlendWhileAnimating, float duration = 0.2f) {
			ChangeAnimation(anim.ToString(), transition, duration);
		}

		/// <summary>
		/// The same as the other change animation methods, but this one only runs successfully if it is currently not blending an animation.
		/// If it is, nothing happens.
		/// </summary>
		public void ChangeAnimationIfNotBlending(DriverAnimation anim, AnimationBlendingTransition transition = AnimationBlendingTransition.BlendWhileAnimating, float duration = 0.2f) {
			if (ModelComponents[0].AnimationBlender.Complete)
				ChangeAnimation(anim, transition, duration);
		}
	}

	public enum DriverAnimation {
		Drive,
		TurnLeft,
		TurnRight,
		Idle,
		Stand,
		Reverse,
		DriftLeft,
		DriftRight,
		Basis,

		_Unknown_,
	}
}

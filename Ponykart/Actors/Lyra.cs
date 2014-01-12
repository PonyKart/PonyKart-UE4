using System;
using System.Threading;
using Mogre;
using Ponykart.Core;
using Ponykart.Players;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// For lyra's sitting model. Mostly the same as the BackgroundPony class, but more watered down
	/// </summary>
	public class Lyra : LThing {
		protected ModelComponent bodyComponent, maneComponent, tailComponent;
		protected AnimationState blinkState;
		protected System.Threading.Timer animTimer;
		protected const float BLEND_TIME = 1f;
		// milliseconds
		protected const int ANIMATION_TIMESPAN_MINIMUM = 5000, ANIMATION_TIMESPAN_MAXIMUM = 8000;
		protected Random random;
		protected Euler neckFacing;
		protected Bone neckbone;
		protected Kart followKart;

		public Lyra(ThingBlock block, ThingDefinition def) : base(block, def) {
			foreach (ModelComponent mc in ModelComponents) {
				if (mc.Name.EndsWith("Body"))
					bodyComponent = mc;
				else if (mc.Name.EndsWith("Mane"))
					maneComponent = mc;
				else if (mc.Name.EndsWith("Tail"))
					tailComponent = mc;
			}

			// make sure our animations add their weights and don't just average out. The AnimationBlender already handles averaging between two anims.
			Skeleton skeleton = bodyComponent.Entity.Skeleton;
			skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;

			// set up the blink animation state with some stuff
			blinkState = bodyComponent.Entity.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1;
			blinkState.AddTime(ID);

			// set up all of the animation states to not use the neck bone
			neckbone = skeleton.GetBone("Neck");
			neckbone.SetManuallyControlled(true);
			foreach (var state in bodyComponent.Entity.AllAnimationStates.GetAnimationStateIterator()) {
				// don't add a blend mask to the blink state because we'll make a different one for it
				if (state == blinkState)
					continue;

				state.CreateBlendMask(skeleton.NumBones);
				state.SetBlendMaskEntry(neckbone.Handle, 0f);
			}
			neckbone.InheritOrientation = false;

			neckFacing = new Euler(0, 0, 0);

			// set up a blend mask so only the eyebrow bones have any effect on the blink animation
			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			// add the blink state to the animation manager so it has time added to it
			LKernel.GetG<AnimationManager>().Add(blinkState);

			// set up some timers to handle animation changing
			random = new Random(IDs.Random);
			animTimer = new System.Threading.Timer(new TimerCallback(AnimTimerTick), null, random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);

			// add a bit of time to things so the animations aren't all synced at the beginning
			float rand = (float) random.NextDouble();
			bodyComponent.AnimationBlender.AddTime(rand);
			tailComponent.AnimationState.AddTime(rand);

			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		private readonly Radian NECK_YAW_LIMIT = new Degree(70f);
		private readonly Radian NECK_PITCH_LIMIT = new Degree(60f);

		/// <summary>
		/// Rotate the neck bone to face the kart. Will eventually need to redo this when we have multiple karts, to face whichever's nearest, etc.
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!Pauser.IsPaused) {
				Vector3 lookat = RootNode.ConvertWorldToLocalPosition(followKart.ActualPosition);
				// temp is how much you need to rotate to get from the current orientation to the new orientation
				// we use -lookat because our bone points towards +Z, whereas this code was originally made for things facing towards -Z
				Euler temp = neckFacing.GetRotationTo(-lookat, true, true, true);
				// limit the offset so the head turns at a maximum of 3 radians per second
				Radian tempTime = new Radian(evt.timeSinceLastFrame * 3);
				temp.LimitYaw(tempTime);
				temp.LimitPitch(tempTime);

				neckFacing = neckFacing + temp;
				neckFacing.LimitYaw(NECK_YAW_LIMIT);
				neckFacing.LimitPitch(NECK_PITCH_LIMIT);
				neckbone.Orientation = neckFacing;
			}

			return true;
		}

		/// <summary>
		/// Play an animation instantly
		/// </summary>
		public override void ChangeAnimation(string animationName) {
			bodyComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public virtual void Blink() {
			blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		/// <summary>
		/// method for the animation timer to run
		/// </summary>
		protected void AnimTimerTick(object o) {
			if (Pauser.IsPaused) {
				// keep trying again until we're unpaused
				animTimer.Change(500, 500);
			}
			else {
				string anim = "Sit" + random.Next(1, 3);
				bodyComponent.AnimationBlender.Blend(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);

				animTimer.Change(random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);
			}
		}

		/// <summary>
		/// Clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				LKernel.GetG<AnimationManager>().Remove(blinkState);
			}
			if (animTimer != null)
				animTimer.Dispose();

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;

			base.Dispose(disposing);
		}
	}
}

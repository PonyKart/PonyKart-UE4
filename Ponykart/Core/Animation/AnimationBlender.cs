/*
 * Porting notes:
 * Ported by Ernesto Gutierrez (smernesto) - 02-july-2007
 * Added properties for the public variables
 * Changed the code to the C# coding style
 * Tested with Mogre 0.2.0 (Ogre 1.4.0)
 * 
 * http://www.ogre3d.org/tikiwiki/MOGRE+AnimationBlender
 * http://www.ogre3d.org/tikiwiki/AnimationBlender#Utilisation
 */

namespace Mogre {
	public enum AnimationBlendingTransition {
		/// <summary>
		/// immediately switch - stop source and start dest
		/// </summary>
		BlendSwitch,
		/// <summary>
		/// cross fade, blend source animation out while blending destination animation in
		/// </summary>
		BlendWhileAnimating,
		/// <summary>
		/// blend source to first frame of dest, when done, start dest anim
		/// </summary>
		BlendThenAnimate
	}

	public class AnimationBlender {
		private Entity mEntity;
		private AnimationState mSource;
		private AnimationState mTarget;
		private AnimationBlendingTransition mTransition;
		private bool mLoop;
		private float mTimeleft, mDuration;
		private bool mComplete;
		private string mCurrentAnim;

		/// <summary>
		/// Fade between two states
		/// </summary>
		/// <param name="animation">The name of the new animation to switch to</param>
		/// <param name="transition">The transition type to use</param>
		/// <param name="duration">How long the blending should last</param>
		/// <param name="looping">Do the animations loop?</param>
		public void Blend(string animation, AnimationBlendingTransition transition, float duration, bool looping) {
			// return if we're already playing this animation or if that animation doesn't exist
			if (mCurrentAnim == animation || !mEntity.AllAnimationStates.HasAnimationState(animation))
				return;
			mCurrentAnim = animation;

			mLoop = looping;

			if (transition == AnimationBlendingTransition.BlendSwitch) {
				if (mSource != null)
					mSource.Enabled = false;

				mSource = mEntity.GetAnimationState(animation);
				mSource.Enabled = true;
				mSource.Weight = 1;
				mSource.TimePosition = 0;
				mSource.Loop = looping;

				mComplete = true;

				mTimeleft = 0;
			}
			else {
				AnimationState newTarget = mEntity.GetAnimationState(animation);

				if (mTimeleft > 0) {
					// oops, weren't finished yet
					if (newTarget == mSource) {
						// going back to the source state, so let's switch
						mSource = mTarget;
						mTarget = newTarget;
						mTimeleft = mDuration - mTimeleft; // i'm ignoring the new duration here

						mComplete = false;
					}
					else if (newTarget != mTarget) {
						// ok, newTarget is really new, so either we simply replace the target with this one, or
						// we make the target the new source
						if (mTimeleft < mDuration * 0.5f) {
							// simply replace the target with this one
							mTarget.Enabled = false;
							mTarget.Weight = 0;
						}
						else {
							// old target becomes new source
							mSource.Enabled = false;
							mSource.Weight = 0;
							mSource = mTarget;

						}
						mTarget = newTarget;
						mTarget.Enabled = true;
						mTarget.Weight = 1.0f - mTimeleft / mDuration;
						mTarget.TimePosition = 0;
						mTarget.Loop = looping;
						mComplete = false;
					}
					// else -> nothing to do! (ignoring duration here)
				}
				else {
					// assert( target == 0, "target should be 0 when not blending" )
					// mSource->setEnabled(true);
					// mSource->setWeight(1);
					mTransition = transition;
					mTimeleft = mDuration = duration;
					mTarget = newTarget;
					mTarget.Enabled = true;
					mTarget.Weight = 0;
					mTarget.TimePosition = 0;
					mComplete = false;
				}
			}
		}

		/// <summary>
		/// Adds time to the animation, similar to AnimationState.AddTime()
		/// </summary>
		/// <param name="time"></param>
		public void AddTime(float time) {
			if (mSource != null) {
				if (!mComplete) {
					if (mTimeleft > 0) {
						mTimeleft -= time;

						if (mTimeleft < 0) {
							// finish blending
							mSource.Enabled = false;
							mSource.Weight = 0;
							mSource = mTarget;
							mSource.Enabled = true;
							mSource.Weight = 1;
							mTarget = null;
						}
						else {
							// still blending, advance weights
							mSource.Weight = mTimeleft / mDuration;
							mTarget.Weight = 1.0f - mTimeleft / mDuration;

							if (mTransition == AnimationBlendingTransition.BlendWhileAnimating)
								mTarget.AddTime(time);
						}
					}

					if (mSource.TimePosition >= mSource.Length) {
						mComplete = true;
					}
					else {
						mComplete = false;
					}
				}

				mSource.AddTime(time);
				mSource.Loop = mLoop;
			}
		}

		/// <summary>
		/// constructor
		/// </summary>
		public AnimationBlender(Entity entity) {
			mEntity = entity;
		}

		/// <summary>
		/// Initialise the animation blender with an initial animation
		/// </summary>
		/// <param name="animation">The name of the animation it should start with</param>
		/// <param name="looping">Whether the animation should loop or not</param>
		public void Init(string animation, bool looping) {
			AnimationStateSet set = mEntity.AllAnimationStates;
			if (set == null)
				throw new System.InvalidOperationException("That mesh does not have any animations!");

			foreach (AnimationState anim in set.GetAnimationStateIterator()) {
				anim.Enabled = false;
				anim.Weight = 0;
				anim.TimePosition = 0;
			}

			mCurrentAnim = animation;
			mSource = mEntity.GetAnimationState(animation);
			mSource.Enabled = true;
			mSource.Weight = 1;
			mTimeleft = 0;
			mDuration = 1;
			mTarget = null;
			mComplete = true;
			mLoop = looping;
		}

		//Properties

		public float Progress {
			get { return mTimeleft / mDuration; }
		}
		public AnimationState Source {
			get { return mSource; }
		}
		public AnimationState Target {
			get { return mTarget; }
		}
		public Entity Entity {
			get { return mEntity; }
		}

		public bool Complete {
			get { return mTimeleft < mDuration; }
		}

		public float TimeLeft {
			get { return mTimeleft; }
		}

		public float Duration {
			get { return mDuration; }
		}
		public string CurrentAnimation {
			get { return mCurrentAnim; }
		}
	}
}
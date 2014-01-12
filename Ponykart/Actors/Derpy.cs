using Mogre;
using Ponykart.Core;
using PonykartParsers;

namespace Ponykart.Actors
{
	public class Derpy : LThing
	{
		private readonly Radian NECK_PITCH_LIMIT = new Degree(60f);
		private readonly Radian NECK_YAW_LIMIT = new Degree(70f);
		private DerpyAnimation anim;
		private AnimationState blinkState;
		private ModelComponent bodyComponent;
		private Euler bodyFacing;
		private ModelComponent flagComponent;
		private Kart followKart;
		private SceneNode interpNode;
		private Bone neckBone;
		private Euler neckFacing;
		private ModelComponent startLightComponent;

		public Derpy(ThingBlock block, ThingDefinition def)
			: base(block, def)
		{
			bodyComponent = ModelComponents[0];
			flagComponent = ModelComponents[1];
			startLightComponent = ModelComponents[2];

			bodyFacing = new Euler(0, 0, 0);
			neckFacing = new Euler(0, 0, 0);

			Skeleton skeleton = bodyComponent.Entity.Skeleton;
			skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;

			blinkState = bodyComponent.Entity.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1f;
			blinkState.AddTime(ID);

			neckBone = skeleton.GetBone("Neck");
			neckBone.SetManuallyControlled(true);
			foreach (var state in bodyComponent.Entity.AllAnimationStates.GetAnimationStateIterator())
			{
				// don't add a blend mask to the blink state because we'll make a different one for it
				if (state == blinkState)
					continue;

				state.CreateBlendMask(skeleton.NumBones);
				state.SetBlendMaskEntry(neckBone.Handle, 0f);
			}
			neckBone.InheritOrientation = false;

			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			LKernel.GetG<AnimationManager>().Add(blinkState);

			interpNode = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode("DerpyInterpNode" + ID, Vector3.ZERO);

			anim = DerpyAnimation.Hover1;
		}

		private enum DerpyAnimation
		{
			Hover1,
			Forward1,
			Lift1,
			FlagWave1,
			FlagWave2,
			HoldStartLight1,
		}

		/// <summary>
		/// Specifies a kart to follow around
		/// </summary>
		public void AttachToKart(Vector3 offset, Kart kart)
		{
			this.followKart = kart;

			// attach this to the kart's node
			interpNode.Parent.RemoveChild(interpNode);
			kart.RootNode.AddChild(interpNode);

			this.RootNode.Position = Vector3.ZERO;

			interpNode.Position = offset;

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public virtual void Blink()
		{
			blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		public override void ChangeAnimation(string animationName)
		{
			if (bodyComponent.Entity.AllAnimationStates.HasAnimationState(animationName))
			{
				bodyComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);

				if (animationName == "FlagWave1" || animationName == "FlagWave2")
				{
					startLightComponent.Entity.Visible = false;
					flagComponent.Entity.Visible = true;

					flagComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
					startLightComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}
				else if (animationName == "HoldStartLight1")
				{
					flagComponent.Entity.Visible = false;
					startLightComponent.Entity.Visible = true;

					startLightComponent.AnimationBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
					flagComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}
				else
				{
					flagComponent.Entity.Visible = false;
					startLightComponent.Entity.Visible = false;

					startLightComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
					flagComponent.AnimationBlender.Blend("Basis", AnimationBlendingTransition.BlendSwitch, 0, true);
				}

				anim = (DerpyAnimation) System.Enum.Parse(typeof(DerpyAnimation), animationName, true);
			}
		}
		
		/// <summary>
		/// Detach from the kart
		/// </summary>
		public void DetachFromKart()
		{
			if (followKart == null)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;

			//this.RootNode.Position = Vector3.ZERO;

			// reattach it to the scene's root scene node so she still gets rendered
			followKart.RootNode.RemoveChild(interpNode);
			interpNode.Creator.RootSceneNode.AddChild(interpNode);

			followKart = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;

			if (disposing)
			{
				LKernel.GetG<AnimationManager>().Remove(blinkState);
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Change animations based on kart speed
		/// </summary>
		private void EveryTenth(object state)
		{
			if (followKart == null)
				return;
			float speed = followKart.VehicleSpeed;
			if ((speed < 30f && speed > -15f) && anim == DerpyAnimation.Forward1)
			{
				bodyComponent.AnimationBlender.Blend("Hover1", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);
				anim = DerpyAnimation.Hover1;
			}
			else if ((speed > 30f || speed < -15f) && anim == DerpyAnimation.Hover1)
			{
				bodyComponent.AnimationBlender.Blend("Forward1", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);
				anim = DerpyAnimation.Forward1;
			}
		}

		/// <summary>
		/// Update position and orientation of body and neck
		/// </summary>
		private bool FrameStarted(FrameEvent evt)
		{
			if (Pauser.IsPaused)
				return true;

			// update position
			Vector3 derivedInterp = interpNode._getDerivedPosition();
			Vector3 derivedDerpy = this.RootNode._getDerivedPosition();

			Vector3 displacement = derivedInterp - derivedDerpy;
			this.RootNode.Translate(displacement * 6 * evt.timeSinceLastFrame);

			// update orientation of derpy
			Vector3 lookat_body = this.RootNode.Position - followKart.ActualPosition;
			Euler temp_body = bodyFacing.GetRotationTo(lookat_body, true, false, true);
			Radian tempTime = new Radian(evt.timeSinceLastFrame * 3f);
			temp_body.LimitYaw(tempTime);

			bodyFacing = bodyFacing + temp_body;
			this.RootNode.Orientation = bodyFacing;

			// update orientation of neck
			Vector3 lookat_neck = this.RootNode.ConvertWorldToLocalPosition(followKart.ActualPosition);
			Euler temp_neck = neckFacing.GetRotationTo(-lookat_neck, true, true, true);
			tempTime *= 1.5f;
			temp_neck.LimitYaw(tempTime);
			temp_neck.LimitPitch(tempTime);

			neckFacing = neckFacing + temp_neck;
			neckFacing.LimitYaw(NECK_YAW_LIMIT);
			neckFacing.LimitPitch(NECK_PITCH_LIMIT);
			neckBone.Orientation = neckFacing;

			return true;
		}

		/*
			Body animaitons:
			Stand, Hover1, Forward1, Lift1, Blink, Blink2 (2 blinks spread out), Flap1, Flap2 (Same as Flap 1 but 12 fames shorter), WingsRest

			Body/Flag animations:
			FlagWave1, FlagWave2

			Body/StartLight animations:
			HoldStartLight1
		 */
	}
}
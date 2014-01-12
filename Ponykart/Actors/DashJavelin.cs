using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Sound;
using PonykartParsers;

namespace Ponykart.Actors {
	public class DashJavelin : Kart {
		private AnimationState jetMax;
		private AnimationState jetMin;
		private readonly float topSpeedKmHour;
		private float jetOpening = 0f;

		private RibbonTrail jetRibbon;

		private SoundMain soundMain;
		private ISound idleSound, fullSound;
		private ISoundSource revDownSound, revUpSound;
		/// <summary>
		/// true if we're in the "play the slower sound" state, false if we're in the "play the faster sound" state
		/// </summary>
		private bool idleState;


		public DashJavelin(ThingBlock block, ThingDefinition def) : base(block, def) {
			ModelComponent chassis = ModelComponents[0];
			// first get rid of the existing animation blender it creates automatically
			LKernel.GetG<AnimationManager>().Remove(chassis.AnimationBlender);
			chassis.AnimationBlender = null;


			Entity chassisEnt = chassis.Entity;
			// get our two animation states
			jetMax = chassisEnt.GetAnimationState("JetMax");
			jetMax.Enabled = true;
			jetMax.Weight = 0f;
			jetMin = chassisEnt.GetAnimationState("JetMin");
			jetMin.Enabled = true;
			jetMin.Weight = 1f;

			// we want the two animations to blend together, not add to each other
			chassisEnt.Skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_AVERAGE;

			jetRibbon = RibbonComponents[0].Ribbon;

			// sounds
			soundMain = LKernel.GetG<SoundMain>();

			idleSound = SoundComponents[0].Sound;
			fullSound = SoundComponents[1].Sound;
			revDownSound = soundMain.GetSource("RD_Kart_Rev_Down.ogg");
			revUpSound = soundMain.GetSource("RD_Kart_Rev_Up.ogg");

			// convert from linear velocity to KPH
			topSpeedKmHour = DefaultMaxSpeed * 3.6f;
			idleState = true;

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		const float JET_FLAP_INTERP = 0.05f;
		/// <summary>
		/// Change the width of the jet engine based on our current speed
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			// crop it to be between 0 and 1
			float relSpeed = _vehicle.CurrentSpeedKmHour / topSpeedKmHour;

			if (_acceleration <= 0f) {
				// halve it
				float halfRelSpeed = relSpeed * 0.5f;

				// make sure it isn't bigger or smaller than this
				if (halfRelSpeed > 0.5f)
					halfRelSpeed = 0.5f;
				else if (halfRelSpeed < 0f)
					halfRelSpeed = 0f;
				
				// close the flaps
				if (halfRelSpeed < jetOpening && jetOpening > 0f) {
					// this logic is to make sure we don't close them faster than JET_FLAP_INTERP, but at the same time, if the difference between
					// the target opening and the actual opening is less than JET_FLAP_INTERP, use that instead
					if (jetOpening - halfRelSpeed < JET_FLAP_INTERP)
						jetOpening = halfRelSpeed;
					else
						jetOpening -= JET_FLAP_INTERP;
				}
				// open the flaps
				else if (halfRelSpeed > jetOpening && jetOpening < 1f) {
					// same here
					if (halfRelSpeed - jetOpening < JET_FLAP_INTERP)
						jetOpening = halfRelSpeed;
					else
						jetOpening += JET_FLAP_INTERP;
				}

				// make the ribbon smaller
				if (jetRibbon != null) {
					jetRibbon.SetInitialWidth(0u, jetOpening * 0.2f);
					jetRibbon.SetColourChange(0u, 0f, 0f, 0f, 20f);
				}

				// play the rev down sound, crossfade
				if (relSpeed < 0.5f && !idleState) {
					soundMain.Play3D(revDownSound, ActualPosition, false);

					new SoundCrossfader(fullSound, idleSound, 1.65f, 0.5f);

					idleState = true;
				}
			}
			else {
				// increase the jet opening
				if (jetOpening < 1f)
					jetOpening += JET_FLAP_INTERP;

				// make the ribbon bigger
				if (jetRibbon != null) {
					jetRibbon.SetInitialWidth(0u, jetOpening * 0.2f);
					jetRibbon.SetColourChange(0u, 0f, 0f, 0f, 3f);
				}

				// play the rev up sound, crossfade
				if (relSpeed > 0.5f && idleState) {
					soundMain.Play3D(revUpSound, ActualPosition, false);

					new SoundCrossfader(idleSound, fullSound, 1.45f, 0.5f);

					idleState = false;
				}
			}

			// then finally set the weights of the jet flap bones appropriately
			jetMax.Weight = jetOpening;
			jetMin.Weight = 1f - jetOpening;

			return true;
		}

		/// <summary>
		/// Unhook from the event
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;

			base.Dispose(disposing);
		}
	}
}

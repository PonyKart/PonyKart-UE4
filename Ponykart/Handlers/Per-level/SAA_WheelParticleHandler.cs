using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race, "SweetAppleAcres")]
	public class SAA_WheelParticleHandler : ILevelHandler {
		// our dictionaries of particles
		private IDictionary<int, Pair<WheelHelper, WheelHelper>> wheelHelpers;
		private IList<KartSpeedState> kartSpeedStates;

		private RigidBody dirtBody;
		private RigidBody grassBody;

		// default emission rates
		float defaultDustEmissionRate = -1;
		float defaultMudEmissionRate = -1;
		float defaultGrassEmissionRate = -1;
		readonly float renderingDistance = 60f;

		/// <summary>
		/// I mostly put this in C# because running a lua function 10 times every second per kart seemed like it could cause some slowdowns
		/// </summary>
		public SAA_WheelParticleHandler() {
			// init the dictionaries
			wheelHelpers = new Dictionary<int, Pair<WheelHelper, WheelHelper>>();
			kartSpeedStates = new List<KartSpeedState>();

			// hook up to events
			KartHandler.OnGround += OnGround;
			KartHandler.OnGroundChanged += OnGroundChanged;
			KartHandler.OnTouchdown += OnTouchdown;
			KartHandler.OnLiftoff += OnLiftoff;

			var sceneMgr = LKernel.GetG<SceneManager>();

			// create particles for each wheel
			foreach (Player p in LKernel.GetG<PlayerManager>().Players) {
				

				WheelHelper lefthelper = new WheelHelper();

				lefthelper.dust = sceneMgr.CreateParticleSystem("wheelDustLeftParticle" + p.ID, "dust");
				lefthelper.mud = sceneMgr.CreateParticleSystem("wheelMudLeftParticle" + p.ID, "mud");
				lefthelper.grass = sceneMgr.CreateParticleSystem("wheelGrassLeftParticle" + p.ID, "grass");

				lefthelper.dust.RenderingDistance = renderingDistance;
				lefthelper.mud.RenderingDistance = renderingDistance;
				lefthelper.grass.RenderingDistance = renderingDistance;

				p.Kart.LeftParticleNode.AttachObject(lefthelper.dust);
				p.Kart.LeftParticleNode.AttachObject(lefthelper.mud);
				//p.Kart.LeftParticleNode.AttachObject(lefthelper.grass);

				lefthelper.DisableDust();
				lefthelper.DisableGrass();

				WheelHelper righthelper = new WheelHelper();

				righthelper.dust = sceneMgr.CreateParticleSystem("wheelDustRightParticle" + p.ID, "dust");
				righthelper.mud = sceneMgr.CreateParticleSystem("wheelMudRightParticle" + p.ID, "mud");
				righthelper.grass = sceneMgr.CreateParticleSystem("wheelGrassRightParticle" + p.ID, "grass");

				righthelper.dust.RenderingDistance = renderingDistance;
				righthelper.mud.RenderingDistance = renderingDistance;
				righthelper.grass.RenderingDistance = renderingDistance;

				p.Kart.RightParticleNode.AttachObject(righthelper.dust);
				p.Kart.RightParticleNode.AttachObject(righthelper.mud);
				//p.Kart.RightParticleNode.AttachObject(righthelper.grass);

				righthelper.DisableDust();
				righthelper.DisableGrass();

				if (defaultDustEmissionRate == -1)
					defaultDustEmissionRate = lefthelper.dust.GetEmitter(0).EmissionRate;
				if (defaultGrassEmissionRate == -1)
					defaultGrassEmissionRate = lefthelper.grass.GetEmitter(0).EmissionRate;
				if (defaultMudEmissionRate == -1)
					defaultMudEmissionRate = lefthelper.mud.GetEmitter(0).EmissionRate;

				wheelHelpers.Add(p.ID, new Pair<WheelHelper, WheelHelper>(lefthelper, righthelper));

				// add one of these to the states list
				kartSpeedStates.Add(KartSpeedState.None);
			}

			dirtBody = LKernel.GetG<LevelManager>().CurrentLevel.Things["SAA_Road_Combined"].Body;
			grassBody = LKernel.GetG<LevelManager>().CurrentLevel.Things["SAA_Ground"].Body;
		}

		/// <summary>
		/// Sets all of the emission states of the dirt particles of a kart
		/// </summary>
		void DirtEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Dust(isEmitting);
			wheelHelpers[kartID].second.Dust(isEmitting);
		}

		/// <summary>
		/// Sets all of the emission states of the grass particles of a kart
		/// </summary>
		void GrassEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Grass(isEmitting);
			wheelHelpers[kartID].second.Grass(isEmitting);
		}

		/// <summary>
		/// Sets all of the emission states of both the dirt and grass particles of a kart
		/// </summary>
		void BothEmitting(int kartID, bool isEmitting) {
			wheelHelpers[kartID].first.Dust(isEmitting);
			wheelHelpers[kartID].second.Dust(isEmitting);

			wheelHelpers[kartID].first.Grass(isEmitting);
			wheelHelpers[kartID].second.Grass(isEmitting);
		}

		/// <summary>
		/// turn off all particles
		/// </summary>
		void OnLiftoff(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			BothEmitting(kart.OwnerID, false);
		}

		/// <summary>
		/// turn on the appropriate particles
		/// </summary>
		void OnTouchdown(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			if (kart.VehicleSpeed > 4f || kart.VehicleSpeed < -4f) {
				if (callback.CollisionObject == dirtBody)
					DirtEmitting(kart.OwnerID, true);
				else if (callback.CollisionObject == grassBody)
					GrassEmitting(kart.OwnerID, true);
			}
			else {
				DirtEmitting(kart.OwnerID, false);
				GrassEmitting(kart.OwnerID, false);
			}
		}

		/// <summary>
		/// Change the particles appropriately
		/// </summary>
		void OnGroundChanged(Kart kart, CollisionObject newGround, CollisionObject oldGround) {
			if (newGround == dirtBody)
				DirtEmitting(kart.OwnerID, true);
			else if (oldGround == dirtBody)
				DirtEmitting(kart.OwnerID, false);

			if (newGround == grassBody)
				GrassEmitting(kart.OwnerID, true);
			else if (oldGround == grassBody)
				GrassEmitting(kart.OwnerID, false);
		}

		/// <summary>
		/// update the particle emission rates depending on speed
		/// </summary>
		void OnGround(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			float speed = System.Math.Abs(kart.VehicleSpeed);

			// if the kart is moving slowly, then just turn the particles completely off
			if (speed < 6) {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Slow) {
					// update this if we need to
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Slow;
					BothEmitting(kart.OwnerID, false);
				}
			}
			// if we're moving at a medium speed
			else if (speed >= 6 && speed < 30) {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Medium) {
					// update this if we need to
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Medium;

					if (callback.CollisionObject == dirtBody)
						DirtEmitting(kart.OwnerID, true);
					else if (callback.CollisionObject == grassBody)
						GrassEmitting(kart.OwnerID, true);
				}

				// make some new emission rates
				float dustEmissionRate = (speed / 30) * defaultDustEmissionRate;
				float grassEmissionRate = (speed / 30) * defaultGrassEmissionRate;
				float mudEmissionRate = (speed / 30) * defaultMudEmissionRate;

				// and update the particles
				Pair<WheelHelper, WheelHelper> pair = wheelHelpers[kart.OwnerID];
				pair.first.dust.GetEmitter(0).EmissionRate = dustEmissionRate;
				pair.second.dust.GetEmitter(0).EmissionRate = dustEmissionRate;
				pair.first.grass.GetEmitter(0).EmissionRate = grassEmissionRate;
				pair.second.grass.GetEmitter(0).EmissionRate = grassEmissionRate;
				pair.first.mud.GetEmitter(0).EmissionRate = mudEmissionRate;
				pair.second.mud.GetEmitter(0).EmissionRate = mudEmissionRate;
			}
			// and if we're moving at a fast speed
			else {
				if (kartSpeedStates[kart.OwnerID] != KartSpeedState.Fast) {
					kartSpeedStates[kart.OwnerID] = KartSpeedState.Fast;

					// and update the particles
					Pair<WheelHelper, WheelHelper> pair = wheelHelpers[kart.OwnerID];
					pair.first.dust.GetEmitter(0).EmissionRate = defaultDustEmissionRate;
					pair.second.dust.GetEmitter(0).EmissionRate = defaultDustEmissionRate;
					pair.first.grass.GetEmitter(0).EmissionRate = defaultGrassEmissionRate;
					pair.second.grass.GetEmitter(0).EmissionRate = defaultGrassEmissionRate;
					pair.first.mud.GetEmitter(0).EmissionRate = defaultMudEmissionRate;
					pair.second.mud.GetEmitter(0).EmissionRate = defaultMudEmissionRate;
				}
			}
		}

		public void Detach() {
			KartHandler.OnGround -= OnGround;
			KartHandler.OnGroundChanged -= OnGroundChanged;
			KartHandler.OnTouchdown -= OnTouchdown;
			KartHandler.OnLiftoff -= OnLiftoff;

			wheelHelpers.Clear();
		}

		private enum KartSpeedState {
			None, Slow, Medium, Fast
		}

		private class WheelHelper {
			public ParticleSystem mud, grass, dust;

			public void EnableGrass() {
				mud.Emitting = grass.Emitting = true;
			}

			public void Grass(bool enabled) {
				mud.Emitting = grass.Emitting = enabled;
			}

			public void DisableGrass() {
				mud.Emitting = grass.Emitting = false;
			}

			public void EnableDust() {
				dust.Emitting = true;
			}

			public void Dust(bool enabled) {
				dust.Emitting = enabled;
			}

			public void DisableDust() {
				dust.Emitting = false;
			}
		}
	}
}

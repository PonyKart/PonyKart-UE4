using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Handlers {
	/// <summary>
	/// moving this from lua to c#
	/// 
	/// this just manages the little particle effect you get when a kart lands on something
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class PomfHandler : ILevelHandler {
		IDictionary<Kart, ParticleSystem> particles;
		IDictionary<Kart, bool> waitDict;


		public PomfHandler() {
			particles = new Dictionary<Kart, ParticleSystem>();
			waitDict = new Dictionary<Kart, bool>();

			var playerMgr = LKernel.GetG<PlayerManager>();
			var sceneMgr = LKernel.GetG<SceneManager>();

			foreach (Player player in playerMgr.Players) {
				var pomf = sceneMgr.CreateParticleSystem("kartPomfParticle" + player.ID, "Pomf");
				pomf.Emitting = false;
				player.Kart.RootNode.AttachObject(pomf);
				particles.Add(player.Kart, pomf);
			}

			KartHandler.OnTouchdown += OnTouchdown;
			KartHandler.OnCloseToTouchdown += OnGround;
			KartHandler.OnLiftoff += OnGround;
			KartHandler.OnGround += OnGround;
		}

		void OnGround(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			// we need to wait two "ticks" of the ray casting (0.1s total at time of writing) before we stop the emitter again
			// we could just wait one, but that's too short
			// the little boolean stuff is just a sort of "toggle" between the two ticks

			// we can't really unhook from the events because we've got multiple karts
			bool wait;
			if (waitDict.TryGetValue(kart, out wait)) {
				if (!wait)
					particles[kart].Emitting = false;
				else
					waitDict[kart] = true;
			}
			else
				waitDict[kart] = true;
		}

		void OnTouchdown(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			particles[kart].Emitting = true;
			waitDict[kart] = false;
		}


		public void Detach() {
			KartHandler.OnTouchdown -= OnTouchdown;
			KartHandler.OnCloseToTouchdown -= OnGround;
			KartHandler.OnLiftoff -= OnGround;
			KartHandler.OnGround -= OnGround;

			var sceneMgr = LKernel.GetG<SceneManager>();
			foreach (ParticleSystem system in particles.Values) {
				sceneMgr.DestroyParticleSystem(system);
			}

			particles.Clear();
			particles = null;

			waitDict.Clear();
			waitDict = null;
		}
	}
}

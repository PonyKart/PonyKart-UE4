using System.Collections.Concurrent;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;

namespace Ponykart.Core {
	public class LThingHelperManager {
		ConcurrentDictionary<Kart, Skidder> Skidders;
		ConcurrentDictionary<LThing, Nlerper> Nlerpers;
		ConcurrentDictionary<LThing, Rotater> Rotaters;

		public LThingHelperManager() {
			Skidders = new ConcurrentDictionary<Kart, Skidder>();
			Nlerpers = new ConcurrentDictionary<LThing, Nlerper>();
			Rotaters = new ConcurrentDictionary<LThing, Rotater>();

			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			Nlerper.Finished += new NlerperEvent(Nlerper_Finished);
			Rotater.Finished += new RotaterEvent(Rotater_Finished);
			Skidder.Finished += new SkidderEvent(Skidder_Finished);
		}

		/// <summary>
		/// Cleans up a skidder when it's finished
		/// </summary>
		void Skidder_Finished(Skidder skidder, Kart kart) {
			if (Skidders.Values.Contains(skidder)) {
				Skidder existing;
				Skidders.TryRemove(kart, out existing);
			}
		}

		/// <summary>
		/// Cleans up a rotater when it's finished
		/// </summary>
		void Rotater_Finished(Rotater rotater, LThing thing) {
			if (Rotaters.Values.Contains(rotater)) {
				Rotater existing;
				Rotaters.TryRemove(thing, out existing);
			}
		}

		/// <summary>
		/// Cleans up a nlerper when it's finished
		/// </summary>
		void Nlerper_Finished(Nlerper nlerper, LThing thing) {
			if (Nlerpers.Values.Contains(nlerper)) {
				Nlerper existing;
				Nlerpers.TryRemove(thing, out existing);
			}
		}

		/// <summary>
		/// Create a Skidder. This removes any existing skidders attached to the kart.
		/// </summary>
		/// <param name="owner">What kart are we affecting?</param>
		/// <param name="duration">How long the skid effect lasts</param>
		/// <returns>The new Skidder</returns>
		public Skidder CreateSkidder(Kart owner, float duration = 0.5f) {
			RemoveSkidder(owner);

			Skidder newSkidder = new Skidder(owner, duration);
			Skidders.TryAdd(owner, newSkidder);
			return newSkidder;
		}

		/// <summary>
		/// Removes a skidder with the given key.
		/// </summary>
		public void RemoveSkidder(Kart owner) {
			Skidder existing;
			if (Skidders.TryRemove(owner, out existing)) {
				existing.Detach();
			}
		}

		/// <summary>
		/// Removes a skidder but checks that the given skidder is still in our dictionary.
		/// </summary>
		public void Remove(Skidder skidder) {
			if (Skidders.Values.Contains(skidder)) {
				RemoveSkidder(skidder.Owner);
			}
		}

		/// <summary>
		/// Creates a Nlerper. This removes any existing nlerpers and rotaters attached to the lthing.
		/// </summary>
		/// <param name="owner">What lthing was we affecting?</param>
		/// <param name="duration">How long are we going to nlerp?</param>
		/// <param name="orientDest">Destination orientation when it is finished.</param>
		/// <returns>The new nlerper</returns>
		public Nlerper CreateNlerper(LThing owner, float duration, Quaternion orientDest) {
			RemoveNlerper(owner);
			RemoveRotater(owner);

			Nlerper newNlerper = new Nlerper(owner, duration, orientDest);
			Nlerpers.TryAdd(owner, newNlerper);
			return newNlerper;
		}

		/// <summary>
		/// Removes a nlerper with the given key.
		/// </summary>
		public void RemoveNlerper(LThing owner) {
			Nlerper existing;
			if (Nlerpers.TryRemove(owner, out existing)) {
				existing.Detach();
			}
		}

		/// <summary>
		/// Removes a nlerper but checks that the given nlerper is still in our dictionary.
		/// </summary>
		public void Remove(Nlerper nlerper) {
			if (Nlerpers.Values.Contains(nlerper)) {
				RemoveNlerper(nlerper.Owner);
			}
		}

		/// <summary>
		/// Creates a Rotater. This removes any existing nlerpers and rotaters attached to the lthing.
		/// </summary>
		/// <param name="owner">What lthing are we affecting?</param>
		/// <param name="duration">How long as we going to rotate?</param>
		/// <param name="angle">Rotation angle</param>
		/// <param name="axis">Axis of rotation</param>
		/// <param name="mode">Rotation axis mode</param>
		/// <returns>The new rotater</returns>
		public Rotater CreateRotater(LThing owner, float duration, Radian angle, Vector3 axis, RotaterAxisMode mode = RotaterAxisMode.Explicit) {
			RemoveRotater(owner);
			RemoveNlerper(owner);

			Rotater newRotater = new Rotater(owner, duration, angle, axis, mode);
			Rotaters.TryAdd(owner, newRotater);
			return newRotater;
		}

		/// <summary>
		/// Creates a Rotater. This removes any existing nlerpers and rotaters attached to the lthing.
		/// </summary>
		/// <param name="owner">What lthing are we affecting?</param>
		/// <param name="duration">How long as we going to rotate?</param>
		/// <param name="angle">Rotation angle</param>
		/// <param name="mode">Rotation axis mode</param>
		/// <returns>The new rotater</returns>
		public Rotater CreateRotater(LThing owner, float duration, Radian angle, RotaterAxisMode mode) {
			RemoveRotater(owner);

			Rotater newRotater = new Rotater(owner, duration, angle, mode);
			Rotaters.TryAdd(owner, newRotater);
			return newRotater;
		}

		/// <summary>
		/// Removes a rotater with the given key.
		/// </summary>
		public void RemoveRotater(LThing owner) {
			Rotater existing;
			if (Rotaters.TryRemove(owner, out existing)) {
				existing.Detach();
			}
		}

		/// <summary>
		/// Removes a rotater but checks that the given rotater is still in our dictionary.
		/// </summary>
		public void Remove(Rotater rotater) {
			if (Rotaters.Values.Contains(rotater)) {
				RemoveRotater(rotater.Owner);
			}
		}


		/// <summary>
		/// clean up everything
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			foreach (var skidder in Skidders.Values) {
				skidder.Detach();
			}
			Skidders.Clear();

			foreach (var nlerper in Nlerpers.Values) {
				nlerper.Detach();
			}
			Nlerpers.Clear();

			foreach (var rotater in Rotaters.Values) {
				rotater.Detach();
			}
			Rotaters.Clear();
		}
	}
}

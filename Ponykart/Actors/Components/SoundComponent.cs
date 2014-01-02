using IrrKlang;
using Mogre;
using Ponykart.Sound;
using PonykartParsers;
// to stop VS from getting rid of these in release mode
#if DEBUG
using Ponykart.UI;
#endif

namespace Ponykart.Actors {
	public delegate void SoundFrameEvent(LThing thing, ISound sound);

	public class SoundComponent {
		public ISound Sound { get; protected set; }
		public string Name { get; protected set; }
		private Vector3 relativePosition;
		private LThing owner;
		public bool NeedUpdate = false;
		public event SoundFrameEvent OnUpdate;

		/// <summary>
		/// For sounds!
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public SoundComponent(LThing lthing, ThingBlock template, SoundBlock block) {
			Name = block.GetStringProperty("name", template.ThingName);
			owner = lthing;

			var soundMain = LKernel.GetG<SoundMain>();
			ISoundSource source = soundMain.GetSource(block.GetStringProperty("File", null));

			bool looping = block.GetBoolProperty("looping", true);
			bool sfx = block.GetBoolProperty("SpecialEffects", false);
			relativePosition = block.GetVectorProperty("position", Vector3.ZERO);
			bool startPaused = block.GetBoolProperty("StartPaused", true);

			Sound = soundMain.Play3D(source, relativePosition, looping, startPaused, sfx);

			if(Sound != null) {
				Sound.PlaybackSpeed = block.GetFloatProperty("Speed", 1);
				float volume;
				if (block.FloatTokens.TryGetValue("volume", out volume))
					Sound.Volume = volume;

				Sound.MinDistance = block.GetFloatProperty("mindistance", soundMain.Engine.Default3DSoundMinDistance);

				// TODO: effects, if we end up using any of them

				Update();
				Sound.Paused = false;
			}

			soundMain.AddSoundComponent(this);
		}

		/// <summary>
		/// Update's the sound's position (and velocity if the owner has a Body).
		/// This is called from <see cref="Ponykart.Physics.MogreMotionState"/>.
		/// </summary>
		public void Update() {
			if(Sound == null)
				return;

			NeedUpdate = false;
			owner.SoundsNeedUpdate = false;

			Vector3 parent = owner.RootNode._getDerivedPosition();
			// update the position
			Sound.Position = new Vector3D(parent.x + relativePosition.x, parent.y + relativePosition.y, parent.z + relativePosition.z);
			if (owner.Body != null) {
				Sound.Velocity = owner.Body.LinearVelocity.ToSoundVector();
			}

			// run the OnUpdate method if it has one
			if (OnUpdate != null) {
#if DEBUG
				try {
#endif
					OnUpdate.Invoke(owner, Sound);
#if DEBUG
				}
				catch (System.Exception ex) {
					Launch.Log("[Lua] *** EXCEPTION *** at " + ex.Source + ": " + ex.Message);
					foreach (var v in ex.Data)
						Launch.Log("[Lua] " + v);
					LKernel.GetG<LuaConsoleManager>().AddLabel("ERROR: " + ex.Message);
					Launch.Log(ex.StackTrace);
				}
#endif
			}
		}

		public override string ToString() {
			return Name + "Sound";
		}

		public void Dispose() {
			if(Sound != null) {
				Sound.Stop();
				Sound.Dispose();
			}
			OnUpdate = null;
		}
	}
}

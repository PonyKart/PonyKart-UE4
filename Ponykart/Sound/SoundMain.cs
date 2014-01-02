using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BulletSharp;
using IrrKlang;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Sound {
	public class SoundMain {
		public ISoundEngine Engine { get; private set; }

		private IList<ISound> musics;
		private IList<ISound> sounds;
		private HashSet<SoundComponent> components;

		private bool enableMusic;
		private bool enableSounds;

		private CameraManager cameraManager;
		private PlayerManager playerManager;

		private IDictionary<string, string> fileList;

		/// <summary>
		/// The sound manager class.
		/// </summary>
		public SoundMain() {
			Launch.Log("[Loading] Creating IrrKlang and SoundMain...");
			musics = new List<ISound>();
			sounds = new List<ISound>();
			components = new HashSet<SoundComponent>();

			enableMusic = Options.GetBool("Music");
			enableSounds = Options.GetBool("Sounds");

			playerManager = LKernel.GetG<PlayerManager>();
			cameraManager = LKernel.GetG<CameraManager>();

			playerManager.OnPostPlayerCreation += new PlayerEvent(OnPostPlayerCreation);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LKernel.GetG<Pauser>().PauseEvent += new PauseEvent(PauseEvent);

			SoundEngineOptionFlag flags = SoundEngineOptionFlag.DefaultOptions | SoundEngineOptionFlag.MuteIfNotFocused | SoundEngineOptionFlag.MultiThreaded;
			
			try {
				Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, flags);
			}
			catch (System.Exception) {
				Launch.Log("[Loading] Cannot initialize real SoundOutputDriver!");
				Engine = new ISoundEngine(SoundOutputDriver.NullDriver, flags);
			}
			
			Engine.Default3DSoundMinDistance = 50f;

			Launch.Log("[Loading] IrrKlang and SoundMain initialised!");
		}

		void PauseEvent(PausingState state) {
			Engine.SetAllSoundsPaused(state == PausingState.Pausing);
		}

		/// <summary>
		/// Manually called from the LevelManager
		/// </summary>
		void OnPostPlayerCreation() {
			if (LKernel.GetG<LevelManager>().IsPlayableLevel)
				Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
		}

		/// <summary>
		/// Dispose all of the sound sources
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;

			Engine.RemoveAllSoundSources();
			Engine.SetListenerPosition(0, 0, 0, 0, 0, -1);
			musics.Clear();
			sounds.Clear();
			components.Clear();
			components = new HashSet<SoundComponent>();
		}

		/// <summary>
		/// prepares file locations
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			fileList = new Dictionary<string, string>();

			foreach (string group in ResourceGroupManager.Singleton.GetResourceGroups().Where(s => ResourceGroupManager.Singleton.IsResourceGroupInitialised(s))) {
				if (group == "Bootstrap")
					continue;

				var resourceLocations = ResourceGroupManager.Singleton.ListResourceLocations(group);

				foreach (string loc in resourceLocations) {
					var soundfiles = Directory.EnumerateFiles(loc, "*.ogg").Union(Directory.EnumerateFiles(loc, "*.mp3")).Union(Directory.EnumerateFiles(loc, "*.wav"));

					foreach (string file in soundfiles) {
						fileList[Path.GetFileName(file)] = file;
					}
				}
			}
		}

		void EveryTenth(object o) {
			if (playerManager.MainPlayer == null) {
				Engine.Update();
				return;
			}
			var cam = cameraManager.CurrentCamera;
			RigidBody body = playerManager.MainPlayer.Body;

			Vector3 pos, rot, vel;
			if (cam is PlayerCamera || cam is KnightyCamera) {
				pos = body.CenterOfMassPosition;
				rot = body.Orientation.YAxis;
				vel = body.LinearVelocity;
			}
			else {
				Quaternion derivedOrientation = cam.Camera.DerivedOrientation;
				pos = cam.Camera.DerivedPosition;
				rot = derivedOrientation.YAxis;
				vel = body.LinearVelocity;
			}

			Engine.SetListenerPosition(
				pos.x, pos.y, pos.z,
				rot.x, rot.y, rot.z,
				vel.x, vel.y, vel.z,
				0, 1, 0);

			foreach (var component in components) {
				if (component.NeedUpdate) {
					component.Update();
				}
			}

			Engine.Update();
		}


		public void EnableMusic() {
			foreach (ISound sound in musics) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enableMusic = true;
		}

		public void DisableMusic() {
			foreach (ISound sound in musics) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enableMusic = false;
		}

		public void EnableSounds() {
			foreach (ISound sound in sounds) {
				sound.Volume += 1;
				sound.Paused = false;
			}
			enableSounds = true;
		}

		public void DisableSounds() {
			foreach (ISound sound in sounds) {
				sound.Volume -= 1;
				sound.Paused = true;
			}
			enableSounds = false;
		}

		/// <summary>
		/// Creates a 2D music sound. This has no position and is controlled by a different option than the Play2D and Play3D methods.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound PlayMusic(string filename, bool startPaused = false) {
			return PlayMusic(GetSource(filename), startPaused);
		}

		/// <summary>
		/// Creates a 2D music sound. This has no position and is controlled by a different option than the Play2D and Play3D methods.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound PlayMusic(ISoundSource source, bool startPaused = false) {
			Launch.Log("[Sounds] Creating music: " + source.Name);

			ISound music = Engine.Play2D(source, true, startPaused, false);
			if(music == null)
				return null;

			musics.Add(music);

			music.Volume = 0.5f;
			if (!enableMusic) {
				music.Paused = true;
				music.Volume = 0;
			}
			else if (startPaused)
				music.Paused = true;

			return music;
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level ambients and whatnot.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound Play2D(string filename, bool looping, bool startPaused = false, bool sfx = false) {
			return Play2D(GetSource(filename), looping, startPaused, sfx);
		}

		/// <summary>
		/// Creates an ambient sound. These have no 3D position or effects or anything, so this is ideal for level ambients and whatnot.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound Play2D(ISoundSource source, bool looping, bool startPaused = false, bool sfx = false) {
			Launch.Log("[Sounds] Creating 2D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play2D(source, looping, startPaused, sfx);
			if(sound == null)
				return null;

			sounds.Add(sound);

			if (!enableSounds) {
				sound.Paused = true;
				sound.Volume = 0;
			}
			else if (startPaused)
				sound.Paused = true;

			return sound;
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
		/// </summary>
		/// <param name="filename">The file path of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound Play3D(string filename, Vector3 pos, bool looping, bool startPaused = false, bool sfx = false) {
			return Play3D(GetSource(filename), pos, looping, startPaused, sfx);
		}

		/// <summary>
		/// Creates an object sound. These sounds do have a 3D position and are attached to SceneNodes. Use these for sound effects and stuff.
		/// </summary>
		/// <param name="source">The sound source of the sound you want to play.</param>
		/// <param name="pos">The Position you want this sound to play at.</param>
		/// <param name="looping">Make this sound loop?</param>
		/// <param name="startPaused">Should this sound be paused when started? Default is false.</param>
		/// <param name="sfx">Does this sound have any effects? Default is false.</param>
		/// <returns>The ISound you just created, or null if we're using the null sound driver.</returns>
		public ISound Play3D(ISoundSource source, Vector3 pos, bool looping, bool startPaused = false, bool sfx = false) {
			if (pos == null)
				throw new ArgumentException("Position cannot be null!", "pos");

			Launch.Log("[Sounds] Creating 3D sound: " + source.Name + " Looping: " + looping);

			ISound sound = Engine.Play3D(source, pos.x, pos.y, pos.z, looping, startPaused, sfx);
			if(sound == null)
				return null;

			sounds.Add(sound);

			if (!enableSounds) {
				sound.Paused = true;
				sound.Volume = 0;
			}
			else if (startPaused)
				sound.Paused = true;

			return sound;
		}

		/// <summary>
		/// Gets a sound source. The engine keeps track of all of these.
		/// </summary>
		/// <param name="filename">Don't include the "media/sound/" bit.</param>
		public ISoundSource GetSource(string filename) {
			string _path = Path.GetFileName(filename);
			string fullpath;

			if (fileList.TryGetValue(_path, out fullpath)) {
				return Engine.GetSoundSource(fullpath, true);
			}
			else
				throw new FileNotFoundException(_path + " was not found!");
		}

		public void AddSoundComponent(SoundComponent sc) {
			components.Add(sc);
		}


		float rolloff;
		/// <summary>
		/// 0 = no rolloff
		/// 1 = default rolloff
		/// </summary>
		public float RolloffFactor {
			get {
				return rolloff;
			}
			set {
				rolloff = value;
				Engine.SetRolloffFactor(value);
			}
		}

		public bool IsMusicEnabled {
			get {
				return enableMusic;
			}
		}

		public bool IsSoundEnabled {
			get {
				return enableSounds;
			}
		}
	}
}

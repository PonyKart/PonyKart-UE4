using System;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a bunch of extra stuff that needs to go in the scene, such as ambient light, shadow info, a sunlight, etc.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class SceneEnvironmentHandler {

		public SceneEnvironmentHandler() {
			LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
		}

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Race) {
				var sceneMgr = LKernel.GetG<SceneManager>();
				var def = eventArgs.NewLevel.Definition;

				// background color
				LKernel.GetG<Viewport>().BackgroundColour = def.GetVectorProperty("Background", Vector3.UNIT_SCALE).ToColourValue();

				// ambient
				sceneMgr.AmbientLight = def.GetVectorProperty("Ambient", Vector3.UNIT_SCALE).ToColourValue();

				// sunlight
				Light light = sceneMgr.CreateLight("sun");
				light.Type = Light.LightTypes.LT_DIRECTIONAL;
				light.Direction = def.GetVectorProperty("SunlightDirection", new Vector3(0.1f, -1f, 0.1f));
				light.Direction.Normalise();
				light.DiffuseColour = def.GetVectorProperty("SunlightColour", Vector3.UNIT_SCALE).ToColourValue();
				light.SpecularColour = def.GetVectorProperty("SunlightColour", Vector3.UNIT_SCALE).ToColourValue();
				// cast shadows if we want some
				if (Options.ShadowDetail != ShadowDetailOption.None)
					light.CastShadows = true;

				// skybox
				if (def.StringTokens.ContainsKey("skybox"))
					sceneMgr.SetSkyBox(true, def.GetStringProperty("Skybox", null), 399f);

				// fog
				FogMode mode = FogMode.FOG_NONE;
				string sMode = def.GetStringProperty("FogType", "None");

				if (sMode.Equals("None", StringComparison.InvariantCultureIgnoreCase))
					mode = FogMode.FOG_NONE;
				else if (sMode.Equals("Exp", StringComparison.InvariantCultureIgnoreCase))
					mode = FogMode.FOG_EXP;
				else if (sMode.Equals("Exp2", StringComparison.InvariantCultureIgnoreCase))
					mode = FogMode.FOG_EXP2;
				else if (sMode.Equals("Linear", StringComparison.InvariantCultureIgnoreCase))
					mode = FogMode.FOG_LINEAR;

				if (mode != FogMode.FOG_NONE) {
					sceneMgr.SetFog(
						mode,
						def.GetQuatProperty("FogColour", Quaternion.IDENTITY).ToColourValue(),
						0.001f,
						def.GetFloatProperty("FogStart", 20),
						def.GetFloatProperty("FogEnd", 100));
				}

#if DEBUG
				// make some axes
				LKernel.GetG<Spawner>().Spawn("Axis", Vector3.ZERO);
#endif
			}
			else if (eventArgs.NewLevel.Type == LevelType.Menu) {
				LKernel.GetG<SceneManager>().AmbientLight = new ColourValue(1, 1, 1);
			}
		}
	}
}

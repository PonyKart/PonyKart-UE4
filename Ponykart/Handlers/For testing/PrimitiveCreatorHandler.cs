using Mogre;
using MOIS;
using Ponykart.Core;
using Ponykart.Players;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class PrimitiveCreatorHandler : ILevelHandler {

		public PrimitiveCreatorHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += ShootPrimitive;
		}

		void ShootPrimitive(KeyEvent ke) {
#if Debug
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			if (ke.key == KeyCode.KC_B) {
				string type;
				switch ((int) Math.RangeRandom(0, 5)) {
					case 0:
					default:
						type = "Box"; break;
					case 1:
						type = "Sphere"; break;
					case 2:
						type = "Cylinder"; break;
					case 3:
						type = "Cone"; break;
					case 4:
						type = "Capsule"; break;
				}
				Vector3 pos = LKernel.GetG<PlayerManager>().MainPlayer.NodePosition + Vector3.UNIT_Y;

				LKernel.GetG<Spawner>().Spawn(type, pos);
			}
#endif
		}

		public void Detach() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything -= ShootPrimitive;
		}
	}
}

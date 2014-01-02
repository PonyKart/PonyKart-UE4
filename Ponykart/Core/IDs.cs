using System;
namespace Ponykart {
	/// <summary>
	/// Handles getting ID numbers
	/// </summary>
	public abstract class IDs {
		private static uint counter = 0;
		private static readonly Random rand = new Random();

		/// <summary>
		/// Just get this property and it'll give you a new ID number.
		/// Note that it doesn't reset to 0 when we load a new level. (Should we fix this?)
		/// </summary>
		/// <example>
		/// something.IDNumber = IDs.New;
		/// </example>
		public static uint Incremental {
			get {
				return counter++;
			}
		}

		public static int Random {
			get {
				return rand.Next();
			}
		}

		public static float RandomFloat {
			get {
				return (float) rand.NextDouble();
			}
		}
	}
}

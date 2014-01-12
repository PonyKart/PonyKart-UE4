using System;
using System.Collections.Generic;
using System.Linq;

namespace Ponykart {
	/// <summary>
	/// "Swallowing" input is when we only give input to one particular thing. For example, if we have WASD as movement keys,
	/// if we have a text box come up asking us for our name or something, we don't want to start moving while we're doing that.
	/// We would say that text box has "swallowed" the input. This class manages that.
	/// 
	/// Sexual references ahoy.
	/// 
	/// Oh and pausing also swallows input. Might want to do something about that later.
	/// </summary>
	public class InputSwallowerManager {
		
		private IDictionary<Func<bool>, object> ThingsToCheck;

		public InputSwallowerManager() {
			ThingsToCheck = new Dictionary<Func<bool>, object>();
		}

		/// <summary>
		/// Add an object that has something that can be swallowed. If it's part of a UI, this should be the class that manages that UI thingy.
		/// The function should be read as "if this is true, then swallow the input".
		/// </summary>
		/// <param name="condition">A condition for when the input should be swallowed. If this is true, it is swallowed.</param>
		/// <param name="swallower">
		/// The object that "manages" the thing doing the swallowing.
		/// For example if the swallower is part of the UI, this should be that UI thingy's "manager".
		/// </param>
		public void AddSwallower(Func<bool> condition, object swallower) {
			ThingsToCheck.Add(condition, swallower);
		}

		/// <summary>
		/// Is the current input swallowed or not, with respect to me?
		/// </summary>
		/// <param name="querier">
		/// For the most part, just use the keyword "this".
		/// The thing asking whether the input is swallowed or not.
		/// This is needed because otherwise nothing would be able to do anything if the input was swallowed.
		/// So when we're checking the conditions, we excude any that are managed by the querier.
		/// </param>
		/// <returns>Returns true if the input is swallowed by something else, false otherwise.</returns>
		public bool IsSwallowed(object querier) {
			bool result = false;
			// we go through our conditions to check
			foreach (KeyValuePair<Func<bool>, object> pair in ThingsToCheck.Where((p) => p.Value != querier)) {
				// and we don't count things that are managed by the querier
				// We OR the conditions with the result. If any of the conditions are true, the input is swallowed.
				result |= pair.Key.Invoke();
			}
			return result;
		}

		/// <summary>
		/// Is the current input swallowed or not? This method is the same as IsSwallowed(object) but without the extra condition.
		/// This should be used by objects that do not swallow input themselves.
		/// </summary>
		/// <returns>Returns true if the input is swallowed by something, false otherwise.</returns>
		public bool IsSwallowed() {
			bool result = false;
			// we go through our conditions to check
			foreach (Func<bool> f in ThingsToCheck.Keys) {
				// we OR the conditions with the result. If any of the conditions are true, the input is swallowed.
				result |= f.Invoke();
			}
			return result;
		}
	}
}

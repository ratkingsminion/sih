using UnityEngine;
using UnityEngine.Events;

namespace RatKing.SIH {

	public class InteractableSimple : Interactable {
		//[SerializeField] UltEvents.UltEvent<ActorController> onSimpleInteract = null;
		[SerializeField] UnityEvent<GameObject> onInteractWithSource = null;
		[SerializeField] UnityEvent onInteract = null;

		//

		public override bool InteractSimple(GameObject source, float input) {
			if (!base.InteractSimple(source, input)) { return false; }
			onInteractWithSource?.Invoke(source);
			onInteract?.Invoke();
			return true;
		}
	}

}
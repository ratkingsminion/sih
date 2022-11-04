using UnityEngine;
using UnityEngine.Events;

namespace RatKing.SIH {

	public class InteractableComplex : Interactable {
		[SerializeField] float holdTime = float.PositiveInfinity;
		[SerializeField] bool needsSource = true;
		[SerializeField] bool onlyOneSource = true;
		[SerializeField] UnityEvent<GameObject> onInteractStartWithSource = null;
		[SerializeField] UnityEvent onStartInteract = null;
		[SerializeField] UnityEvent<Vector3> onInteractWithInput = null;
		[SerializeField] UnityEvent onInteract = null;
		[SerializeField] UnityEvent<bool> onEndInteractWithSuccess = null;
		[SerializeField] UnityEvent onEndInteract = null;
		//
		bool hasSource;
		GameObject curSource;

		//

		public override InteractableType InteractableType => InteractableType.Long;
		public override float InteractableHoldTime => holdTime;

		public override bool InteractComplex(GameObject source, Vector3 input) {
			if (!needsSource || curSource == source) {
				onInteractWithInput?.Invoke(input);
				onInteract?.Invoke();
			}
			return true;
		}

		public override void InteractComplexStart(GameObject source) {
			if (onlyOneSource && curSource != null) {
				return;
			}
			if (needsSource) {
				hasSource = source != null;
				curSource = source;
			}
			if (!needsSource || hasSource) {
				onInteractStartWithSource?.Invoke(source);
				onStartInteract?.Invoke();
			}
		}

		public override void InteractComplexStop(GameObject source, bool success) {
			if (!needsSource || curSource == source) {
				onEndInteractWithSuccess?.Invoke(success);
				onEndInteract?.Invoke();
				hasSource = false;
				curSource = null;
			}
		}
	}

}
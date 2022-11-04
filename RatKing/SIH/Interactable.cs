using UnityEngine;

namespace RatKing.SIH {

	// TODO: visible states for activated/deactivated (i.e., available for interaction)

	public enum InteractableType { Uninteractable = 0, Short = 1, Long = 2, ShortAndLong = ~0 }; // , Continuous = 4 }

	public class Interactable : MonoBehaviour {

		// SIGNALS

		public static readonly Base.TargetedSignal<Interactable, GameObject, float> INTERACT = new Base.TargetedSignal<Interactable, GameObject, float>();

		//

		[SerializeField] bool usableOnStart = true;
		public bool IsUsable { get; private set; } = true;
		
		//

		protected virtual void Awake() {
			SetUsable(usableOnStart);
		}

		public virtual void SetUsable(bool usable) {
			IsUsable = usable;
		}

		// optionally call this before using any Interact*() method
		public virtual bool CanInteract(GameObject source) {
			return isActiveAndEnabled && IsUsable;
		}

		//

		public virtual InteractableType InteractableType => InteractableType.Short;
		public virtual float InteractableHoldTime => float.PositiveInfinity;

		public virtual bool InteractComplex(GameObject source, Vector3 input) {
			return InteractSimple(source, input.magnitude);
		}
		public virtual bool InteractSimple(GameObject source, float input) {
			INTERACT.Broadcast(this, source, input);
			return true;
		}
		public virtual void InteractComplexStart(GameObject source) { }
		public virtual void InteractComplexStop(GameObject source, bool success) { }

		//

#if UNITY_EDITOR
		protected virtual void OnDrawGizmos() {
			Gizmos.color = ((!Application.isPlaying && usableOnStart) || (Application.isPlaying && IsUsable) ? Color.green : Color.red).WithAlpha(0.5f);
			Gizmos.DrawCube(transform.position, Vector3.one * 0.65f);
		}
#endif
	}

}

using UnityEngine;

namespace RatKing.SIH {
		
	[System.Flags] public enum InteractType { Invalid = 0, Instant = 1, Delayed = 2, All = 3 }

	public enum InteractDelayedPhase { Start, Success, Canceled }

	public abstract class DelayedInputBehaviour : MonoBehaviour {

		public enum Phase { Start, Hold, Stop, Instant }
		
		public const float INSTANT_INTERACT_TIME = 0.2f;
		
		// SIGNALS

		// target, source, factor
		public static readonly Base.TargetedSignal<DelayedInputBehaviour, GameObject, float> INPUT = new Base.TargetedSignal<DelayedInputBehaviour, GameObject, float>();
		
		//

		[SerializeField, Tooltip("Only used if this behaviour actually allows delayed interaction! Set to Infinity if the object can be used forever (e.g. spinning wheel)")]
		protected float delayedInputTime = -1f;
		public float DelayedInputTime => delayedInputTime;
		//
		float curInputTime = -1f;
		float curInputTimeAdd = 0f;

		//

		protected void ApplyInput(GameObject source, Phase phase) {
			var allowInstant = IsInputAllowed(source, InteractType.Instant);
			var allowDelayed = IsInputAllowed(source, InteractType.Delayed);
			if (!allowInstant && !allowDelayed) { return; }
			var delayedIsInfinite = allowDelayed && float.IsInfinity(delayedInputTime);
			//Debug.Log("interact with " + name + " " + phase + " " + Time.frameCount, this);
			switch (phase) {
				case Phase.Start:
					curInputTime = curInputTimeAdd = 0f;
					if (allowInstant && !allowDelayed) {
						OnInputInstant(source);
						curInputTime = -1f;
					}
					else if (allowDelayed && !allowInstant) {
						OnInputDelayed(source, InteractDelayedPhase.Start);
						SetInputTime(source, 9f);
					}
					else { // both instant and delayed
						curInputTimeAdd = INSTANT_INTERACT_TIME;
					}
					break;
				case Phase.Hold:
					if (allowDelayed && curInputTime >= 0f) {
						var nextInputTime = curInputTime + Time.unscaledDeltaTime;
						if (allowInstant && curInputTime <= curInputTimeAdd && nextInputTime > curInputTimeAdd) {
							OnInputDelayed(source, InteractDelayedPhase.Start);
							if (!delayedIsInfinite && delayedInputTime != 0f) { INPUT.Broadcast(this, source, 0f); }
						}
						else if (!delayedIsInfinite && curInputTime <= delayedInputTime + curInputTimeAdd && nextInputTime > delayedInputTime + curInputTimeAdd) {
							//Debug.Log("SUCCESS");
							OnInputDelayed(source, InteractDelayedPhase.Success);
							SetInputTime(source, nextInputTime = -1f);
						}
						else if (!delayedIsInfinite && curInputTime > curInputTimeAdd && curInputTime < delayedInputTime + curInputTimeAdd) {
							//Debug.Log("HOLD " + curInputTime);
							if (delayedInputTime > 0f) { INPUT.Broadcast(this, source, (curInputTime - curInputTimeAdd) / delayedInputTime); }
						}
						curInputTime = nextInputTime;
					}
					break;
				case Phase.Stop:
					if (curInputTime >= 0f) {
						if (allowInstant && curInputTime <= INSTANT_INTERACT_TIME) {
							OnInputInstant(source);
						}
						if (allowDelayed) {
							if (curInputTime < delayedInputTime + curInputTimeAdd) {
								OnInputDelayed(source, InteractDelayedPhase.Canceled);
								SetInputTime(source, -1f);
							}
							else {
								OnInputDelayed(source, InteractDelayedPhase.Success);
								SetInputTime(source, -1f);
							}
						}
					}
					break;
				case Phase.Instant:
					if (allowInstant) {
						OnInputInstant(source);
						curInputTime = -1f;
					}
					else if (allowDelayed) {
						OnInputDelayed(source, InteractDelayedPhase.Success);
						SetInputTime(source, -1f);
					}
					break;
			}
		}

		protected void SetInputTime(GameObject source, float time) {
			if (delayedInputTime != 0f && !float.IsInfinity(delayedInputTime)) { INPUT.Broadcast(this, source, Mathf.Clamp01(time / delayedInputTime)); }
			curInputTime = time;
		}

		//

		public abstract bool IsInputAllowed(GameObject source, InteractType type);
		public abstract void OnInputInstant(GameObject source);
		public abstract void OnInputDelayed(GameObject source, InteractDelayedPhase phase);
	}

}

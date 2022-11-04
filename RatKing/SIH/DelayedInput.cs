
using UnityEngine;

namespace RatKing.SIH {

	public class DelayedInput : DelayedInputBehaviour {

		public enum InputCheck { None, Start, Hold, Stop, Instant }

#if USE_REWIRED
		int button = -1;
#else
		string button = null;
#endif
		bool hasInstant, hasHold;
		System.Action actionInstant = null;
		System.Action actionStart = null;
		System.Action actionHold = null;
		System.Action<bool> actionStop = null; // bool success
		bool holdActionIsInfinite = false;
		bool cleared, started;
		int usedFrame = -1;
		(Interactable cur, InteractableType type) setInteractable = (null, 0);

		//

#if USE_REWIRED
		public static DelayedInput Create(GameObject target, int button) {
#else
		public static DelayedInput Create(GameObject target, string button) {
#endif
			var di = target.AddComponent<DelayedInput>();
			di.button = button;
			return di;
		}

		//

		public DelayedInput Use() {
			usedFrame = Time.frameCount;
			return this;
		}

		public DelayedInput Clear() {
			ClearInstant();
			ClearHold();
			cleared = true;
			setInteractable = (null, 0);
			return Use();
		}

		public DelayedInput SetInstant(string function, System.Action action) {
			if (action == null) { return this; }
			if (actionInstant == action) { return this; }
			WrappedInput.ADD_HINT_BUTTON.Broadcast(button, function);
			actionInstant = action;
			hasInstant = true;
			if (setInteractable.cur != null && (setInteractable.type & InteractableType.Short) != 0) { setInteractable = (null, 0); }
			return this;
		}

		public DelayedInput ClearInstant() {
			if (hasInstant) {
				WrappedInput.REMOVE_HINT_BUTTON.Broadcast(button);
				actionInstant = null;
				hasInstant = false;
				if (setInteractable.cur != null && (setInteractable.type & InteractableType.Short) != 0) { setInteractable = (null, 0); }
			}
			return this;
		}

		public DelayedInput SetHold(string function, System.Action actionStart, float delayTime = float.PositiveInfinity) {
			return SetHold(function, actionStart, null, null, delayTime);
		}

		public DelayedInput SetHold(string function, System.Action<bool> actionStop, float delayTime = float.PositiveInfinity) {
			return SetHold(function, null, null, actionStop, delayTime);
		}

		public DelayedInput SetHold(string function, System.Action actionStart, System.Action<bool> actionStop, float delayTime = float.PositiveInfinity) {
			return SetHold(function, actionStart, null, actionStop, delayTime);
		}

		public DelayedInput SetHold(string function, System.Action actionStart, System.Action actionHold, System.Action<bool> actionStop, float delayTime = float.PositiveInfinity) {
			if (actionStart == null && actionHold == null && actionStop == null) { return this; }
			if (this.actionStart == actionStart && this.actionHold == actionHold && this.actionStop == actionStop) { return this; }
			ClearHold();
			if (float.IsInfinity(delayTime)) {
				holdActionIsInfinite = true;
				WrappedInput.ADD_HINT_BUTTON_FOREVER.Broadcast(button, function);
			}
			else {
				holdActionIsInfinite = false;
				WrappedInput.ADD_HINT_BUTTON_DELAYED.Broadcast(button, delayTime, function);
			}
			delayedInputTime = delayTime;
			this.actionStart = actionStart;
			this.actionHold = actionHold;
			this.actionStop = actionStop;
			hasHold = true;
			if (setInteractable.cur != null && (setInteractable.type & InteractableType.Long) != 0) { setInteractable = (null, 0); }
			return this;
		}

		public DelayedInput SetInteractable(Interactable interactable, string functionShort = null, string functionLong = null) {
			if (setInteractable.cur == interactable) { return this; }
			if (setInteractable.cur != null) { Clear(); }
			setInteractable = (null, 0); 
			if (interactable == null) { return this; }

			if ((interactable.InteractableType & InteractableType.Short) != 0) {
				SetInstant(functionShort, () => {
					if (interactable != null) { interactable.InteractSimple(gameObject, 1f); }
				});
			}
			if ((interactable.InteractableType & InteractableType.Long) != 0) {
				SetHold(functionLong, () => {
					if (interactable != null) { interactable.InteractComplexStart(gameObject); }
				}, () => {
					if (interactable != null) { interactable.InteractComplex(gameObject, Vector3.one); }
				}, success => {
					if (interactable != null) { interactable.InteractComplexStop(gameObject, success); }
				}, interactable.InteractableHoldTime);
			}

			setInteractable.cur = interactable;
			setInteractable.type = interactable.InteractableType;

			return this;
		}

		public DelayedInput ClearHold() {
			if (hasHold) {
				if (holdActionIsInfinite) { WrappedInput.REMOVE_HINT_BUTTON_FOREVER.Broadcast(button); }
				else { WrappedInput.REMOVE_HINT_BUTTON_DELAYED.Broadcast(button); }
				// TODO: need? Base.Updater.Remove(OnHold, false);
				actionStart = null;
				actionHold = null;
				actionStop = null;
				holdActionIsInfinite = false;
				hasHold = false;
				SetInputTime(gameObject, -1f);

				if (setInteractable.cur != null && (setInteractable.type & InteractableType.Long) != 0) { setInteractable = (null, 0); }
			}
			return this;
		}

		public bool CheckDown(bool mustHaveEffect = false) {
			if (usedFrame == Time.frameCount) { return false; }
			if (mustHaveEffect && !hasInstant) { return false; }

			if (WrappedInput.GetButtonDown(button)) {
				cleared = false;
				ApplyInput(gameObject, Phase.Start);
				return true;
			}

			return false;
		}

		public bool CheckAny(bool mustHaveEffect = false) {
			if (usedFrame == Time.frameCount) { return false; }
			if (mustHaveEffect && !hasInstant && !hasHold) { return false; }

			if (WrappedInput.GetButtonDown(button)) {
				cleared = false;
				ApplyInput(gameObject, Phase.Start);
				return true;
			}
			else if (!cleared && hasHold && WrappedInput.GetButtonUp(button)) {
				ApplyInput(gameObject, Phase.Stop);
				return true;
			}
			else if (!cleared && hasHold && WrappedInput.GetButton(button)) {
				ApplyInput(gameObject, Phase.Hold);
				return true;
			}

			return false;
		}

		public InputCheck Check(bool mustHaveEffect = false) {
			if (usedFrame == Time.frameCount) { return InputCheck.None; }
			if (mustHaveEffect && !hasInstant && !hasHold) { return InputCheck.None; }

			var result = InputCheck.None;

			if (WrappedInput.GetButtonDown(button)) {
				ApplyInput(gameObject, Phase.Start);
				result = InputCheck.Start;
			}
			else if (WrappedInput.GetButtonUp(button)) {
				ApplyInput(gameObject, Phase.Stop);
				result = InputCheck.Stop;
			}
			else if (WrappedInput.GetButton(button)) {
				ApplyInput(gameObject, Phase.Hold);
				result = InputCheck.Hold;
			}

			return result;
		}

		public bool IsButtonPressed() {
			return WrappedInput.GetButton(button);
		}

		//

		public override bool IsInputAllowed(GameObject source, InteractType type) {
			if ((type & InteractType.Instant) != 0 && !hasInstant) { return false; }
			if ((type & InteractType.Delayed) != 0 && !hasHold) { return false; }
			return true;
		}

		public override void OnInputDelayed(GameObject source, InteractDelayedPhase phase) {
			// Debug.Log(button + " OnInputDelayed " + source + " .. " + phase);
			if (!hasHold) { return; }
			switch (phase) {
				case InteractDelayedPhase.Start:
					started = true;
					actionStart?.Invoke();
					Base.Updater.Add(source, OnHold);
					break;
				case InteractDelayedPhase.Success:
					Base.Updater.Remove(OnHold); // TODO: necessary?
					if (started && !holdActionIsInfinite) { actionStop?.Invoke(true); }
					started = false;
					break;
				case InteractDelayedPhase.Canceled:
					Base.Updater.Remove(OnHold);
					if (holdActionIsInfinite) { actionStop?.Invoke(started); }
					else if (started) { actionStop?.Invoke(false); }
					started = false;
					break;
			}
		}

		public override void OnInputInstant(GameObject source) {
			actionInstant?.Invoke();
		}

		//

		bool OnHold() {
			if (!hasHold || actionHold == null) { return false; }
			actionHold();
			return true;
		}
	}

}

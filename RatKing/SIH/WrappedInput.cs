using UnityEngine;

namespace RatKing.SIH {

	public class WrappedInput : MonoBehaviour {

		// SIGNALS
		
#if USE_REWIRED
		// based on RewiredConsts.Category
		public static readonly Base.Signal<int> CHANGE_SCHEME = new Base.Signal<int>();

		// int button, string function
		public static readonly Base.Signal<int, string> ADD_HINT_BUTTON = new Base.Signal<int, string>();
		public static readonly Base.Signal<int, float, string> ADD_HINT_BUTTON_DELAYED = new Base.Signal<int, float, string>();
		public static readonly Base.Signal<int, string> ADD_HINT_BUTTON_FOREVER = new Base.Signal<int, string>();
		public static readonly Base.Signal<int> REMOVE_HINT_BUTTON = new Base.Signal<int>();
		public static readonly Base.Signal<int> REMOVE_HINT_BUTTON_DELAYED = new Base.Signal<int>();
		public static readonly Base.Signal<int> REMOVE_HINT_BUTTON_FOREVER = new Base.Signal<int>();
#else
		// string button, string function
		public static readonly Base.Signal<string, string> ADD_HINT_BUTTON = new Base.Signal<string, string>();
		public static readonly Base.Signal<string, float, string> ADD_HINT_BUTTON_DELAYED = new Base.Signal<string, float, string>();
		public static readonly Base.Signal<string, string> ADD_HINT_BUTTON_FOREVER = new Base.Signal<string, string>();
		public static readonly Base.Signal<string> REMOVE_HINT_BUTTON = new Base.Signal<string>();
		public static readonly Base.Signal<string> REMOVE_HINT_BUTTON_DELAYED = new Base.Signal<string>();
		public static readonly Base.Signal<string> REMOVE_HINT_BUTTON_FOREVER = new Base.Signal<string>();
#endif

		//
		
#if USE_REWIRED
		static Rewired.Player player = null;
		public static Rewired.Player Player => player != null ? player : player = Rewired.ReInput.players.GetPlayer(0);
		public static bool UsingGamepad { get; private set; } = true;
		public static bool UsingMouseAndKeyboard { get; private set; }
		public static int CurScheme { get; private set; } = 0; // based on RewiredConsts.Category
		//
		Vector3 lastMousePos;
		double lastTimeSwitch;
#endif

		//
		
#if USE_REWIRED
		public static bool GetButtonDown(int button) {
			return Player.GetButtonDown(button);
		}

		public static bool GetButton(int button) {
			return Player.GetButton(button);
		}

		public static bool GetButtonUp(int button) {
			return Player.GetButtonUp(button);
		}

		public static float GetAxis(int axis, bool raw = true) {
			if (raw) { return Player.GetAxisRaw(axis); }
			return Player.GetAxis(axis);
		}

		public static bool GetButtonDown(string button) {
			return Player.GetButtonDown(button);
		}

		public static bool GetButton(string button) {
			return Player.GetButton(button);
		}

		public static bool GetButtonUp(string button) {
			return Player.GetButtonUp(button);
		}

		public static float GetAxis(string axis, bool raw = true) {
			if (raw) { return Player.GetAxisRaw(axis); }
			return Player.GetAxis(axis);
		}
#else
		public static bool GetButtonDown(string button) {
			return Input.GetButtonDown(button);
		}

		public static bool GetButton(string button) {
			return Input.GetButton(button);
		}

		public static bool GetButtonUp(string button) {
			return Input.GetButtonUp(button);
		}

		public static float GetAxis(string axis, bool raw = true) {
			if (raw) { return Input.GetAxisRaw(axis); }
			return Input.GetAxis(axis);
		}
#endif

		//
		
#if USE_REWIRED
		void OnDestroy() {
			player = null;
		}
		
		void Update() {
			if (Player == null) { return; }

			if (Input.mousePosition != lastMousePos) {
				UsingMouseAndKeyboard = true;
				UsingGamepad = false;
				lastTimeSwitch = Rewired.ReInput.time.unscaledTime;
			}

			if (lastTimeSwitch < Rewired.ReInput.time.unscaledTime) {
				foreach (var j in Player.controllers.Joysticks) {
					if (j.GetLastTimeAnyAxisChanged() > lastTimeSwitch) {
						UsingGamepad = true;
						UsingMouseAndKeyboard = false;
						lastTimeSwitch = Rewired.ReInput.time.unscaledTime;
					}
				}
			}
			
			if (lastTimeSwitch < Rewired.ReInput.time.unscaledTime) {
				if (Rewired.ReInput.controllers.GetAnyButton(Rewired.ControllerType.Mouse) || Rewired.ReInput.controllers.GetAnyButton(Rewired.ControllerType.Keyboard)) {
					UsingGamepad = false;
					UsingMouseAndKeyboard = true;
					lastTimeSwitch = Rewired.ReInput.time.unscaledTime;
				}
				else if (Rewired.ReInput.controllers.GetAnyButton(Rewired.ControllerType.Joystick)) {
					UsingGamepad = true;
					UsingMouseAndKeyboard = false;
					lastTimeSwitch = Rewired.ReInput.time.unscaledTime;
				}
			}

			lastMousePos = Input.mousePosition;
		}

		//

		// call with RewiredConsts.Category.XYZ
		public static void SwitchScheme(int newScheme) {
			if (CurScheme == newScheme) { return; }

			CHANGE_SCHEME.Broadcast(newScheme);

			Player.controllers.maps.SetMapsEnabled(false, CurScheme);
			Player.controllers.maps.SetMapsEnabled(true, newScheme);
			
			CurScheme = newScheme;
		}
#endif
	}

}

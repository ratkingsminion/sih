# sih
Simple input-related helpers for Unity, supports legacy Input and Rewired

Example usage:

```C#
public class Player : MonoBehaviour {
	DelayedInput inputJump;
	DelayedInput inputInteract;
	
	void OnEnable() {
		if (inputJump == null) { inputJump = DelayedInput.Create(gameObject, "jump"); }
		if (inputInteract == null) { inputInteract = DelayedInput.Create(gameObject, "interact"); }
		inputJump.SetInstant("player/jump", () => { GetComponent<Rigidbody>().AddForce(0f, 20f, 0f, ForceMode.Impulse));
	}
	
	void OnDisable() {
		inputInteract.Clear();
		inputJump.Clear();
	}
	
	void Update() {
		// interacting with the world elements
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hit, 2f, LayerMask.GetLayer("Default", "Interactable"), QueryTriggerInteraction.Ignore)
					&& hit.TryGetComponent(hit.collider, out Interactable interactable)
					&& interactable.CanInteract(gameObject)) {
			// TODO: highlight hovered interactable
			inputInteract.SetInteractable(interactable, "player/interact_short", "player/interact_long");
		}
		else {
			inputInteract.SetInteractable(null);
		}
		
		if (inputInteract.CheckAny()) { return; }
		inputJump.Check();
	}
}
```

To enable support for Rewired, add USE_REWIRED to the project's Scripting Define Symbols (in the Player settings).

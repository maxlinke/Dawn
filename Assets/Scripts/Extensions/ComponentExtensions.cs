using UnityEngine;

public static class ComponentExtensions {

    public static void SetGOActive (this Component component, bool value) {
        component.gameObject.SetActive(value);
    }
	
}

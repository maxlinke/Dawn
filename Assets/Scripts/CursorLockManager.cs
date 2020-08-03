using System.Collections.Generic;
using UnityEngine;

public static class CursorLockManager {

    // doesn't need a monobehaviour instance that listens to OnApplicationFocus because the lockstate is automatically set to what it is upon tabbing back in.
    // the purpose of this: i don't want the player class to mess with the cursor lock state
    // yes, the pause menu has to SHOW the cursor and thus unlock it but closing it MIGHT not have to lock the cursor, if you know what i mean. 
    // i can't currently think of any reasons why there should be multiple layers of cursor unlockage but i can always remove this thing if it's pointless
    // one good reason could be a bona fide CONSOLE but i don't have that yet :)
    // the player would also have to then check if the debug console was open
    // OR the input system would have to be "blocked" (override all commands ? force a getkeyup for everything and then just falses... sounds like a lot of work)

    private static List<object> unlockObjects;

    static CursorLockManager () {
        unlockObjects = new List<object>();
    }

    // this should be the only place where Cursor.lockState is used
    public static void UpdateLockState () {
        if(unlockObjects.Count > 0){
            Cursor.lockState = CursorLockMode.None;
        }else{
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
	
    public static bool AddUnlocker (object unlocker) {      // TODO some kind of identifier would be nice, like a name
        if(unlockObjects.Contains(unlocker)){               // i can't save a tuple tho. i'd have to stash the name in a dictionary i guess
            return false;                                   // or use another list and with each add or remove match indices...
        }
        unlockObjects.Add(unlocker);
        UpdateLockState();
        return true;
    }

    public static bool RemoveUnlocker (object unlocker) {
        if(unlockObjects.Remove(unlocker)){
            UpdateLockState();
            return true;
        }
        return false;
    }

    public static void ForceCursorLock () {
        if(unlockObjects.Count > 0){
            string message = $"Forcing a locked cursor but there were {unlockObjects.Count} objects still keeping it unlocked!";
            foreach(var obj in unlockObjects){
                message += $"\n-{obj.GetType()}";
            }
            unlockObjects.Clear();
        }
        UpdateLockState();
    }

    public static string GetLog () {
        var output = $"{nameof(Cursor.lockState)}: {Cursor.lockState}\n";
        if(unlockObjects.Count <= 0){
            output += "No unlocking objects";
        }else{
            output += $"{unlockObjects.Count} unlocking objects:";
            foreach(var obj in unlockObjects){
                output += $"\n-{obj.GetType()}";
            }
        }
        return output;
    }

}

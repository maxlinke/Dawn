using System.Collections.Generic;
using UnityEngine;

public static class CursorLockManager {

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

    public static bool CursorIsLocked () {
        return Cursor.lockState == CursorLockMode.Locked;
    }
	
    public static bool AddUnlocker (object unlocker) {
        if(unlockObjects.Contains(unlocker)){
            return false;
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

    public static bool IsUnlocker (object obj) {
        return unlockObjects.Contains(obj);
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

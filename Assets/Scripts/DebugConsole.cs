using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour {

    // TODO instance and stuff

    // plan: print things to an ingame debug console
    // for build-debugging and such
    
    private static string log = string.Empty;

    private static void AddToLog (string message) {
        log += $"{message}\n";
        // TODO if visible update tmp
        // otherwise update tmp on enable
    }

    public static void Log (object obj, bool alsoDebugLog = true) {
        AddToLog(obj.ToString());       // TODO tmp text coloring
        if(alsoDebugLog) Debug.Log(obj);
    }
    
	public static void LogWarning (object obj, bool alsoDebugLog = true) {
        AddToLog(obj.ToString());
        if(alsoDebugLog) Debug.LogWarning(obj);
    }

    public static void LogError (object obj, bool alsoDebugLog = true) {
        AddToLog(obj.ToString());
        if(alsoDebugLog) Debug.LogError(obj);
    }

    public static void Clear () {
        log = string.Empty;
    }

}

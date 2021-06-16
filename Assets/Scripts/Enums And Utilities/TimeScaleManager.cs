using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleManager {

    private static float initTimeScale;
    private static List<TimeScaler> timeScalers;
    public static event System.Action OnTimeScaleUpdated = delegate {};

    public static float timeScale => Time.timeScale;    // TODO make my scripts call this

    class TimeScaler {
        public readonly object caller;
        public float timeScale;
        public TimeScaler (object caller, float timeScale) {
            this.caller = caller;
            this.timeScale = timeScale;
        }
    }

    static TimeScaleManager () {
        initTimeScale = Time.timeScale;
        timeScalers = new List<TimeScaler>();
    }

    public static void ForceResetTimeScale () {
        if(timeScalers.Count > 0){
            Debug.LogWarning($"Force reset of time scale deleting {timeScalers.Count} time scalers!");
        }
        timeScalers.Clear();
        Time.timeScale = initTimeScale;
    }

    // TODO rigorously test all this
    public static bool AddTimeScaler (object caller, float newValue) {
        for(int i=0; i<timeScalers.Count; i++){
            if(timeScalers[i].caller == caller){
                Debug.LogWarning($"Time scaler \"{caller.ToString()}\" is already registered! Time scale won't be set to {newValue}!");
                return false;
            }
        }
        Time.timeScale = newValue;
        timeScalers.Add(new TimeScaler(caller, newValue));
        return true;
    }

    public static bool AdjustTimeScaler (object caller, float newValue) {
        for(int i=timeScalers.Count-1; i>=0; i--){
            if(timeScalers[i].caller == caller){
                timeScalers[i].timeScale = newValue;
                if(i == timeScalers.Count-1){
                    Time.timeScale = newValue;
                }else{
                    Debug.LogWarning($"Time scaler \"{caller.ToString()} ({caller.GetType()}) adjusted time scale but it's not immediately visible because it's not the last time scaler!");
                }
                return true;
            }
        }
        return false;
    }

    public static bool RemoveTimeScaler (object caller) {
        bool success = false;
        for(int i=timeScalers.Count-1; i>=0; i--){
            if(timeScalers[i].caller == caller){
                timeScalers.RemoveAt(i);
                success = true;
                break;
            }
        }
        if(success){
            if(timeScalers.Count > 0){
                Time.timeScale = timeScalers[timeScalers.Count - 1].timeScale;
            }else{
                Time.timeScale = initTimeScale;
            }
        }else{
            Debug.LogWarning($"Time scaler \"{caller.ToString()}\" wasn't registered (maybe there was a forced reset?)");
        }
        return success;
    }
	
}

using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleManager {

    private static float initTimeScale;
    private static List<TimeScaler> timeScalers;
    public static event System.Action OnTimeScaleUpdated = delegate {};

    public static float timeScale => Time.timeScale;    // TODO make my scripts call this

    class TimeScaler {
        public readonly object caller;
        public readonly float timeScale;
        public TimeScaler (object caller, float timeScale) {
            this.caller = caller;
            this.timeScale = timeScale;
        }
    }

    static TimeScaleManager () {
        initTimeScale = Time.timeScale;
        timeScalers = new List<TimeScaler>();
    }

    public static void ResetTimeScale () {
        timeScalers.Clear();
        Time.timeScale = initTimeScale;
    }

    // TODO rigorously test all this
    public static void AddTimeScaler (object caller, float newValue) {
        for(int i=0; i<timeScalers.Count; i++){
            if(timeScalers[i].caller == caller){
                Debug.LogWarning($"Time scaler \"{caller.ToString()}\" is already registered! Time scale won't be set to {newValue}!");
                return;
            }
        }
        Time.timeScale = newValue;
        timeScalers.Add(new TimeScaler(caller, newValue));
    }

    public static bool RemoveTimeScaler (object caller) {
        bool success = false;
        for(int i=0; i<timeScalers.Count; i++){
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
            Debug.LogWarning($"Time scaler \"{caller.ToString()}\" wasn't register (maybe there was a forced reset?)");
        }
        return success;
    }
	
}

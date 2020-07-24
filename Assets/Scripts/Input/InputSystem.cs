using System.Collections.Generic;
using UnityEngine;

// GIVE THIS A NEGATIVE INDEX IN THE SCRIPT EXECUTION ORDER
// SO THAT THIS EXECUTES BEFORE ANYTHING ELSE!!!

// why am i doing all this shit? 
// so i can
// > bind look up to the mouse or the controller
// > bind things to the mousewheel or keys
// > or use the dpad and triggers for binds

public partial class InputSystem : MonoBehaviour {

    [SerializeField] bool resetAxisConfigToDefault = false;
    [SerializeField] bool resetKeybindsToDefault = false;

    private static InputSystem instance;

    int lastUpdatedFrame = -1;
    List<AxisInput> axisInputs = new List<AxisInput>();
    bool currentlyCheckingInputValidity = false;

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation, instance of {nameof(InputSystem)} is not null!!!");
            return;
        }
        instance = this;
        Test();
        Bind.Initialize();
        if(resetAxisConfigToDefault){
            Axis.Config.ResetToDefault();
        }
        if(resetKeybindsToDefault){
            Bind.ResetToDefault();
        }
        DebugConsole.Log($"Axis Log: \n{Axis.Config.GetLog()}");
        DebugConsole.Log($"Keybind Log: \n{Bind.GetLog()}");
    }

    void Test () {

    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    void Update () {
        EnsureAllInputsUpToDate();
    }

    void FixedUpdate () {
        EnsureAllInputsUpToDate();
    }

    void EnsureAllInputsUpToDate () {
        if(Time.frameCount == lastUpdatedFrame){
            return;
        }
        lastUpdatedFrame = Time.frameCount;
        foreach(var axisInput in axisInputs){
            axisInput.Update();
        }
    }

    static void BindsChanged (bool saveToDiskIfValid = false) {           // has to be static. trust me.
        if(instance == null){
            Debug.LogError($"SOMEBODY REALLY FUCKED UP SOMEWHERE! INSTANCE OF {nameof(InputSystem)} IS NULL!");
            return;
        }
        if(instance.currentlyCheckingInputValidity){
            return;
        }
        instance.currentlyCheckingInputValidity = true;
        DebugConsole.Log("Checking validity of binds");
        instance.axisInputs.Clear();
        var allInputs = new List<InputMethod>();
        var faultyInputs = new List<(Bind badBind, InputMethod badInput)>();
        RegisterAxisInputsAndFlagRecurringInputs();
        bool doARecursiveCallAtTheEnd;
        if(faultyInputs.Count > 0){
            doARecursiveCallAtTheEnd = true;
            RemoveRecurringInputs();
        }else{
            doARecursiveCallAtTheEnd = false;
            DebugConsole.Log("All binds valid");
            if(saveToDiskIfValid){
                Bind.SaveToDisk();
            }
        }
        instance.currentlyCheckingInputValidity = false;
        if(doARecursiveCallAtTheEnd){
            BindsChanged(true);
        }

        void RegisterAxisInputsAndFlagRecurringInputs () {
            foreach(var bind in Bind.Binds()){
                foreach(var input in bind){
                    if(input is AxisInput axisInput){
                        if(!instance.axisInputs.Contains(axisInput)){
                            instance.axisInputs.Add(axisInput);
                        }
                    }
                    if(allInputs.Contains(input)){
                        DebugConsole.LogError($"Detected duplicate usage of input \"{input}\"!");
                        faultyInputs.Add((bind, input));
                    }else{
                        allInputs.Add(input);
                    }
                }
            }
        }

        void RemoveRecurringInputs () {
            foreach(var faultyInput in faultyInputs){
                faultyInput.badBind.RemoveInput(faultyInput.badInput);  // would cause unintended recursion
            }
        }
    }

    public static bool IsAlreadyBound (InputMethod newInput, out Bind inputUsingBind) {
        foreach(var bind in Bind.Binds()){
            if(bind.UsesInput(newInput)){
                inputUsingBind = bind;
                return true;
            }
        }
        inputUsingBind = default;
        return false;
    }

    public static bool AnyInputHeld () {
        if(Input.anyKey){
            return true;
        }
        foreach(var axis in Axis.Axes()){
            if(Mathf.Abs(axis.GetRaw()) >= AxisInput.ANALOG_TO_BOOL_THRESHOLD){
                return true;
            }
        }
        return false;
    }

    public static InputMethod CurrentlyHeldInput () {
        foreach(var kc in KeyCodeUtils.KeyCodes()){
            if(Input.GetKey(kc)){
                return new KeyCodeInput(kc);
            }
        }
        foreach(var axis in Axis.Axes()){
            var rawVal = axis.GetRaw();
            if(Mathf.Abs(rawVal) >= AxisInput.ANALOG_TO_BOOL_THRESHOLD){
                return new AxisInput(axis.id, rawVal > 0);
            }
        }
        return null;
    }
	
}

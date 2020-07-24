using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GIVE THIS A NEGATIVE INDEX IN THE SCRIPT EXECUTION ORDER
// SO THAT THIS EXECUTES BEFORE ANYTHING ELSE!!!

// why am i doing all this shit? 
// so i can
// > bind look up to the mouse or the controller
// > bind things to the mousewheel or keys
// > use the dpad for binds

public partial class InputSystem : MonoBehaviour {

    private static InputSystem instance;

    // TODO some way for immutable binds... that should probably not show up in a binds menu... 
    // partially manual binds menu instead of a 100% generated one?

    // so, i can properly serialize the input methods, as they are based on enums, which serialize nicely.
    // to serialize the binds i'll have to figure something out. something that won't make me go mad from constantly having to update things
    // the class-enum-parallelity is probably the way to go again...
    // upon deserialization, get the enum, get the object for the enum (via switch) and apply stuff. yes.
    // then i also don't need the namelist stuff

    int lastUpdatedFrame = -1;

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation, instance of {nameof(InputSystem)} is not null!!!");
            return;
        }
        instance = this;
        Test();
        Bind.Initialize();
        Debug.Log(Bind.GetLog());
    }

    void Test () {
        // InputMethod a = new KeyCodeInput(KeyCode.Q);
        // // InputMethod b = new KeyCodeInput(KeyCode.Q);
        // InputMethod b = new AxisInput(Axis.ID.RIGHT_STICK_X, true);
        // var ja = JsonUtility.ToJson(new SaveableInputMethod(a));
        // var jb = JsonUtility.ToJson(new SaveableInputMethod(b));
        // JsonUtility.FromJson<SaveableInputMethod>(ja).TryRestoreInputMethod(out var na);
        // JsonUtility.FromJson<SaveableInputMethod>(jb).TryRestoreInputMethod(out var nb);
        // Debug.Log($"{na.Equals(a)}, {ja}");
        // Debug.Log($"{nb.Equals(b)}, {jb}");
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

    void ValidateNameList () {
        int issueCount = 0;
        List<Bind> issueIDs = new List<Bind>();
        foreach(var obj in System.Enum.GetValues(typeof(Bind))){
            if(GetName((Bind)obj) == null){
                issueCount++;
                issueIDs.Add((Bind)obj);
            }
        }
        if(issueCount > 0){
            string output = $"{issueCount} {nameof(Bind)}s in {nameof(InputSystem)} don't have corresponding names!";
            foreach(var issueID in issueIDs){
                output += $"\n - {issueID}";
            }
        }
    }

    void EnsureAllInputsUpToDate () {
        if(Time.frameCount == lastUpdatedFrame){
            return;
        }
        lastUpdatedFrame = Time.frameCount;
        // TODO big fukken todo

        // foreach(var inputs in binds.Values){
        //     foreach(var input in inputs){
        //         if(input is AxisInput axisInput){   	// TODO just add abstract update to inputmethod? 
        //             axisInput.Update();
        //         }
        //     }
        // }
    }

    public static bool IsAlreadyBound (InputMethod newInput, out Bind currentBind) {
        // TODO also this
        
        // foreach(var id in instance.binds.Keys){
        //     if(instance.binds[id].Contains(newInput)){
        //         currentBind = id;
        //         return true;
        //     }
        // }
        currentBind = default;
        return false;
    }

    public static void Set (Bind id, InputMethod newInput) {
        // if(!instance.binds.ContainsKey(id)){
        //     instance.binds.Add(id, newInput);
        // }else{
        //     instance.binds[id] = newInput;
        // }
    }

    // public static InputMethod Get (ID id) {
    //     return instance.inputs[id];
    // }

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

    // TODO i don't like this. i'd much prefer to return just a keycode/axis-bool-combo or something like that
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

    public static string GetName (Bind id) {
        switch(id){

            default: 
                // Debug.LogError($"Unknown {nameof(ID)} \"{id}\"!");
                return null;
        }
    }
	
}

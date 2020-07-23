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

    public const float ANALOG_TO_BOOL_THRESHOLD = 0.5f;
    public const int MAX_INPUTS_PER_BINDING = 2;

    private static InputSystem instance;

    private Dictionary<Bind, List<InputMethod>> binds;  // i don't like the list here...
    // TODO some way for immutable binds... that should probably not show up in a binds menu... 
    // partially manual binds menu instead of a 100% generated one?

    public enum Bind {
        GAME_PAUSE_UNPAUSE,
        PLAYER_MOVE_FWD,
        PLAYER_MOVE_BWD,
        PLAYER_MOVE_LEFT,
        PLAYER_MOVE_RIGHT
    }

    int lastUpdatedFrame = -1;

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation, instance of {nameof(InputSystem)} is not null!!!");
            return;
        }
        Test();
        ValidateNameList();
        instance = this;
        binds = new Dictionary<Bind, List<InputMethod>>();
    }

    void Test () {
        var list = new List<JSONableInput>();
        list.Add(new JSONableInput(Bind.PLAYER_MOVE_LEFT, new KeyCodeInput(KeyCode.Q)));
        list.Add(new JSONableInput(Bind.PLAYER_MOVE_RIGHT, new AxisInput(Axis.RIGHT_STICK_X, true)));
        var col = new JSONableInputCollection();
        col.jsonableInputs = list.ToArray();
        Debug.Log(JsonUtility.ToJson(col));
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
        foreach(var inputs in binds.Values){
            foreach(var input in inputs){
                if(input is AxisInput axisInput){   	// TODO just add abstract update to inputmethod? 
                    axisInput.Update();
                }
            }
        }
    }

    public static bool IsAlreadyBound (InputMethod newInput, out Bind currentBind) {
        foreach(var id in instance.binds.Keys){
            if(instance.binds[id].Contains(newInput)){
                currentBind = id;
                return true;
            }
        }
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
        foreach(var axisID in Axes.AxisIDs()){
            if(Mathf.Abs(Axes.GetAxisRaw(axisID)) >= ANALOG_TO_BOOL_THRESHOLD){
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
        foreach(var axisID in Axes.AxisIDs()){
            var rawVal = Axes.GetAxisRaw(axisID);
            if(Mathf.Abs(rawVal) >= ANALOG_TO_BOOL_THRESHOLD){
                return new AxisInput(axisID, rawVal > 0);
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

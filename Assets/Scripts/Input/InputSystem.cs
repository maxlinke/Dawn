using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GIVE THIS A NEGATIVE INDEX IN THE SCRIPT EXECUTION ORDER
// SO THAT THIS EXECUTES BEFORE ANYTHING ELSE!!!

// the input methods indivudually can be easily JSONed and unJSONed
// but you have to know which type it is to properly deserialize it
// best case: it throws an exception, worst case, it just does it. wrongly.

public class InputSystem : MonoBehaviour {

    public const float ANALOG_TO_BOOL_THRESHOLD = 0.5f;

    private static InputSystem instance;

    private List<AxisInput> axisInputs;
    private Dictionary<ID, InputMethod> inputs;

    public enum ID {
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
        instance = this;
        inputs = new Dictionary<ID, InputMethod>();
        axisInputs = new List<AxisInput>();
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

    public void EnsureAllInputsUpToDate () {
        if(Time.frameCount == lastUpdatedFrame){
            return;
        }
        lastUpdatedFrame = Time.frameCount;
        foreach(var axisInput in axisInputs){
            axisInput.Update();
        }
    }

    public static void Set (ID id, InputMethod newInput) {

    }

    public static InputMethod Get (ID id) {
        return instance.inputs[id];
    }

    public static bool AnyInputHeld () {
        if(Input.anyKey){
            return true;
        }
        foreach(var axisID in Axes.IDs()){
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
        foreach(var axisID in Axes.IDs()){
            var rawVal = Axes.GetAxisRaw(axisID);
            if(Mathf.Abs(rawVal) >= ANALOG_TO_BOOL_THRESHOLD){
                return new AxisInput(axisID, rawVal > 0);
            }
        }
        return null;
    }

    public abstract class InputMethod {
        
        public abstract bool Down { get; }
        public abstract bool Hold { get; }
        public abstract bool Up { get; }
        
        public abstract float Value { get; }
        public abstract string Name { get; }
    }

    public class KeyCodeInput : InputMethod {
        
        public KeyCode keyCode;
        
        public KeyCodeInput (KeyCode keyCode) : base () {
            this.keyCode = keyCode;
        }
        
        public override bool Down => Input.GetKeyDown(keyCode);
        public override bool Hold => Input.GetKey(keyCode);
        public override bool Up => Input.GetKeyUp(keyCode);
        
        public override float Value => Hold ? 1f : 0f;
        public override string Name => KeyCodeUtils.ToNiceString(this.keyCode);

        public override bool Equals (object obj) {
            if(obj is KeyCodeInput other){
                return this.keyCode.Equals(other.keyCode);
            }
            return false;
        }

        public override int GetHashCode () {
            return base.GetHashCode();
        }

        public override string ToString () {
            return $"({nameof(KeyCodeInput)}) [{keyCode.ToString()}]";
        }
    }

    public class AxisInput : InputMethod {
        
        public Axes.ID axisID;
        public bool positive;
        
        public AxisInput (Axes.ID axisID, bool positive) : base () {
            this.axisID = axisID;
            this.positive = positive;
        }

        public void Update () {
            var wasHeld = Hold;
            _value = Mathf.Clamp01(Axes.GetAxisRaw(axisID) * (positive ? 1f : -1f));
            _hold = _value >= ANALOG_TO_BOOL_THRESHOLD;
            _down = _hold & !wasHeld;
            _up = wasHeld & !_hold;
        }

        private bool _down;
        private bool _hold;
        private bool _up;
        private float _value;

        public override bool Down => _down;
        public override bool Hold => _hold;
        public override bool Up => _up;

        public override float Value => _value;
        public override string Name => Axes.NiceSubAxisName(this.axisID, this.positive);

        public override bool Equals (object obj) {
            if(obj is AxisInput other){
                return (this.axisID == other.axisID) && (this.positive == other.positive);
            }
            return false;
        }

        public override int GetHashCode () {
            return base.GetHashCode();
        }

        public override string ToString () {
            return $"({nameof(AxisInput)}) [{axisID.ToString()}, {positive.ToString()}]";
        }
    }
	
}

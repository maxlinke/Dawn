using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputSystem {

    [System.Serializable]
    public class JSONableInputCollection {
        public JSONableInput[] jsonedInputs;
    }

    // this is pretty horrible works just fine
    [System.Serializable]
    public class JSONableInput {

        public ID id;
        public KeyCodeInput keyCodeInput;
        public AxisInput axisInput;
        public string inputType;

        public JSONableInput (ID id, InputMethod input) {
            this.id = id;
            this.inputType = input.GetType().ToString();
            if(input is KeyCodeInput){
                this.keyCodeInput = (KeyCodeInput)input;
            }else if(input is AxisInput){
                this.axisInput = (AxisInput)input;
            }else{
                Debug.LogError($"Unknown {nameof(InputMethod)} \"{input.GetType().ToString()}\"!");
            }
        }

        public InputMethod GetInputMethod () {
            if(inputType.Equals(typeof(KeyCodeInput).ToString())){
                return keyCodeInput;
            }else if(inputType.Equals(typeof(AxisInput).ToString())){
                return axisInput;
            }else{
                Debug.LogError($"Unknown {nameof(InputMethod)} \"{inputType}\"!");
                return null;
            }
        }
    }

    void LoadDefaults () {

    }

    bool TryLoadingBinds () {
        return false;
    }

    public static void SaveBinds () {
        // var jsonableInputs = new List<JSONableInput>();
        // foreach(var key in instance.inputs.Keys){
        //     jsonableInputs.Add(new JSONableInput(key, instance.inputs[key]));
        // }
    }
	
}

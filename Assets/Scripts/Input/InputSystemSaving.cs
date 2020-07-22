using UnityEngine;

public partial class InputSystem {

    // registry (playerprefs) size limit is 1MB for strings, so either i'll have to save all keybinds per id OR use files for saving...
    // or rather, the registry doesn't LIKE entries over a certain size but it still saves them... 
    // i'll probably still do my own thing tho. for fun.
    // and as practice for savegames.

    [System.Serializable]
    private class JSONableInputCollection {
        public JSONableInput[] jsonableInputs;
    }

    // this is pretty horrible
    [System.Serializable]
    private class JSONableInput {

        public Bind id;
        public KeyCodeInput keyCodeInput;
        public AxisInput axisInput;
        public string inputType;

        public JSONableInput (Bind id, InputMethod input) {
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

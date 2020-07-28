using UnityEngine;

namespace CustomInputSystem.Inputs {

    [System.Serializable]
    public class SaveableInputMethod {

        public string typeName;
        public KeyCode keyCode;
        public Axis.ID axisID;
        public bool axisPositive;

        public SaveableInputMethod (InputMethod input) {
            this.typeName = input.GetType().ToString();
            if(input is KeyCodeInput keyCodeInput){
                this.keyCode = keyCodeInput.keyCode;
            }else if(input is AxisInput axisInput){
                this.axisID = axisInput.axis.id;
                this.axisPositive = axisInput.positive;
            }else{
                Debug.LogError($"Unknown {nameof(InputMethod)} \"{this.typeName}\"!");
            }
        }

        public bool TryRestoreInputMethod (out InputMethod outputInputMethod) {
            if(this.typeName.Equals(typeof(KeyCodeInput).ToString()) && System.Enum.IsDefined(typeof(KeyCode), this.keyCode)){
                outputInputMethod = new KeyCodeInput(this.keyCode);
                return true;
            }
            if(this.typeName.Equals(typeof(AxisInput).ToString()) && System.Enum.IsDefined(typeof(Axis.ID), this.axisID)){
                outputInputMethod = new AxisInput(this.axisID, this.axisPositive);
                return true;
            }
            outputInputMethod = null;
            return false;
        }

        public override string ToString () {
            return $"[{typeName}, {keyCode}, {axisID}, {axisPositive}]";
        }
        
    }

}
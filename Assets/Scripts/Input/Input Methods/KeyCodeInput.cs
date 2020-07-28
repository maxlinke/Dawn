using UnityEngine;

namespace CustomInputSystem.Inputs {

    public class KeyCodeInput : InputMethod {
            
        public readonly KeyCode keyCode;
        
        public KeyCodeInput (KeyCode keyCode) : base () {
            this.keyCode = keyCode;
        }

        public KeyCodeInput (KeyCodeUtils.XBoxKeyCode xBoxKeyCode) {
            this.keyCode = (KeyCode)xBoxKeyCode;
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

}
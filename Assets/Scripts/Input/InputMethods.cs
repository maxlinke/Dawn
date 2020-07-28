using UnityEngine;

namespace CustomInputSystem {

    public partial class InputSystem {

        public abstract class InputMethod {
            
            public abstract bool Down { get; }
            public abstract bool Hold { get; }
            public abstract bool Up { get; }
            
            public abstract float Value { get; }
            public abstract string Name { get; }
        }

        private class KeyCodeInput : InputMethod {
            
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

        private class AxisInput : InputMethod {

            public const float ANALOG_TO_BOOL_THRESHOLD = 0.5f;
            
            public readonly Axis axis;
            public readonly bool positive;

            private bool _down;
            private bool _hold;
            private bool _up;

            private float sign => (positive ? 1f : -1f);
            
            public AxisInput (Axis.ID axisID, bool positive) : base () {
                this.axis = Axis.GetAxis(axisID);
                this.positive = positive;
            }

            public void Update () {
                var wasHeld = Hold;
                _hold = Value >= ANALOG_TO_BOOL_THRESHOLD;
                _down = _hold & !wasHeld;
                _up = wasHeld & !_hold;
            }

            public override bool Down => _down;
            public override bool Hold => _hold;
            public override bool Up => _up;

            public override float Value => Mathf.Max(0f, this.axis.GetRaw() * this.sign);
            public override string Name => (positive ? this.axis.positiveName : this.axis.negativeName);

            public override bool Equals (object obj) {
                if(obj is AxisInput other){
                    return (this.axis == other.axis) && (this.positive == other.positive);
                }
                return false;
            }

            public override int GetHashCode () {
                return base.GetHashCode();
            }

            public override string ToString () {
                return $"({nameof(AxisInput)}) [{axis.ToString()}, {(positive ? "POS" : "NEG")}]";
            }
        }

        [System.Serializable]
        private class SaveableInputMethod {

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
}
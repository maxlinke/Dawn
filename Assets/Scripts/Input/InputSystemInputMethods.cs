using UnityEngine;

public partial class InputSystem {

    public abstract class InputMethod {
        
        public abstract bool Down { get; }
        public abstract bool Hold { get; }
        public abstract bool Up { get; }
        
        public abstract float Value { get; }
        public abstract string Name { get; }
    }

    [System.Serializable]
    private class KeyCodeInput : InputMethod {
        
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

    [System.Serializable]
    private class AxisInput : InputMethod {
        
        public Axis axisID;
        public bool positive;

        private bool _down;
        private bool _hold;
        private bool _up;
        private float _value;

        private float sign => (positive ? 1f : -1f);
        
        public AxisInput (Axis axisID, bool positive) : base () {
            this.axisID = axisID;
            this.positive = positive;
        }

        public void Update () {
            var wasHeld = Hold;
            _value = Mathf.Max(0f, Axes.GetAxisRaw(axisID) * this.sign);
            _hold = _value >= ANALOG_TO_BOOL_THRESHOLD;
            _down = _hold & !wasHeld;
            _up = wasHeld & !_hold;
        }

        public override bool Down => _down;
        public override bool Hold => _hold;
        public override bool Up => _up;

        public override float Value => _value;
        public override string Name => Axes.SubAxisName(this.axisID, this.positive);

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

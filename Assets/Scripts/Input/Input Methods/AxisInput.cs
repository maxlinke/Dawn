using UnityEngine;

namespace CustomInputSystem.Inputs {

    public class AxisInput : InputMethod {

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

        public override float Value => Mathf.Max(0f, this.axis.GetUnsmoothed() * this.sign);
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

}
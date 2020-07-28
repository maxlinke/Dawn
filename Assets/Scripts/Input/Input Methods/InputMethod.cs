using UnityEngine;

namespace CustomInputSystem.Inputs {

    public abstract class InputMethod {
        
        public abstract bool Down { get; }
        public abstract bool Hold { get; }
        public abstract bool Up { get; }
        
        public abstract float Value { get; }
        public abstract string Name { get; }
    }

}
using System.Collections.Generic;

public partial class InputSystem {

    public partial class Bind {

        public readonly ID id;
        public readonly string name;

        public bool immutable { get; private set; }     // TODO i'd like to get rid of it. but then i'd have to treat pause/unpause like a special snowflake in a FEW places. might be worth it.

        private List<InputMethod> inputs = new List<InputMethod>();

        public int inputCount => inputs.Count;

        private Bind (ID id, string name) {
            this.id = id;
            this.name = name;
            immutable = false;
        }

        public override string ToString () {
            var output = string.Empty;
            foreach(var input in inputs){
                output += $"{input}, ";
            }
            if(output.Length > 0){
                output.Remove(output.Length - 2);
            }else{
                output = "NO INPUTS!";
            }
            return $"[{(immutable ? "IMMUTABLE, " : "")}{output}]";
        }

        private bool ImmutableAbort () {
            if(immutable){
                DebugConsole.LogError($"Can't modify {nameof(Bind)} \"{id}\"!");
            }
            return immutable;
        }

        public IEnumerator<InputMethod> GetEnumerator () {
            foreach(var input in inputs){
                yield return input;
            }
        }

        public void AddInput (InputMethod newInput) {
            if(ImmutableAbort()){
                return;
            }
            inputs.Add(newInput);
            while(inputs.Count > MAX_INPUTS_PER_BIND){
                inputs.RemoveAt(0);
            }
            NotifyInputSystemIfAllowed();
        }

        public bool RemoveInput (InputMethod inputToRemove) {
            if(ImmutableAbort()){
                return false;
            }
            var output = inputs.Remove(inputToRemove);
            NotifyInputSystemIfAllowed();
            return output;
        }

        public void ClearInputs () {
            if(ImmutableAbort()){
                return;
            }
            inputs.Clear();
            NotifyInputSystemIfAllowed();
        }

        public bool UsesInput (InputMethod otherInput) {
            foreach(var input in inputs){
                if(input.Equals(otherInput)){
                    return true;
                }
            }
            return false;
        }

        public bool GetKeyDown () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Down;
            }
            return output;
        }

        public bool GetKey () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Hold;
            }
            return output;
        }

        public bool GetKeyUp () {
            var output = false;
            foreach(var input in inputs){
                output |= input.Up;
            }
            return output;
        }

        public float GetValue () {
            var output = 0f;
            foreach(var input in inputs){
                output += input.Value;
            }
            return output;
        }
    }
	
}

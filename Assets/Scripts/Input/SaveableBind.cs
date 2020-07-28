using CustomInputSystem.Inputs;

namespace CustomInputSystem {

    [System.Serializable]
    public class SaveableBind {

        public Bind.ID bindID;
        public SaveableInputMethod[] inputMethods;

        public SaveableBind (Bind bind) {
            this.bindID = bind.id;
            this.inputMethods = new SaveableInputMethod[bind.inputCount];
            int i=0;
            foreach(var input in bind){
                inputMethods[i] = new SaveableInputMethod(input);
                i++;
            }
        }

        public bool Apply (Bind target, out string errorMessage) {
            errorMessage = string.Empty;
            if(target.id != this.bindID){
                errorMessage = $"{nameof(Bind.ID)} mismatch! ({this.bindID}, {target.id})";
                return false;
            }
            foreach(var input in inputMethods){
                if(input.TryRestoreInputMethod(out var restoredInput)){
                    target.AddInput(restoredInput, false);
                }else{
                    errorMessage = $"Couldn't restore input method {input}";
                    return false;
                }
            }
            return true;
        }

    }

}
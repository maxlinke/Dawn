using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem.Inputs;

namespace CustomInputSystem {

    public class InputSystem : MonoBehaviour {

        [SerializeField] bool resetAxisConfigToDefault = false;
        [SerializeField] bool resetKeybindsToDefault = false;

        private static InputSystem instance;

        int lastUpdatedFrame = -1;
        int m_fixedUpdateCount = 0;
        List<AxisInput> axisInputs = new List<AxisInput>();

        public static int fixedUpdateCount => instance.m_fixedUpdateCount;

        void Awake () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(InputSystem)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            Test();
            Bind.Initialize();
            if(resetAxisConfigToDefault){
                Axis.Config.ResetToDefault();
            }
            if(resetKeybindsToDefault){
                Bind.ResetToDefault();
            }
            LogState();
        }

        public static void LogState () {
            Debug.Log($"Axis Log: \n{Axis.Config.GetLog()}");
            Debug.Log($"Keybind Log: \n{Bind.GetLog()}");
            Debug.Log($"Managed Axis Inputs: \n{GetAxisInputLog()}");
        }

        public static string GetAxisInputLog () {
            var output = string.Empty;
            foreach(var axisInput in instance.axisInputs){
                output += $"{axisInput}\n";
            }
            return output;
        }

        void Test () {

        }

        void OnDestroy () {
            if(instance == this){
                instance = null;
            }
        }

        void Update () {
            EnsureAllInputsUpToDate();
        }

        void FixedUpdate () {
            EnsureAllInputsUpToDate();
            m_fixedUpdateCount++;
        }

        void EnsureAllInputsUpToDate () {
            if(Time.frameCount == lastUpdatedFrame){
                return;
            }
            lastUpdatedFrame = Time.frameCount;
            foreach(var axisInput in axisInputs){
                axisInput.Update();
            }
        }

        public static bool IsAlreadyBound (InputMethod newInput, out Bind inputUsingBind) {
            foreach(var bind in Bind.Binds()){
                if(bind.UsesInput(newInput)){
                    inputUsingBind = bind;
                    return true;
                }
            }
            inputUsingBind = default;
            return false;
        }

        public static bool AnyInputHeld () {
            if(Input.anyKey){
                return true;
            }
            foreach(var axis in Axis.Axes()){
                if(Mathf.Abs(axis.GetUnsmoothed()) >= AxisInput.ANALOG_TO_BOOL_THRESHOLD){
                    return true;
                }
            }
            return false;
        }

        public static InputMethod CurrentlyHeldInput () {
            foreach(var kc in KeyCodeUtils.KeyCodes()){
                if(Input.GetKey(kc)){
                    return new KeyCodeInput(kc);
                }
            }
            foreach(var axis in Axis.Axes()){
                var rawVal = axis.GetUnsmoothed();
                if(Mathf.Abs(rawVal) >= AxisInput.ANALOG_TO_BOOL_THRESHOLD){
                    return new AxisInput(axis.id, rawVal > 0);
                }
            }
            return null;
        }

        public static void ValidateBinds () {
            var badBinds = new List<(Bind bind, InputMethod input)>();
            ScanForDoubleBinds(ref badBinds, true);
            if(badBinds.Count > 0){
                RemoveDoubleBinds(badBinds);
                ScanForDoubleBinds(ref badBinds, false);
                if(badBinds.Count > 0){
                    Debug.LogError("Found more invalid binds, something went very wrong!");
                }else{
                    Debug.Log("Removed bad binds");
                    Bind.SaveToDisk();
                }
            }
            if(instance != null){
                CollectAxisInputs();
            }
        }

        static void ScanForDoubleBinds (ref List<(Bind, InputMethod)> badBinds, bool logErrors) {
            var allInputs = new List<InputMethod>();
            foreach(var bind in Bind.Binds()){
                foreach(var input in bind){
                    if(allInputs.Contains(input)){
                        badBinds.Add((bind, input));
                        Debug.LogWarning($"{nameof(Bind)} \"{bind.name}\" uses {nameof(InputMethod)} \"{input.Name}\", which is already in use elsewhere!");
                    }else{
                        allInputs.Add(input);
                    }
                }
            }
        }

        static void RemoveDoubleBinds (List<(Bind bind, InputMethod input)> badBinds) {
            while(badBinds.Count > 0){
                int i = badBinds.Count-1;
                var badBind = badBinds[i];
                badBind.bind.RemoveInput(badBind.input, false);
                badBinds.RemoveAt(i);
            }
        }

        static void CollectAxisInputs () {
            instance.axisInputs.Clear();
            foreach(var bind in Bind.Binds()){
                foreach(var input in bind){
                    if(input is AxisInput axisInput){
                        if(instance.axisInputs.Contains(axisInput)){
                            Debug.LogWarning($"Duplicate usage of {nameof(AxisInput)} \"{input.Name}\"!");
                        }else{
                            instance.axisInputs.Add(axisInput);
                        }
                    }
                }
            }
        }
        
    }
}
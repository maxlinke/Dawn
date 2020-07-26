using System.Collections.Generic;
using UnityEngine;
using Persistence;

namespace CustomInputSystem {

    public partial class InputSystem {

        public partial class Bind {

            public const int MAX_INPUTS_PER_BIND = 3;

            public enum ID {          
                PAUSE_CANCEL_ETC,
                MOVE_FWD,
                MOVE_BWD,
                MOVE_LEFT,
                MOVE_RIGHT,
                LOOK_UP,
                LOOK_DOWN,
                LOOK_LEFT,
                LOOK_RIGHT,
                JUMP,
                TOGGLE_DEBUG_CONSOLE
            }
        
            public static Bind PAUSE_CANCEL_ETC { get; private set; }

            public static Bind MOVE_FWD { get; private set; }
            public static Bind MOVE_BWD { get; private set; }
            public static Bind MOVE_LEFT { get; private set; }
            public static Bind MOVE_RIGHT { get; private set; }
            public static Bind LOOK_UP { get; private set; }
            public static Bind LOOK_DOWN { get; private set; }
            public static Bind LOOK_LEFT { get; private set; }
            public static Bind LOOK_RIGHT { get; private set; }
            public static Bind JUMP { get; private set; }
            public static Bind TOGGLE_DEBUG_CONSOLE { get; private set; }
            // fire, alt fire?, interact
            // crouch, jump, run/walk
            // the scroll-weapon-switch stuff, the q-weapon-switch stuff ("previous"/"next" and "last used")
            // weapon category keys
            // kick?

            public static Bind GetBind (ID id) {
                switch(id){
                    case ID.PAUSE_CANCEL_ETC: return PAUSE_CANCEL_ETC;
                    case ID.MOVE_FWD: return MOVE_FWD;
                    case ID.MOVE_BWD: return MOVE_BWD;
                    case ID.MOVE_LEFT: return MOVE_LEFT;
                    case ID.MOVE_RIGHT: return MOVE_RIGHT;
                    case ID.LOOK_UP: return LOOK_UP;
                    case ID.LOOK_DOWN: return LOOK_DOWN;
                    case ID.LOOK_LEFT: return LOOK_LEFT;
                    case ID.LOOK_RIGHT: return LOOK_RIGHT;
                    case ID.JUMP: return JUMP;
                    case ID.TOGGLE_DEBUG_CONSOLE: return TOGGLE_DEBUG_CONSOLE;
                    default: 
                        Debug.LogError($"Unknown {nameof(ID)} \"{id}\"!");
                        return null;
                }
            }

            public static IEnumerable<ID> BindIDs () {
                foreach(var obj in System.Enum.GetValues(typeof(ID))){
                    yield return (ID)obj;
                }
            }

            public static IEnumerable<Bind> Binds () {          // could also be implemented as a manual iteration over all the things à la Axis.Axes() but eh
                foreach(var id in BindIDs()){                   // it would be faster but this shouldn't be called every frame anyways...
                    yield return GetBind(id);
                }
            }

            private static bool m_initialized = false;
            public static bool initialized => m_initialized;

            public static void Initialize () {
                if(initialized){
                    Debug.LogError("Duplicate init call, aborting!");
                    return;
                }
                InitializeBinds();
                if(!TryLoadingBindsFromDisk()){
                    ResetToDefault(false);
                }
                m_initialized = true;
                InputSystem.BindsChanged();
            }

            private static void InitializeBinds () {
                PAUSE_CANCEL_ETC = new Bind(ID.PAUSE_CANCEL_ETC, "Pause/Unpause/Cancel Rebind");
                PAUSE_CANCEL_ETC.AddInput(new KeyCodeInput(KeyCode.Escape), false);
                PAUSE_CANCEL_ETC.AddInput(new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.START), false);
                PAUSE_CANCEL_ETC.immutable = true;

                MOVE_FWD = new Bind(ID.MOVE_FWD, "Move Forward");
                MOVE_BWD = new Bind(ID.MOVE_BWD, "Move Back");
                MOVE_LEFT = new Bind(ID.MOVE_LEFT, "Move Left");
                MOVE_RIGHT = new Bind(ID.MOVE_RIGHT, "Move Right");
                LOOK_UP = new Bind(ID.LOOK_UP, "Look Up");
                LOOK_DOWN = new Bind(ID.LOOK_DOWN, "Look Down");
                LOOK_LEFT = new Bind(ID.LOOK_LEFT, "Look Left");
                LOOK_RIGHT = new Bind(ID.LOOK_RIGHT, "Look Right");
                JUMP = new Bind(ID.JUMP, "Jump");
                TOGGLE_DEBUG_CONSOLE = new Bind(ID.TOGGLE_DEBUG_CONSOLE, "Toggle Debug Console");
            }

            public static void ResetToDefault (bool notifyInputSystem = true) {
                LoadDefaultBinds();
                SaveToDisk();
                if(notifyInputSystem){
                    InputSystem.BindsChanged();
                }
            }

            private static void LoadDefaultBinds () {
                Debug.Log("Loading default keybinds");
                ClearAndAddWithoutInputSystemNotification(MOVE_FWD,   new KeyCodeInput(KeyCode.W), new AxisInput(Axis.ID.LEFT_STICK_Y, true));
                ClearAndAddWithoutInputSystemNotification(MOVE_BWD,   new KeyCodeInput(KeyCode.S), new AxisInput(Axis.ID.LEFT_STICK_Y, false));
                ClearAndAddWithoutInputSystemNotification(MOVE_LEFT,  new KeyCodeInput(KeyCode.A), new AxisInput(Axis.ID.LEFT_STICK_X, false));
                ClearAndAddWithoutInputSystemNotification(MOVE_RIGHT, new KeyCodeInput(KeyCode.D), new AxisInput(Axis.ID.LEFT_STICK_X, true));
                ClearAndAddWithoutInputSystemNotification(LOOK_UP,    new AxisInput(Axis.ID.MOUSE_Y, true),  new AxisInput(Axis.ID.RIGHT_STICK_Y, true));
                ClearAndAddWithoutInputSystemNotification(LOOK_DOWN,  new AxisInput(Axis.ID.MOUSE_Y, false), new AxisInput(Axis.ID.RIGHT_STICK_Y, false));
                ClearAndAddWithoutInputSystemNotification(LOOK_LEFT,  new AxisInput(Axis.ID.MOUSE_X, false), new AxisInput(Axis.ID.RIGHT_STICK_X, false));
                ClearAndAddWithoutInputSystemNotification(LOOK_RIGHT, new AxisInput(Axis.ID.MOUSE_X, true),  new AxisInput(Axis.ID.RIGHT_STICK_X, true));
                ClearAndAddWithoutInputSystemNotification(JUMP, new KeyCodeInput(KeyCode.Space), new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.A));
                ClearAndAddWithoutInputSystemNotification(TOGGLE_DEBUG_CONSOLE, new KeyCodeInput(KeyCode.F1));

                void ClearAndAddWithoutInputSystemNotification (Bind bind, params InputMethod[] inputsToAdd) {
                    bind.ClearInputs(false);
                    foreach(var inputToAdd in inputsToAdd){
                        bind.AddInput(inputToAdd, false);
                    }
                }
            }

            private static bool TryLoadingBindsFromDisk () {
                if(!FileHelper.ConfigFileExists(FileNames.keybinds)){
                    Debug.Log("No keybind file found");
                    return false;
                }
                if(FileHelper.TryLoadConfigFile(FileNames.keybinds, out var json)){
                    if(json == null || json.Length == 0){
                        Debug.LogError("Keybinds file is empty!");
                        return false;
                    }
                    try{
                        var allIDs = new List<Bind.ID>();
                        foreach(var bind in Binds()){
                            if(!bind.immutable){
                                allIDs.Add(bind.id);
                            }
                        }
                        var col = JsonUtility.FromJson<SaveableBindCollection>(json);
                        foreach(var sb in col.binds){
                            if(!System.Enum.IsDefined(typeof(Bind.ID), sb.bindID)){
                                throw new System.ArgumentException($"Undefined {nameof(Bind.ID)} \"{sb.bindID}\"");
                            }
                            if(!sb.Apply(GetBind(sb.bindID), out var errorMessage)){
                                throw new System.ArgumentException(errorMessage);
                            }
                            if(!allIDs.Remove(sb.bindID)){
                                throw new System.ArgumentException($"Multiple occurences of {nameof(Bind.ID)} \"{sb.bindID}\"");
                            }
                        }
                        if(allIDs.Count > 0){
                            Debug.LogError("Not all binds could be loaded");
                            return false;
                        }else{
                            Debug.Log("Successfully loaded keybinds");
                            return true;
                        }
                    }catch(System.Exception e){
                        Debug.LogError($"Issue loading keybinds \n{e.Message}");
                    }
                }
                return false;
            }

            public static void SaveToDisk () {
                var bindsList = new List<SaveableBind>();
                foreach(var bindID in BindIDs()){
                    var bind = GetBind(bindID);
                    if(bind.immutable){
                        continue;
                    }
                    bindsList.Add(new SaveableBind(bind));
                }
                var saveableObject = new SaveableBindCollection();
                saveableObject.binds = bindsList.ToArray();
                var json = JsonUtility.ToJson(saveableObject, true);
                FileHelper.SaveConfigFile(FileNames.keybinds, json);
                Debug.Log("Saving keybinds to disk");
            }

            public static string GetLog () {
                var output = string.Empty;
                foreach(var bind in Binds()){
                    output += $"{bind.id}: {bind}\n";
                }
                return output;
            }

        }

        [System.Serializable]
        private class SaveableBindCollection {

            public SaveableBind[] binds;

        }

        [System.Serializable]
        private class SaveableBind {

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
}
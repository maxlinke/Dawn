using System.Collections.Generic;
using UnityEngine;
using Persistence;

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
            LOOK_RIGHT
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
        // fire, alt fire?, interact
        // crouch, jump, run/walk
        // the scroll-weapon-switch stuff, the q-weapon-switch stuff
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
                default: 
                    DebugConsole.LogError($"Unknown {nameof(ID)} \"{id}\"!");
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

        private static bool notifyInputSystemOfUpdates = true;

        public static void Initialize () {
            if(initialized){
                DebugConsole.LogError("Duplicate init call, aborting!");
                return;
            }
            notifyInputSystemOfUpdates = false;
            InitializeBinds();
            if(!TryLoadingBindsFromDisk()){
                ResetToDefault();
            }
            notifyInputSystemOfUpdates = true;
            m_initialized = true;
            InputSystem.BindsChanged();
        }

        private static void NotifyInputSystemIfAllowed () {
            if(notifyInputSystemOfUpdates){
                InputSystem.BindsChanged();
            }
        }

        private static void InitializeBinds () {
            PAUSE_CANCEL_ETC = new Bind(ID.PAUSE_CANCEL_ETC, "Pause/Unpause/Cancel Rebind");
            PAUSE_CANCEL_ETC.AddInput(new KeyCodeInput(KeyCode.Escape));
            PAUSE_CANCEL_ETC.AddInput(new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.START));
            PAUSE_CANCEL_ETC.immutable = true;

            MOVE_FWD = new Bind(ID.MOVE_FWD, "Move Forward");
            MOVE_BWD = new Bind(ID.MOVE_BWD, "Move Back");
            MOVE_LEFT = new Bind(ID.MOVE_LEFT, "Move Left");
            MOVE_RIGHT = new Bind(ID.MOVE_RIGHT, "Move Right");
            LOOK_UP = new Bind(ID.LOOK_UP, "Look Up");
            LOOK_DOWN = new Bind(ID.LOOK_DOWN, "Look Down");
            LOOK_LEFT = new Bind(ID.LOOK_LEFT, "Look Left");
            LOOK_RIGHT = new Bind(ID.LOOK_RIGHT, "Look Right");
        }

        public static void ResetToDefault () {
            var notifCache = notifyInputSystemOfUpdates;
            notifyInputSystemOfUpdates = false;
            LoadDefaultBinds();
            SaveToDisk();
            notifyInputSystemOfUpdates = notifCache;
            NotifyInputSystemIfAllowed();
        }

        private static void LoadDefaultBinds () {
            DebugConsole.Log("Loading default keybinds");
            ClearAndAdd(MOVE_FWD,   new KeyCodeInput(KeyCode.W), new AxisInput(Axis.ID.LEFT_STICK_Y, true));
            ClearAndAdd(MOVE_BWD,   new KeyCodeInput(KeyCode.S), new AxisInput(Axis.ID.LEFT_STICK_Y, false));
            ClearAndAdd(MOVE_LEFT,  new KeyCodeInput(KeyCode.A), new AxisInput(Axis.ID.LEFT_STICK_X, false));
            ClearAndAdd(MOVE_RIGHT, new KeyCodeInput(KeyCode.D), new AxisInput(Axis.ID.LEFT_STICK_X, true));
            ClearAndAdd(LOOK_UP,    new AxisInput(Axis.ID.MOUSE_Y, true),  new AxisInput(Axis.ID.RIGHT_STICK_Y, true));
            ClearAndAdd(LOOK_DOWN,  new AxisInput(Axis.ID.MOUSE_Y, false), new AxisInput(Axis.ID.RIGHT_STICK_Y, false));
            ClearAndAdd(LOOK_LEFT,  new AxisInput(Axis.ID.MOUSE_X, false), new AxisInput(Axis.ID.RIGHT_STICK_X, false));
            ClearAndAdd(LOOK_RIGHT, new AxisInput(Axis.ID.MOUSE_X, true),  new AxisInput(Axis.ID.RIGHT_STICK_X, true));

            void ClearAndAdd (Bind bind, params InputMethod[] inputsToAdd) {
                bind.ClearInputs();
                foreach(var inputToAdd in inputsToAdd){
                    bind.AddInput(inputToAdd);
                }
            }
        }

        private static bool TryLoadingBindsFromDisk () {
            if(!FileHelper.ConfigFileExists(FileNames.keybinds)){
                DebugConsole.Log("No keybind file found");
                return false;
            }
            if(FileHelper.TryLoadConfigFile(FileNames.keybinds, out var json)){
                try{
                    var col = JsonUtility.FromJson<SaveableBindCollection>(json);
                    foreach(var sb in col.binds){
                        if(!sb.Apply(GetBind(sb.bindID))){
                            return false;
                        }
                    }
                    DebugConsole.Log("Successfully loaded keybinds");
                    return true;
                }catch(System.Exception e){
                    DebugConsole.LogError($"Issue loading keybinds \n{e.Message}");
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
            DebugConsole.Log("Saving keybinds to disk");
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

        public bool Apply (Bind target) {
            if(target.id != this.bindID){
                DebugConsole.LogError($"{nameof(Bind.ID)} mismatch! ({this.bindID}, {target.id})");
                return false;
            }
            foreach(var input in inputMethods){
                if(input.TryRestoreInputMethod(out var restoredInput)){
                    target.AddInput(restoredInput);
                }else{
                    DebugConsole.LogError("Couldn't restore input method");
                    return false;
                }
            }
            return true;
        }

    }

}

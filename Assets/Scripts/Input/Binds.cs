using System.Collections.Generic;
using UnityEngine;
using Persistence;
using CustomInputSystem.Inputs;

namespace CustomInputSystem {

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
            CROUCH_HOLD,
            CROUCH_TOGGLE,
            WALK_OR_RUN,

            INTERACT,

            TOGGLE_DEBUG_LOG,
            TOGGLE_FRAMERATE_DISPLAY,
            TOGGLE_DEBUG_CONSOLE,
            TOGGLE_PLAYERCONTROLLER_DEBUG
        }
    
        public static readonly Bind PAUSE_CANCEL_ETC = CreateImmutableBind(ID.PAUSE_CANCEL_ETC, "Pause/Unpause/Cancel Rebind", new KeyCodeInput(KeyCode.Escape), new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.START));

        public static readonly Bind MOVE_FWD =   new Bind(ID.MOVE_FWD, "Move Forward");
        public static readonly Bind MOVE_BWD =   new Bind(ID.MOVE_BWD, "Move Back");
        public static readonly Bind MOVE_LEFT =  new Bind(ID.MOVE_LEFT, "Move Left");
        public static readonly Bind MOVE_RIGHT = new Bind(ID.MOVE_RIGHT, "Move Right");
        public static readonly Bind LOOK_UP =    new Bind(ID.LOOK_UP, "Look Up");
        public static readonly Bind LOOK_DOWN =  new Bind(ID.LOOK_DOWN, "Look Down");
        public static readonly Bind LOOK_LEFT =  new Bind(ID.LOOK_LEFT, "Look Left");
        public static readonly Bind LOOK_RIGHT = new Bind(ID.LOOK_RIGHT, "Look Right");
        
        public static readonly Bind JUMP =          new Bind(ID.JUMP, "Jump");
        public static readonly Bind CROUCH_HOLD =   new Bind(ID.CROUCH_HOLD, "Crouch (Hold)");
        public static readonly Bind CROUCH_TOGGLE = new Bind(ID.CROUCH_TOGGLE, "Crouch (Toggle)");
        public static readonly Bind WALK_OR_RUN   = new Bind(ID.WALK_OR_RUN, "Walk/Run");

        public static readonly Bind INTERACT = new Bind(ID.INTERACT, "Interact");

        public static readonly Bind TOGGLE_DEBUG_LOG = new Bind(ID.TOGGLE_DEBUG_LOG, "Toggle Debug Log");
        public static readonly Bind TOGGLE_FRAMERATE_DISPLAY = new Bind(ID.TOGGLE_FRAMERATE_DISPLAY, "Toggle Framerate Display");
        public static readonly Bind TOGGLE_DEBUG_CONSOLE = new Bind(ID.TOGGLE_DEBUG_CONSOLE, "Toggle Debug Console");
        public static readonly Bind TOGGLE_PLAYERCONTROLLER_DEBUG = new Bind(ID.TOGGLE_PLAYERCONTROLLER_DEBUG, "Toggle PlayerController Debug UI");
        // fire, alt fire?, interact
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
                case ID.CROUCH_HOLD: return CROUCH_HOLD;
                case ID.CROUCH_TOGGLE: return CROUCH_TOGGLE;
                case ID.WALK_OR_RUN: return WALK_OR_RUN;

                case ID.INTERACT: return INTERACT;

                case ID.TOGGLE_DEBUG_LOG: return TOGGLE_DEBUG_LOG;
                case ID.TOGGLE_FRAMERATE_DISPLAY: return TOGGLE_FRAMERATE_DISPLAY;
                case ID.TOGGLE_DEBUG_CONSOLE: return TOGGLE_DEBUG_CONSOLE;
                case ID.TOGGLE_PLAYERCONTROLLER_DEBUG: return TOGGLE_PLAYERCONTROLLER_DEBUG;
                
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
            if(!TryLoadingBindsFromDisk()){
                ResetToDefault(false);
            }
            m_initialized = true;
            InputSystem.ValidateBinds();
        }

        public static void ResetToDefault (bool notifyInputSystem = true) {
            LoadDefaultBinds();
            SaveToDisk();
            if(notifyInputSystem){
                InputSystem.ValidateBinds();
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

            ClearAndAddWithoutInputSystemNotification(JUMP,          new KeyCodeInput(KeyCode.Space),     new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.A), new AxisInput(Axis.ID.MOUSE_SCROLL, false));
            ClearAndAddWithoutInputSystemNotification(CROUCH_HOLD,   new KeyCodeInput(KeyCode.LeftControl));
            ClearAndAddWithoutInputSystemNotification(CROUCH_TOGGLE, new KeyCodeInput(KeyCode.C),         new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.LS));
            ClearAndAddWithoutInputSystemNotification(WALK_OR_RUN,   new KeyCodeInput(KeyCode.LeftShift), new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.X));

            ClearAndAddWithoutInputSystemNotification(INTERACT, new KeyCodeInput(KeyCode.E), new KeyCodeInput(KeyCodeUtils.XBoxKeyCode.B));

            ClearAndAddWithoutInputSystemNotification(TOGGLE_DEBUG_LOG, new KeyCodeInput(KeyCode.F1));
            ClearAndAddWithoutInputSystemNotification(TOGGLE_FRAMERATE_DISPLAY, new KeyCodeInput(KeyCode.F2));
            ClearAndAddWithoutInputSystemNotification(TOGGLE_DEBUG_CONSOLE, new KeyCodeInput(KeyCode.F3));
            ClearAndAddWithoutInputSystemNotification(TOGGLE_PLAYERCONTROLLER_DEBUG, new KeyCodeInput(KeyCode.F4));

            void ClearAndAddWithoutInputSystemNotification (Bind bind, params InputMethod[] inputsToAdd) {
                bind.ClearInputs(false);
                foreach(var inputToAdd in inputsToAdd){
                    bind.AddInput(inputToAdd, false);
                }
            }
        }

        private static bool TryLoadingBindsFromDisk () {
            if(!FileHelper.ConfigFileExists(FileNames.keybinds, out var keybindPath)){
                Debug.Log($"No keybind file found at\n{keybindPath}");
                return false;
            }
            if(FileHelper.TryLoadConfigFile(FileNames.keybinds, out var json)){
                if(json == null || json.Length == 0){
                    Debug.LogWarning($"Empty keybinds file at\n{keybindPath}");
                    return false;
                }
                try{
                    var allIDs = new List<Bind.ID>();
                    foreach(var bind in Binds()){
                        if(!bind.immutable){
                            allIDs.Add(bind.id);
                        }
                    }
                    var binds = JsonHelper.GetJsonArray<SaveableBind>(json);
                    foreach(var sb in binds){
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
                        Debug.LogWarning($"Not all binds could be loaded from\n{keybindPath}");
                        return false;
                    }else{
                        Debug.Log($"Successfully loaded keybinds from\n{keybindPath}");
                        return true;
                    }
                }catch(System.Exception e){
                    Debug.LogWarning($"Issue loading keybinds \n{e.Message}");
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
            var json = JsonHelper.ToJsonArray<SaveableBind>(bindsList.ToArray(), true);
            FileHelper.SaveConfigFile(FileNames.keybinds, json, out var path);
            Debug.Log($"Saving keybinds to disk\n{path}");
        }

        public static string GetLog () {
            var output = string.Empty;
            foreach(var bind in Binds()){
                output += $"{bind.id}: {bind}\n";
            }
            return output;
        }

        public static Vector2 GetViewInput () {
            var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
            var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
            return new Vector2(dx, dy);
        }

        public static Vector3 GetHorizontalMoveInput () {
            float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
            float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
            var output = new Vector3(strafe, 0, move);
            if(output.sqrMagnitude > 1){
                return output.normalized;
            }
            return output;
        }

        public static Vector3 GetVerticalMoveInput () {
            var up = Bind.JUMP.GetValue();
            var down = Mathf.Max(Bind.CROUCH_HOLD.GetValue(), Bind.CROUCH_TOGGLE.GetValue());
            var output = new Vector3(0f, up - down, 0f);
            if(output.sqrMagnitude > 1){
                return output.normalized;
            }
            return output;
        }

    }

    
}
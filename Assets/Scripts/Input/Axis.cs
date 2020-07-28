using System.Collections.Generic;
using UnityEngine;

namespace CustomInputSystem {

    public partial class Axis {

        public enum ID {
            NONE,
            MOUSE_X,
            MOUSE_Y,
            MOUSE_SCROLL,
            LEFT_STICK_X,
            LEFT_STICK_Y,
            RIGHT_STICK_X,
            RIGHT_STICK_Y,
            DPAD_X,
            DPAD_Y,
            LEFT_TRIGGER,
            RIGHT_TRIGGER
        }

        public static readonly Axis NONE =           new Axis("None",    ID.NONE,    Config.ZERO,  "None");
        public static readonly Axis MOUSE_X =        new Axis("Mouse X", ID.MOUSE_X, Config.MOUSE, "Mouse X");
        public static readonly Axis MOUSE_Y =        new Axis("Mouse Y", ID.MOUSE_Y, Config.MOUSE, "Mouse Y");
        public static readonly Axis MOUSE_SCROLL =   new Axis("Mouse ScrollWheel", ID.MOUSE_SCROLL, Config.NEUTRAL, "Mouse Scroll");
        public static readonly Axis LEFT_STICK_X =   new Axis("Axis1",  ID.LEFT_STICK_X,  Config.LEFT_STICK,  "Left Stick X");
        public static readonly Axis LEFT_STICK_Y =   new Axis("Axis2",  ID.LEFT_STICK_Y,  Config.LEFT_STICK,  "Left Stick Y");
        public static readonly Axis RIGHT_STICK_X =  new Axis("Axis4",  ID.RIGHT_STICK_X, Config.RIGHT_STICK, "Right Stick X");
        public static readonly Axis RIGHT_STICK_Y =  new Axis("Axis5",  ID.RIGHT_STICK_Y, Config.RIGHT_STICK, "Right Stick Y");
        public static readonly Axis DPAD_X =         new Axis("Axis6",  ID.DPAD_X,        Config.NEUTRAL,     "DPad X",        "DPad Right",    "DPad Left");
        public static readonly Axis DPAD_Y =         new Axis("Axis7",  ID.DPAD_Y,        Config.NEUTRAL,     "DPad Y",        "DPad Up",       "DPad Down");
        public static readonly Axis LEFT_TRIGGER =   new Axis("Axis9",  ID.LEFT_TRIGGER,  Config.TRIGGERS,    "Left Trigger",  "Left Trigger",  "Left Trigger NEGATIVE??!");
        public static readonly Axis RIGHT_TRIGGER =  new Axis("Axis10", ID.RIGHT_TRIGGER, Config.TRIGGERS,    "Right Trigger", "Right Trigger", "Right Trigger NEGATIVE??!");

        public static IEnumerable<Axis> Axes () {
            yield return NONE;
            yield return MOUSE_X;
            yield return MOUSE_Y;
            yield return MOUSE_SCROLL;
            yield return LEFT_STICK_X;
            yield return LEFT_STICK_Y;
            yield return RIGHT_STICK_X;
            yield return RIGHT_STICK_Y;
            yield return DPAD_X;
            yield return DPAD_Y;
            yield return LEFT_TRIGGER;
            yield return RIGHT_TRIGGER;
        }

        public static IEnumerable<ID> AxisIDs () {
            foreach(var obj in System.Enum.GetValues(typeof(ID))){
                yield return (ID)obj;
            }
        }

        public static Axis GetAxis (ID id) {
            switch(id){
                case ID.NONE: return NONE;
                case ID.MOUSE_X: return MOUSE_X;
                case ID.MOUSE_Y: return MOUSE_Y;
                case ID.MOUSE_SCROLL: return MOUSE_SCROLL;
                case ID.LEFT_STICK_X: return LEFT_STICK_X;
                case ID.LEFT_STICK_Y: return LEFT_STICK_Y;
                case ID.RIGHT_STICK_X: return RIGHT_STICK_X;
                case ID.RIGHT_STICK_Y: return RIGHT_STICK_Y;
                case ID.DPAD_X: return DPAD_X;
                case ID.DPAD_Y: return DPAD_Y;
                case ID.LEFT_TRIGGER: return LEFT_TRIGGER;
                case ID.RIGHT_TRIGGER: return RIGHT_TRIGGER;
                default:
                    Debug.LogError($"Unknown {nameof(ID)} \"{id}\"!");
                    return null;
            }
        }

        public static Axis.ID Parse (string value, bool ignoreCase = true) {
            return (ID)(System.Enum.Parse(typeof(Axis.ID), value, ignoreCase));
        }

        public static bool TryParse (string value, out Axis.ID result, bool ignoreCase = true) {
            return System.Enum.TryParse<Axis.ID>(value, ignoreCase, out result);
        }

        private readonly string unityInputIdentifier;
        private readonly Config config;

        public readonly ID id;
        public readonly string name;
        public readonly string positiveName;
        public readonly string negativeName;

        private Axis (string unityInputIdentifier, ID id, Config config, string name, string positiveName, string negativeName) {
            this.unityInputIdentifier = unityInputIdentifier;
            this.id = id;
            this.config = config;
            this.name = name;
            this.positiveName = positiveName;
            this.negativeName = negativeName;
        }

        private Axis (string unityInputIdentifier, ID id, Config config, string name) {
            this.unityInputIdentifier = unityInputIdentifier;
            this.id = id;
            this.config = config;
            this.name = name;
            this.positiveName = $"{name}+";
            this.negativeName = $"{name}-";
        }

        public float GetRaw () {
            return config.ApplyConfig(Input.GetAxisRaw(unityInputIdentifier));
        }

        public float GetSmoothed () {
            return config.ApplyConfig(Input.GetAxis(unityInputIdentifier));
        }

        public override string ToString () {
            return id.ToString();
        }

    }
}
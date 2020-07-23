using System.Collections.Generic;
using UnityEngine;

public static partial class Axes {
                                                                // referring to unity input manager settings: 
    private const string mouseX = "Mouse X";                    // < sensitivity kept at 0.1
    private const string mouseY = "Mouse Y";                    // < sensitivity kept at 0.1
    private const string mouseScroll = "Mouse ScrollWheel";     // < sensitivity upped to 1

    private const string leftStickX = "Axis1";  // < (and down) 0 deadzone, 1 sensitivity
    private const string leftStickY = "Axis2";
    // axis 3 would be the triggers as one axis, but that makes treating the triggers individually impossible
    private const string rightStickX = "Axis4";
    private const string rightStickY = "Axis5";
    private const string dpadX = "Axis6";
    private const string dpadY = "Axis7";
    // also there's no 8th axis on an xbox controller...
    private const string leftTrigger = "Axis9";
    private const string rightTrigger = "Axis10";

    public const float MIN_DEADZONE = 0f;
    public const float MAX_DEADZONE = 0.999f;           // because 1 would result in NaN problems
    public const float MIN_SENSITIVITY = 0f;
    public const float MAX_SENSITIVITY = 20f;

    // TODO see if i can't make the ids classes/structs again.
    // but try initializing them in the static constructor of this class
    // by then MAYBE the axisconfigs should exist?
    // if not, find a way to ensure their existence
    // i like enums, but i don't like this switcheroo

    // and i kinda want to allow deadzones up to and including 1 but avoid the NaNs without if

    public static IEnumerable<Axis> AxisIDs () {
        foreach(var idObj in System.Enum.GetValues(typeof(Axis))){
            yield return (Axis)idObj;
        }
    }

    private static (string name, AxisConfig config) GetAxisData (Axis id) {
        switch(id){
            case Axis.MOUSE_X:       return (mouseX,       AxisConfig.MOUSE);
            case Axis.MOUSE_Y:       return (mouseY,       AxisConfig.MOUSE);
            case Axis.MOUSE_SCROLL:  return (mouseScroll,  AxisConfig.NEUTRAL);
            case Axis.LEFT_STICK_X:  return (leftStickX,   AxisConfig.LEFT_STICK);
            case Axis.LEFT_STICK_Y:  return (leftStickY,   AxisConfig.LEFT_STICK);
            case Axis.RIGHT_STICK_X: return (rightStickX,  AxisConfig.RIGHT_STICK);
            case Axis.RIGHT_STICK_Y: return (rightStickY,  AxisConfig.RIGHT_STICK);
            case Axis.DPAD_X:        return (dpadX,        AxisConfig.NEUTRAL);
            case Axis.DPAD_Y:        return (dpadY,        AxisConfig.NEUTRAL);
            case Axis.LEFT_TRIGGER:  return (leftTrigger,  AxisConfig.TRIGGERS);
            case Axis.RIGHT_TRIGGER: return (rightTrigger, AxisConfig.TRIGGERS);
            default:
                Debug.LogError($"Unknown or unsupported {nameof(Axis)} for axis \"{id}\"!");
                return (string.Empty, default);
        }
    }

    public static float GetAxisRaw (Axis id) {
        var axisData = GetAxisData(id);
        return axisData.config.ApplyConfig(Input.GetAxisRaw(axisData.name));
    }

    public static float GetAxisSmoothed (Axis id) {
        var axisData = GetAxisData(id);
        return axisData.config.ApplyConfig(Input.GetAxis(axisData.name));
    }

}

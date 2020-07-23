using System.Collections.Generic;
using UnityEngine;

public static partial class Axes {
                                                                // referring to unity input manager settings: 
    private const string mouseX = "Mouse X";                    // < sensitivity kept at 0.1
    private const string mouseY = "Mouse Y";                    // < sensitivity kept at 0.1
    private const string mouseScroll = "Mouse ScrollWheel";     // < sensitivity upped to 1

    private const string leftStickX = "Left Stick Horizontal";  // < (and down) 0 deadzone, 1 sensitivity
    private const string leftStickY = "Left Stick Vertical";
    private const string rightStickX = "Right Stick Horizontal";
    private const string rightStickY = "Right Stick Vertical";
    private const string dpadX = "DPad Horizontal";
    private const string dpadY = "DPad Vertical";
    private const string leftTrigger = "Left Trigger";
    private const string rightTrigger = "Right Trigger";

    public const float MIN_DEADZONE = 0f;
    public const float MAX_DEADZONE = 0.999f;           // because 1 would result in NaN problems
    public const float MIN_SENSITIVITY = 1f;
    public const float MAX_SENSITIVITY = 2f;

    // TODO see if i can't make the ids classes/structs again.
    // but try initializing them in the static constructor of this class
    // by then MAYBE the axisconfigs should exist?
    // if not, find a way to ensure their existence
    // i like enums, but i don't like this switcheroo

    // also maybe rename the axes in the input manager to simply reflect their number?

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

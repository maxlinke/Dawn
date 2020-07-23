using UnityEngine;

public static partial class Axes {

    // related to the names in KeyCodeUtils...

    public static string AxisName (Axis id) {
        switch(id){
            case Axis.MOUSE_X:       return $"Mouse X";
            case Axis.MOUSE_Y:       return $"Mouse Y";
            case Axis.MOUSE_SCROLL:  return $"Mouse Scroll";
            case Axis.LEFT_STICK_X:  return $"Left Stick X";
            case Axis.LEFT_STICK_Y:  return $"Left Stick Y";
            case Axis.RIGHT_STICK_X: return $"Right Stick X";
            case Axis.RIGHT_STICK_Y: return $"Right Stick Y";
            case Axis.DPAD_X:        return $"DPad X";
            case Axis.DPAD_Y:        return $"DPad Y";
            case Axis.LEFT_TRIGGER:  return "Left Trigger";
            case Axis.RIGHT_TRIGGER: return "Right Trigger";
            default:
                Debug.LogError($"Unknown or unsupported {nameof(Axis)} for axis \"{id}\"!");
                return "Unknown Axis";
        }
    }
    
    public static string SubAxisName (Axis id, bool positive) {
        switch(id){
            case Axis.DPAD_X:        return $"DPad {(positive ? "Right" : "Left")}";
            case Axis.DPAD_Y:        return $"DPad {(positive ? "Up" : "Down")}";
            case Axis.LEFT_TRIGGER:  return "Left Trigger";
            case Axis.RIGHT_TRIGGER: return "Right Trigger";
            default:
                return $"{AxisName(id)}{DefaultSuffix()}";
        }

        string DefaultSuffix() {
            return (positive ? "+" : "-");
        }
    }
	
}

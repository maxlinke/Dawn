using UnityEngine;
using UnityEditor;

namespace PlayerController {

    [CustomEditor(typeof(Properties))]
    public class PropertiesEditor : GenericEditor {

        SerializedProperty boostProp;
        SerializedProperty brakeProp;
        SerializedProperty stickProp;

        bool boostOn => boostProp.floatValue != 1f;
        bool brakeOn => brakeProp.floatValue != 1f;

        protected void OnEnable () {
            boostProp = serializedObject.FindProperty("boostMultiplier");
            brakeProp = serializedObject.FindProperty("landingSpeedMultiplier");
            stickProp = serializedObject.FindProperty("forcedGroundSticking");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "groundStickiness":
                    return DrawEnabledIf(stickProp.boolValue);
                case "boostDirection":
                    return DrawEnabledIf(boostOn);
                case "enableOverBoosting":
                    return DrawEnabledIf(boostOn);
                case "enableBunnyHopping":
                    return DrawEnabledIf(brakeOn);
                default: 
                    return false;
            }

            bool DrawEnabledIf (bool condition) {
                var guiOn = GUI.enabled;
                GUI.enabled &= condition;
                EditorTools.DrawIndented(property);
                GUI.enabled = guiOn;
                return true;
            }
        }
        
    }

}
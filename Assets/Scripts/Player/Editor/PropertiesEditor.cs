using UnityEngine;
using UnityEditor;

namespace PlayerController {

    [CustomEditor(typeof(Properties))]
    public class PropertiesEditor : GenericEditor {

        SerializedProperty jumpProp;
        SerializedProperty boostProp;
        SerializedProperty brakeProp;
        SerializedProperty stickProp;

        bool setJump => (jumpProp.enumValueIndex == (int)(Properties.JumpVelocityMode.SetLocalVelocity));
        bool boostOn => boostProp.floatValue != 1f;
        bool brakeOn => brakeProp.floatValue != 1f;
        bool stickOn => stickProp.intValue != 0;

        protected void OnEnable () {
            jumpProp = serializedObject.FindProperty("velocityMode");
            boostProp = serializedObject.FindProperty("boostMultiplier");
            brakeProp = serializedObject.FindProperty("landingSpeedMultiplier");
            stickProp = serializedObject.FindProperty("groundStick");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "minGravityTurn":
                    EditorTools.DrawIndented(property);
                    return true;
                case "minJumpVelocity":
                    return DrawEnabledIf(setJump);
                case "limitDescentJumpHeight":
                    return DrawEnabledIf(setJump);
                case "groundStickiness":
                    return DrawEnabledIf(stickOn);
                case "groundStickInterval":
                    return DrawEnabledIf(stickOn);
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
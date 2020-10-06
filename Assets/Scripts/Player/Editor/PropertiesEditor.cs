using UnityEngine;
using UnityEditor;

namespace PlayerController {

    [CustomEditor(typeof(Properties))]
    public class PropertiesEditor : GenericEditor {

        SerializedProperty boostProp;

        bool boostOn => (boostProp.enumValueIndex != (int)(Properties.JumpSpeedBoostMode.Off));

        protected override void OnEnable () {
            base.OnEnable();
            boostProp = serializedObject.FindProperty("jumpSpeedBoost");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "jumpSpeedBoostMultiplier":
                    return DrawDependentOnBoost();
                case "enableOverBoosting":
                    return DrawDependentOnBoost();
                case "enableBunnyHopping":
                    return DrawDependentOnBoost();
                default: 
                    return false;
            }

            bool DrawDependentOnBoost () {
                var guiOn = GUI.enabled;
                GUI.enabled &= boostOn;
                EditorTools.DrawIndented(property);
                GUI.enabled = guiOn;
                return true;
            }
        }
        
    }

}
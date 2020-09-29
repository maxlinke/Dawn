using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(PlaneGenerator))]
    public class PlaneGeneratorEditor : GeometryGeneratorEditor { 

        SerializedProperty triModeProp;

        protected override void OnEnable () {
            base.OnEnable();
            triModeProp = serializedObject.FindProperty("triMode");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            if(property.name.Equals("triSeed")){
                if(triModeProp.enumValueIndex == (int)(PlaneGenerator.TriMode.Random)){
                    EditorTools.DrawIndented(() => EditorGUILayout.PropertyField(property, true));
                }
                return true;
            }
            return false;
        }

    }

}
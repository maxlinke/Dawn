using UnityEditor;

namespace GeometryGenerators {

    [CustomEditor(typeof(PlaneGenerator))]
    public class PlaneGeneratorEditor : GeometryGeneratorEditor { 

        SerializedProperty tileModeProp;
        SerializedProperty triModeProp;

        protected override void OnEnable () {
            base.OnEnable();
            tileModeProp = serializedObject.FindProperty("tileMode");
            triModeProp = serializedObject.FindProperty("triMode");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            switch(property.name){
                case "triMode":
                    if(tileModeProp.enumValueIndex == (int)(PlaneGenerator.TileMode.Quads)){
                        EditorGUILayout.PropertyField(property, true);
                    }
                    return true;
                case "triSeed":
                    if(tileModeProp.enumValueIndex == (int)(PlaneGenerator.TileMode.Quads) && triModeProp.enumValueIndex == (int)(PlaneGenerator.TriMode.Random)){
                        EditorTools.DrawIndented(() => EditorGUILayout.PropertyField(property, true));
                    }
                    return true;
                default:
                    return false;
            }
        }

    }

}
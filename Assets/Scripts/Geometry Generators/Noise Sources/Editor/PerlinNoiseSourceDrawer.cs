using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(PerlinNoiseSource))]
    public class PerlinNoiseSourceDrawer : NoiseSourceDrawer {

        protected override int AdditionalPropLines => 0;

        protected override void DrawAdditionalProperty (int index, SerializedProperty property) { }
        
    }

}
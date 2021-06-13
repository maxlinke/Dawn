using UnityEditor;

namespace GeometryGenerators {

    [CustomPropertyDrawer(typeof(TextureNoiseSource))]
    public class TextureNoiseSourceDrawer : NoiseSourceDrawer {

        protected override int AdditionalPropLines => 1;

        protected override void DrawAdditionalProperty (int index, SerializedProperty property) {
            DoubleProp("texture", "Texture", "filterSize", "Filter Size");
        }

    }

}
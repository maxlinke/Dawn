using System.Collections.Generic;
using UnityEditor;

public static class SerializedPropertyExtension {

    public static IEnumerable<SerializedProperty> IterateOverVisibleChildren (this SerializedProperty property) {
        var startDepth = property.depth;
        property.NextVisible(true);
        while(property.depth > startDepth){
            yield return property;
            if(!property.NextVisible(false)){
                break;
            }
        }
    }

}
